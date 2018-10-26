using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScramble.Rewrite;

namespace TypeScramble.Analysis.Context.Methods {
    class MethodDefAnalyzer : MethodContextAnalyzer<MethodDef> {

        public override void ProcessOperand(ITypeService service, MethodDef m, Instruction i) {

            var mr = (MethodDef)i.Operand;
            if (i.OpCode == OpCodes.Newobj) {
                if (mr.MethodSig.Params.Count == 0) {
                    ObjectCreationFactory.Instance.AddObjectReference(mr);

                }
            } else {
                CallProxyFactory.Instance.AddMethodReference(mr);
            }

            Process(service, m, mr);
        }

        public override void Process(ITypeService service, MethodDef m, MethodDef o) {

            var chainCall = service.GetScannedItem(o);
            if(chainCall != null) {
                foreach(var t in chainCall.AssociatedTypes) {
                    service.AddAssociatedType(o, t);
                }
            }

        }
    }
}
