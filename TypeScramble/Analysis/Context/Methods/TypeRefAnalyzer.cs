using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis.Context.Methods {
    class TypeRefAnalyzer : MethodContextAnalyzer<TypeRef> {
        public override void Process(ITypeService service, MethodDef m, TypeRef o) {
            service.AddAssociatedType(m, o.ToTypeSig());
        }
    }
}
