using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
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

            /*
            var entry = context.CurrentModule.EntryPoint;
            if(entry != null) {
                entry.Name = "_start";
                var newEntry = new MethodDefUser("Main", entry.MethodSig, entry.ImplAttributes, entry.Attributes);
                newEntry.Body = new dnlib.DotNet.Emit.CilBody(false, new Instruction[]{
                    Instruction.Create(OpCodes.Call, newEntry),
                }, null, new LocalList());
                entry.DeclaringType.Methods.Add(newEntry);
                context.CurrentModule.EntryPoint = newEntry;
                
            }
            */

            foreach(var m in service.TargetMethods) {
                m.CreateGenerics();

                foreach(var g in m.GenericParams) {
                    m.TargetMethod.GenericParameters.Add(g);
                }

                foreach (var v in m.TargetMethod.Body.Variables) {
                    v.Type = m.ToGenericIfAvalible(v.Type);
                }

                m.TargetMethod.ReturnType = m.ToGenericIfAvalible(m.TargetMethod.ReturnType);
            }


            foreach (var method in parameters.Targets.WithProgress(context.Logger).OfType<MethodDef>()) {

                if (!method.HasBody) {
                    return;
                }

                service.RewriteMethodInstructions(method);

            }

        }
    }
}
