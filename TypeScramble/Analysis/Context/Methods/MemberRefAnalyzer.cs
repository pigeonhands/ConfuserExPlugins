using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis.Context.Methods {
    class MemberRefAnalyzer : MethodContextAnalyzer<MemberRef> {

        public override void ProcessOperand(ITypeService service, MethodDef m, Instruction i) {

            var mr = (MemberRef)i.Operand;
            if(i.OpCode == OpCodes.Newobj && mr.MethodSig.Params.Count == 0) {
                service.AddObjectReference(mr);
            }

            Process(service, m, mr);
        }
        public override void Process(ITypeService service, MethodDef m, MemberRef o) {

            var tr = (o.Class as ITypeDefOrRef);
            TypeSig sig = tr?.ToTypeSig();
            if (sig == null) {
                return;
            }
            
            service.AddAssociatedType(m, sig);
        }
    }
}
