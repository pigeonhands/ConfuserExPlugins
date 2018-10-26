using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScramble.Analysis;

namespace TypeScramble.Rewrite.Instructions {
    class MemberRefInstructionRewriter : InstructionRewriter<MemberRef> {
        public override void ProcessOperand(ITypeService service, MethodDef method, IList<Instruction> body, ref int index, MemberRef operand) {

            if (operand.MethodSig == null)
                return;

            var currentMethod = service.GetScannedItem(method);

            if (body[index].OpCode == OpCodes.Newobj) {
                FactoryHealper.ApplyObjectCreationProxy(service, currentMethod, body, ref index, operand);
            } else {
                FactoryHealper.ApplyCallProxy(service, currentMethod, body, ref index, operand);
            }

        }

       
    }
}
