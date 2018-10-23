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
        ConfuserContext Context { get; }

        void AnalizeMethod(MethodDef m);
        void AddAssociatedType(IMemberRef m, TypeSig t);

        ScannedItem GetScannedItem(IMemberRef m);
        bool ShouldModify(MethodDef m);

        void RewriteMethodInstructions(MethodDef m);
    }

    class TypeService : ITypeService {

        public IEnumerable<ScannedItem> Targets => scannedItems;

        public ConfuserContext Context { get; }


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
            this.Context = _context;

        }

        public void AddAssociatedType(IMemberRef type, TypeSig associatedType) {
            if (associatedType.IsGenericInstanceType || associatedType.IsGenericTypeParameter) {
                return;
            }

            var si = GetScannedItem(type);
            if (si == null) {

                switch (type) {
                    case MethodDef m:
                        si = new ScannedMethod(m);
                        break;

                    case TypeDef t:
                       // si = new ScannedType(t); //types are currently Broken
                        break;

                    default:
                        throw new ArgumentException($"AddAssociatedType type must be either MethodDef or TypeDef", "type");
                }
                
                if(si == null) {
                    return; //Should never happen in a working version. More for testing
                }

                scannedItems.Add(si);
            }

            si.AddAssociation(associatedType);
        }

        public void AnalizeMethod(MethodDef m) {
            foreach(Instruction i in m.Body.Instructions) {
                if (i.Operand == null) {
                    continue;
                }

                var operandType = i.Operand.GetType().BaseType;

                foreach (MethodContextAnalyzer c in methodAnalyzers.Where(x => x.TargetType == operandType)){
                    c.ProcessOperand(this, m, i);
                    Context.CheckCancellation();
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
                    Context.CheckCancellation();
                }
            }
        }

        public ScannedItem GetScannedItem(IMemberRef m) => Targets.FirstOrDefault(x => x.MDToken == m.MDToken);

        public bool ShouldModify(MethodDef m) 
            => m.HasBody && !( m.IsAbstract || m.IsVirtual || m.IsConstructor || m.IsGetter || m.HasOverrides);
    }
}
