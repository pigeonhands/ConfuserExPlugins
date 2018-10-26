using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScramble.Analysis;

namespace TypeScramble.Rewrite {
    internal static class FactoryHealper {

        public static void ApplyObjectCreationProxy(ITypeService service, ScannedItem parentItem, IList<Instruction> body, ref int index, IMethodDefOrRef operand) {
            if (operand.MethodSig.Params.Count > 0) { return; } //Not supporeted yet

            var proxyMethod = ObjectCreationFactory.Instance.GetCreationMethod(operand.MethodSig.Params.Count);
            if (proxyMethod == null) {
                return; //No factory for this parameter number (probaby disabled)
            }

            var typeSig = operand.DeclaringType.ToTypeSig();
            if (parentItem != null) {
                typeSig = parentItem.ToGenericIfAvalible(typeSig);
            }

            body[index].OpCode = OpCodes.Call;
            body[index].Operand = new MethodSpecUser(proxyMethod, new GenericInstMethodSig(typeSig));
        }

        public static void ApplyCallProxy(ITypeService service, ScannedItem parentItem, IList<Instruction> body, ref int index, IMethodDefOrRef operand) {
            var proxyMethod = CallProxyFactory.Instance.GetFactory(operand.MethodSig.Params.Count + (operand.MethodSig.HasThis ? 1 : 0));
            if (proxyMethod == null) {
                return; //No factory for this parameter number (probaby disabled)
            }

            var sigs = new List<TypeSig>();
            sigs.Add(operand.DeclaringType.ToTypeSig());

            if (operand.MethodSig.HasThis) {
                sigs.Add(operand.DeclaringType.ToTypeSig());
            }

            foreach (var p in operand.MethodSig.Params) {
                if (p is GenericVar) {
                    sigs.Add((GenericVar)p);
                } else {
                    sigs.Add(p.ScopeType.ToTypeSig());
                }
            }

            if (operand.MethodSig.RetType == operand.Module.CorLibTypes.Void) {
                sigs.Add(operand.Module.CorLibTypes.Object);
            }
            body.Insert(index++, Instruction.CreateLdcI4(CallProxyFactory.Instance.GetIndexOfMethodInDeclaringType(operand)));

            if (parentItem != null) {
                sigs = sigs.Select(x => parentItem.ToGenericIfAvalible(x)).ToList();
            }

            body[index].Operand = new MethodSpecUser(proxyMethod, new GenericInstMethodSig(sigs));
            if (operand.MethodSig.RetType == operand.Module.CorLibTypes.Void) {
                body.Insert(++index, Instruction.Create(OpCodes.Pop));
            }
        }

    }
}
