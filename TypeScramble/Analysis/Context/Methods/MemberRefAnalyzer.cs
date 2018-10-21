using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis.Context.Methods {
    class MemberRefAnalyzer : MethodContextAnalyzer<MemberRef> {

        public override void Process(ITypeService service, MethodDef m, MemberRef o) {
        
            TypeSig sig = null;

            if (o.Class is TypeRef) {
                sig = (o.Class as TypeRef)?.ToTypeSig();
            }
            if (o.Class is TypeSpec) {
                sig = (o.Class as TypeSpec)?.ToTypeSig();
            }
            if (sig != null) {
                service.AddAssociatedType(m, sig);
            }
        }
    }
}
