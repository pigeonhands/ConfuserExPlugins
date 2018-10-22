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
            method.Body.KeepOldMaxStack = true;
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

            if (body[index].OpCode == OpCodes.Newobj) { //object creation factory
                if(operand.MethodSig.Params.Count > 0) { return; } //Not supporeted yet

                var typeSig = operand.DeclaringType.ToTypeSig();
                var currentMethod = service.GetScannedItem(method);
                if (currentMethod != null) {
                    typeSig = currentMethod.ToGenericIfAvalible(typeSig);
                }

                body[index].OpCode = OpCodes.Call;
                body[index].Operand = new MethodSpecUser(ObjectCreationFactory.Instance.GetCreationMethod(0), new GenericInstMethodSig(typeSig));

                return;
            }

            return;

            //Proxy reroute
            var proxy = CallProxyFactory.Instance.GetFactory(operand.MethodSig.Params.Count + (operand.MethodSig.HasThis ? 1 : 0));
            var sigs = new List<TypeSig>();
            sigs.Add(operand.DeclaringType.ToTypeSig());

            if (operand.MethodSig.HasThis) {
                sigs.Add(operand.DeclaringType.ToTypeSig());
            }

            foreach (var p in operand.MethodSig.Params) {
                if(p is GenericVar) {
                    sigs.Add((GenericVar)p);
                } else {
                    sigs.Add(p.ScopeType.ToTypeSig());
                }
            }

            if (operand.MethodSig.RetType == operand.Module.CorLibTypes.Void) {
                sigs.Add(operand.Module.CorLibTypes.Object);
            }
            body.Insert(index++, Instruction.CreateLdcI4(CallProxyFactory.Instance.GetIndexOfMethodInDeclaringType(operand)));

            body[index].Operand = new MethodSpecUser(proxy, new GenericInstMethodSig(sigs));
            if (operand.MethodSig.RetType == operand.Module.CorLibTypes.Void) {
                body.Insert(++index, Instruction.Create(OpCodes.Pop));
            }

        }
    }
}
