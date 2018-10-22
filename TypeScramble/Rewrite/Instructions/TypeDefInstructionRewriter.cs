using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScramble.Analysis;

namespace TypeScramble.Rewrite.Instructions {
    class TypeDefInstructionRewriter : InstructionRewriter<TypeDef> {
        public override void ProcessOperand(ITypeService service, MethodDef method, IList<Instruction> body, ref int index, TypeDef operand) {


            var currentMethod = service.GetScannedItem(method);
            var targetType = service.GetScannedItem(operand);

            if (targetType != null) {
                var typeSigList = targetType.GenericCallTypes.Select(x => currentMethod?.ToGenericIfAvalible(x) ?? x).ToArray();
                new TypeSpecUser(new GenericInstSig(new ClassSig(operand), typeSigList));
            }

            return;

            //Broken
            ScannedItem current = service.GetScannedItem(method);
            if(current != null) {
                body[index].Operand = new TypeSpecUser(current.ToGenericIfAvalible(operand.ToTypeSig()));
            }

        }
    }
}
