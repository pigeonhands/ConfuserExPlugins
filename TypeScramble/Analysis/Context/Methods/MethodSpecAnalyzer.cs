using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis.Context.Methods {
    class MethodSpecAnalyzer : MethodContextAnalyzer<MethodSpec> {
        public override void Process(ITypeService service, MethodDef m, MethodSpec o) {
            foreach (var t in o.GenericInstMethodSig.GenericArguments) {
                service.AddAssociatedType(m, t);
            }
        }
    }
}
