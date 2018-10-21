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

            var currentMethod = service.TargetMethods.FirstOrDefault(x => x.TargetMethod.MDToken == method.MDToken);
            var targetMethod = service.TargetMethods.FirstOrDefault(x => x.TargetMethod.MDToken == operand.MDToken);


            if (targetMethod != null && currentMethod != null) {
                var typeSigList = targetMethod.GenericCallTypes.Select(currentMethod.ToGenericIfAvalible).ToArray();
                body[index].Operand = new MethodSpecUser(operand, new GenericInstMethodSig(typeSigList));
            }

        }
    }
}
