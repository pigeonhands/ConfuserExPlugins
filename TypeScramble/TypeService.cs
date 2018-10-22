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
        ScannedMethod GetScannedMethod(IMethod m);

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
            new TypeDefInstructionRewriter(),
          //  new TypeRefInstructionRewriter(),
        };

        public TypeService( ConfuserContext _context) {
            this.context = _context;

        }


        public void AddAssociatedType(MethodDef m, TypeSig t) {
            if (t.IsGenericInstanceType || t.IsGenericTypeParameter) {
                return;
            }
             
            var sm = GetScannedMethod(m);
            if(sm == null) {
                sm = new ScannedMethod(m);
                scannedMethods.Add(sm);
            }
            if (t.IsSingleOrMultiDimensionalArray) {
                context.Logger.DebugFormat("{0} -> {1}", m.Name, t.FullName);
            }
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


        public void RewriteMethodInstructions(MethodDef m) {
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

        public ScannedMethod GetScannedMethod(IMethod m) => TargetMethods.FirstOrDefault(x => x.TargetMethod.MDToken == m.MDToken);
    }
}
