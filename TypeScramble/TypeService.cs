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
        IEnumerable<ScannedItem> Targets { get; }

        void AnalizeMethod(MethodDef m);
        void AddAssociatedType(MethodDef m, TypeSig t);
        ScannedItem GetScannedItem(IMemberRef m);

    }

    class TypeService : ITypeService {

        public IEnumerable<ScannedItem> Targets => scannedItems;


        private readonly ConfuserContext context;

        private readonly List<ScannedItem> scannedItems = new List<ScannedItem>();
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
        public void AddAssociatedType(TypeDef type, TypeSig t) {
            return; //Broken
            if (type.IsAbstract || type.IsInterface || type.IsNested) {
                return;
            }

            var si = GetScannedItem(type);
            if(si == null) {
                si = new ScannedType(type);
                scannedItems.Add(si);
            }
            si.AddAssociation(t);

        }

        public void AddAssociatedType(MethodDef m, TypeSig t) {
            if (t.IsGenericInstanceType || t.IsGenericTypeParameter) {
                return;
            }

            var si = GetScannedItem(m);
            if (si == null) {
                si = new ScannedMethod(m);
                scannedItems.Add(si);
            }

            si.AddAssociation(t);
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

        public ScannedItem GetScannedItem(IMemberRef m) => Targets.FirstOrDefault(x => x.MDToken == m.MDToken);
    }
}
