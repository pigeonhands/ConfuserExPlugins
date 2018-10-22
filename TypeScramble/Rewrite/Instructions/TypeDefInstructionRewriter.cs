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

            ScannedMethod current = service.GetScannedMethod(method);
            if(current != null) {
                body[index].Operand = new TypeSpecUser(current.ToGenericIfAvalible(operand.ToTypeSig()));
            }

        }
    }
}
