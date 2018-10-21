using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScramble.Analysis;
using TypeScramble.Analysis.Context;
using TypeScramble.Analysis.Context.Methods;
using TypeScramble.Rewrite.Instructions;

namespace TypeScramble {
    interface ITypeService {
        IEnumerable<ScannedMethod> TargetMethods { get; }

        void AnalizeMethod(MethodDef m);
        void AddAssociatedType(MethodDef m, TypeSig t);

    }

    class TypeService : ITypeService {

        public IEnumerable<ScannedMethod> TargetMethods => scannedMethods;


        private readonly ConfuserContext context;

        private readonly List<ScannedMethod> scannedMethods = new List<ScannedMethod>();
        private readonly MethodContextAnalyzer[] methodAnalyzers = new MethodContextAnalyzer[] {
            new MemberRefAnalyzer(),
            new MethodDefAnalyzer(),
            new MethodSpecAnalyzer(),
            new TypeRefAnalyzer(),
        };

        private readonly InstructionRewriter[] instructionRewriters = new InstructionRewriter[] {
            new MemberRefInstructionRewriter(),
            new MethodDefInstructionRewriter(),
        };

        public TypeService( ConfuserContext _context) {
            this.context = _context;

        }


        public void AddAssociatedType(MethodDef m, TypeSig t) {
            var sm = scannedMethods.FirstOrDefault(x => x.TargetMethod == m);
            if(sm == null) {
                sm = new ScannedMethod(m);
                scannedMethods.Add(sm);
            }
           // if (t.IsSingleOrMultiDimensionalArray) {
            //    var arraySig = t as SZArraySig;
           // }
            sm.AddAssociation(t);
        }

        public void AnalizeMethod(MethodDef m) {

            foreach(Instruction i in m.Body.Instructions) {
                if (i.Operand == null) {
                    continue;
                }

                var operandType = i.Operand.GetType().BaseType;


                foreach (MethodContextAnalyzer c in methodAnalyzers.Where(x => x.TargetType == operandType)){
                    c.ProcessOperand(this, m, i.Operand);
                }
            }

        }


        public void RewriteMethod(MethodDef m) {
            var instructions = m.Body.Instructions;

            for (int i = 0; i < instructions.Count; i++) {
                Instruction inst = instructions[i];
                if(inst.Operand == null) {
                    continue;
                }
                var operandType = inst.Operand.GetType().BaseType;
                foreach (InstructionRewriter ir in instructionRewriters.Where(x => x.TargetType == operandType)) {
                    ir.ProcessInstruction(this, m, instructions, ref i, inst);
                }
            }
        }

    }
}
