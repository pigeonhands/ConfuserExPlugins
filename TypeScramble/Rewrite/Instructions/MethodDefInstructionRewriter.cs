using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Rewrite.Instructions {
    class MethodDefInstructionRewriter : InstructionRewriter<MethodDef> {
        public override void ProcessOperand(ITypeService service, MethodDef method, IList<Instruction> body, ref int index, MethodDef operand) {

            var currentMethod = service.GetScannedItem(method);
            var targetMethod = service.GetScannedItem(operand);

            /* //Type field scrabmble
            var targetType = service.GetScannedItem(operand.DeclaringType);
            if (targetType != null) {
                var typeSigList = targetType.GenericCallTypes.Select(x => currentMethod?.ToGenericIfAvalible(x) ?? x).ToArray();
                new TypeSpecUser(new GenericInstSig(new ClassSig(operand.DeclaringType), typeSigList));
            }
            */

            if (body[index].OpCode == OpCodes.Newobj) {
                FactoryHealper.ApplyObjectCreationProxy(service, currentMethod, body, ref index, operand);
            } else {
                FactoryHealper.ApplyCallProxy(service, currentMethod, body, ref index, operand);
            }

            if (targetMethod != null) {
                var typeSigList = targetMethod.GenericCallTypes.Select(x => currentMethod?.ToGenericIfAvalible(x) ?? x).ToArray();
                body[index].Operand = new MethodSpecUser(operand, new GenericInstMethodSig(typeSigList));
            }

        }
    }
}
