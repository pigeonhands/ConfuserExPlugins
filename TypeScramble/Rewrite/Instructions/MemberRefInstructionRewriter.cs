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

            if (operand.MethodSig == null)
                return;

            if (operand.MethodSig.Params.Count > 0  || body[index].OpCode != OpCodes.Newobj) {
                return;
            }
                
            // :TODO

            //Object creation factory

        }
    }
}
