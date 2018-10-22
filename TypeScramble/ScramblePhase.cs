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


            foreach (var m in service.TargetMethods) {
                m.CreateGenerics();

                foreach(var g in m.GenericParams) {
                    m.TargetMethod.GenericParameters.Add(g);
                }

                foreach (var v in m.TargetMethod.Body.Variables) {
                    v.Type = m.ToGenericIfAvalible(v.Type);
                }

                m.TargetMethod.ReturnType = m.ToGenericIfAvalible(m.TargetMethod.ReturnType);
            }

            //Reroute entrypoint
            var originalEntry = context.CurrentModule.EntryPoint;
            if (originalEntry != null) {
                originalEntry.Name = "_start";

                var param = originalEntry.Parameters.FirstOrDefault()?.Type;


                var newEntry = new MethodDefUser("Main",
                    //new MethodSig(originalEntry.CallingConvention, 0, originalEntry.ReturnType, param == null ? new TypeSig[0] : new TypeSig[] { param }), 
                    originalEntry.MethodSig,
                    originalEntry.ImplAttributes, originalEntry.Attributes);

                IMethod callSig = originalEntry;
                var scannedEntry = service.GetScannedMethod(originalEntry);
                if (scannedEntry != null) {
                    callSig = new MethodSpecUser(originalEntry, new GenericInstMethodSig(scannedEntry.GenericCallTypes.ToArray()));
                }


                newEntry.Body = new dnlib.DotNet.Emit.CilBody(false, new Instruction[]{
                    Instruction.Create(param == null ? OpCodes.Nop : OpCodes.Ldarg_0),
                    Instruction.Create(OpCodes.Call, callSig),
                    Instruction.Create(OpCodes.Ret),
                }, new ExceptionHandler[0], new LocalList());

                originalEntry.DeclaringType.Methods.Add(newEntry);
                context.CurrentModule.EntryPoint = newEntry;
                ProtectionParameters.SetParameters(context, newEntry, ProtectionParameters.GetParameters(context, originalEntry));

                service.RewriteMethodInstructions(newEntry);
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
