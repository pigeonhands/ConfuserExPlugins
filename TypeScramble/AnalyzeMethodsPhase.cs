using Confuser.Core;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble {
    class AnalyzeMethodsPhase : ProtectionPhase {

        public AnalyzeMethodsPhase(TypeScrambleProtection parent) : base(parent) {

        }

        public override ProtectionTargets Targets => ProtectionTargets.AllDefinitions;

        public override string Name => "Type analysis";

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters) {

            var service = (TypeService)context.Registry.GetService<ITypeService>();


            foreach (var m in parameters.Targets.WithProgress(context.Logger).OfType<MethodDef>()) {


                if(!m.HasBody || m.Module.EntryPoint == m || m.IsAbstract || m.IsVirtual || m.IsConstructor || m.IsGetter || m.HasOverrides) {
                    continue;
                }

                foreach (var v in m.Body.Variables) {
                    service.AddAssociatedType(m, v.Type);
                }

                if (m.ReturnType != m.Module.CorLibTypes.Void) {
                    service.AddAssociatedType(m, m.ReturnType);
                }

                foreach (var param in m.Parameters) {
                    if (param.Index == 0 && !m.IsStatic) {
                        continue;
                    }
                    service.AddAssociatedType(m, param.Type);
                }

                service.AnalizeMethod(m);

            }

        }
    }
}
