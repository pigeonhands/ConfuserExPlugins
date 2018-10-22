using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Rewrite.Instructions {
    class MemberRefInstructionRewriter : InstructionRewriter<MemberRef> {
        public override void ProcessOperand(ITypeService service, MethodDef method, IList<Instruction> body, ref int index, MemberRef operand) {

            /*
           * Testing
           * 
           * 
           var current = service.GetScannedMethod(method);
           if (current != null) {
               var c = operand.Class;
               new MemberRefUser(operand.Module, operand.Name, new MethodSig());
               operand.Class = new ClassSig(current.ToGenericIfAvalible(operand.DeclaringType.ToTypeSig()).ToTypeDefOrRef()).TypeDefOrRef;

           }
           */

            if (operand.MethodSig == null)
                return;

            if (operand.MethodSig.Params.Count > 0  || body[index].OpCode != OpCodes.Newobj) {
                return;
            }

            var typeSig = operand.DeclaringType.ToTypeSig();
            var currentMethod = service.GetScannedItem(method);
            if(currentMethod != null) {
                typeSig = currentMethod.ToGenericIfAvalible(typeSig);
            }

            body[index].OpCode = OpCodes.Call;
            body[index].Operand = new MethodSpecUser(ObjectCreationFactory.Instance.GetCreationMethod(0), new GenericInstMethodSig(typeSig));

        }
    }
}
