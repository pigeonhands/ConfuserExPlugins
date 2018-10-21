using Confuser.Core;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble {
    class ScramblePhase : ProtectionPhase {
        public override ProtectionTargets Targets => ProtectionTargets.AllDefinitions;

        public override string Name => "Type scrambling";

        public ScramblePhase(TypeScrambleProtection parent)
            : base(parent) { }

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters) {

            var service = (TypeService)context.Registry.GetService<ITypeService>();


            foreach(var m in service.TargetMethods) {
                m.CreateGenerics();

                foreach(var g in m.GenericParams) {
                    m.TargetMethod.GenericParameters.Add(g);
                }
            }


            foreach (var method in parameters.Targets.WithProgress(context.Logger).OfType<MethodDef>()) {

                if (method.HasBody) {
                    service.RewriteMethod(method);
                    
                }

            }

        }
    }
}
