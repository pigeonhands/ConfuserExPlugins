using Confuser.Core;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScramble.Analysis;
using TypeScramble.Rewrite;

namespace TypeScramble {
    class ScramblePhase : ProtectionPhase {
        public override ProtectionTargets Targets => ProtectionTargets.Methods;

        public override string Name => "Type scrambling";

        public ScramblePhase(TypeScrambleProtection parent)
            : base(parent) { }

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters) {

            var service = context.Registry.GetService<ITypeService>();


            var objectFactory = new TypeDefUser("factory", context.CurrentModule.GlobalType);
            CreateFactories(objectFactory, service, context, service.Factories);
            context.CurrentModule.Types.Add(objectFactory);

            ProtectionParameters.SetParameters(context, objectFactory, new ProtectionSettings());

            //Apply new generic signatures for targets
            foreach (ScannedItem item in service.Targets) {
                SetGenericsForItem(item); 
            }

            //Create a new Main method so the original can bennifit from typescambling
            RerouteEntrypoint(service, context); //If this is removed, either remove the metrypoint from the scanned items or add a check in the Analize phase

            //Modify calls/references so that they work with new signatures
            foreach (var method in parameters.Targets.WithProgress(context.Logger).OfType<MethodDef>()) {
                if (!method.HasBody) {
                    return;
                }
                service.RewriteMethodInstructions(method);
            }
        }

        private void RerouteEntrypoint(ITypeService service, ConfuserContext context) {
            var originalEntry = context.CurrentModule.EntryPoint;
            if (originalEntry != null) {
                originalEntry.Name = "_start";

                var param = originalEntry.Parameters.FirstOrDefault()?.Type;


                var newEntry = new MethodDefUser("Main",
                    originalEntry.MethodSig,
                    originalEntry.ImplAttributes, originalEntry.Attributes);

                IMethod callSig = originalEntry;
                var scannedEntry = service.GetScannedItem(originalEntry);
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

        }

        private void SetGenericsForItem(ScannedItem target) {
            target.CreateGenerics();

            foreach (var g in target.GenericParams) {
                target.AddGenerticParam(g);
            }

            switch (target) {
                case ScannedMethod m:

                    foreach (var v in m.TargetMethod.Body.Variables) {
                        v.Type = m.ToGenericIfAvalible(v.Type);
                    }

                    m.TargetMethod.ReturnType = target.ToGenericIfAvalible(m.TargetMethod.ReturnType);
                    break;


                case ScannedType t:
                    /*
                    foreach(var f in t.TargetType.Fields) {
                        f.FieldType = target.ToGenericIfAvalible(f.FieldType);
                    }
                    */

                    break;
            }
        }

        private void CreateFactories(TypeDefUser factpryParentClass, ITypeService service, ConfuserContext context, IFactory[] factories) {

            foreach(IFactory factory in factories) {
                factory.CreateFactories(service, context.CurrentModule);

                foreach(var generatedFactoryMethod in factory.FactoryMethods) {
                    factpryParentClass.Methods.Add(generatedFactoryMethod);
                    ProtectionParameters.SetParameters(context, generatedFactoryMethod, new ProtectionSettings());
                }
            }

        }
    }
}
