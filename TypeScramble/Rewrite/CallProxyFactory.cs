using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Rewrite {
    class CallProxyFactory : IFactory {

        public static readonly CallProxyFactory Instance = new CallProxyFactory();

        private Dictionary<int, List<IMethodDefOrRef>> callReferences = new Dictionary<int, List<IMethodDefOrRef>>();
        private Dictionary<int, MethodDef> callFactories = new Dictionary<int, MethodDef>();

        public IEnumerable<MethodDef> FactoryMethods => callFactories.Values;

        public void AddMethodReference(MemberRef m) {
            var p = m.MethodSig.Params.Count;
            if (m.HasThis) {
                p++;
            }
            if (!callReferences.ContainsKey(p)) {
                callReferences.Add(p, new List<IMethodDefOrRef>());
            }
            if (!callReferences[p].Contains(m)) {
                callReferences[p].Add(m);
            }
        }


        public void CreateFactories(ITypeService service, ModuleDef module) {
            callFactories.Clear();
            service.Context.Logger.DebugFormat("factories {0}", callReferences.Count);
            foreach(var kv in callReferences) {
                callFactories.Add(kv.Key, CreateFactory(service, module, kv.Key, kv.Value));
            }

        }

        private MethodDef CreateFactory(ITypeService service, ModuleDef module, int paramNumber, IList<IMethodDefOrRef> methods) {

            var declaringTypeGeneric = new GenericParamUser(0, GenericParamAttributes.NoSpecialConstraint, "t");
            var declaringTypeGenericMVar = new GenericMVar(0);

            var pGenericTypeSpecs = Enumerable.Range(1, paramNumber).Select((x) => new TypeSpecUser(new GenericMVar(x))).ToArray();

            var returnGeneric = new GenericMVar(paramNumber+1);//last generic is return type

            var typeSpec = new TypeSpecUser(declaringTypeGenericMVar);

            var local = new Local(declaringTypeGenericMVar);
            var rtHandle = new Local(module.Import(typeof(RuntimeTypeHandle)).ToTypeSig());


            var methodSig = new MethodSig(CallingConvention.Default, 1,
                returnGeneric,
                 //Method index
                Enumerable.Range(1, paramNumber)
                .Select(x => new GenericMVar(x))
                .Concat(new TypeSig[] { module.CorLibTypes.UInt32 }) //Index
                .ToArray());

            var method = new MethodDefUser("call", methodSig, MethodAttributes.Static);
            method.GenericParameters.Add(declaringTypeGeneric);

            for (ushort genericNum = 1; genericNum < paramNumber+2 /*declare type / return type  */; genericNum++) {
                method.GenericParameters.Add(new GenericParamUser(genericNum, GenericParamAttributes.NoSpecialConstraint, "p" + genericNum.ToString()));
            }

            var gettype = typeof(Type).GetMethod("GetTypeFromHandle");
            var comparetypes = typeof(Type).GetMethod("op_Equality");


            var i = new List<Instruction>();
            i.Add(Instruction.Create(OpCodes.Ldtoken, typeSpec));
            i.Add(Instruction.Create(OpCodes.Call, module.Import(gettype)));
            i.Add(Instruction.Create(OpCodes.Stloc, rtHandle));

            var retDef = new Instruction[]{
                Instruction.Create(OpCodes.Ldloca_S, local),
                Instruction.Create(OpCodes.Initobj, new TypeSpecUser(returnGeneric)),
                Instruction.Create(OpCodes.Ldloc, local),
                Instruction.Create(OpCodes.Ret),
            };
            foreach (var mr in methods) {

                Instruction endjump = Instruction.Create(OpCodes.Nop);

                //Calling type
                i.Add(Instruction.Create(OpCodes.Ldloc, rtHandle));
                i.Add(Instruction.Create(OpCodes.Ldtoken, mr.DeclaringType));
                i.Add(Instruction.Create(OpCodes.Call, module.Import(gettype)));
                i.Add(Instruction.Create(OpCodes.Call, module.Import(comparetypes)));
                i.Add(Instruction.Create(OpCodes.Brfalse_S, endjump));

                //method index

                i.Add(Instruction.Create(OpCodes.Ldarg, new Parameter(paramNumber)));
                i.Add(Instruction.Create(OpCodes.Ldc_I4, GetIndexOfMethodInDeclaringType(mr)));
                i.Add(Instruction.Create(OpCodes.Ceq));
                i.Add(Instruction.Create(OpCodes.Brfalse_S, endjump));

                //params
                for (int index = 0; index < mr.MethodSig.Params.Count; index++) {
                    i.Add(Instruction.Create(OpCodes.Ldtoken, pGenericTypeSpecs[index]));
                    i.Add(Instruction.Create(OpCodes.Call, module.Import(gettype)));
                    i.Add(Instruction.Create(OpCodes.Ldtoken, new TypeSpecUser(mr.MethodSig.Params[index])));
                    i.Add(Instruction.Create(OpCodes.Call, module.Import(gettype)));
                    i.Add(Instruction.Create(OpCodes.Call, module.Import(comparetypes)));
                    i.Add(Instruction.Create(OpCodes.Brfalse_S, endjump));
                }
                
                for (int index = 0; index < paramNumber; index++) {
                    i.Add(Instruction.Create(OpCodes.Ldarg, new Parameter(index)));
                }

                i.Add(Instruction.Create(OpCodes.Call, mr));
                if (mr.MethodSig.RetType != module.CorLibTypes.Void) {
                    i.Add(Instruction.Create(OpCodes.Ret));
                } else {
                   // i.AddRange(retDef);
                }

                i.Add(endjump);
            }
            i.AddRange(retDef);

          

            method.Body = new CilBody(true, i, new ExceptionHandler[0], new Local[] { local, rtHandle });
            //  method.Body.KeepOldMaxStack = true;
            return method;
        }

        public int GetIndexOfMethodInDeclaringType(IMethodDefOrRef mr) {
            var methods = callReferences[mr.MethodSig.Params.Count];
            int index = 0;
            foreach (var m in methods) {
                if (mr.DeclaringType == m.DeclaringType) {
                    index++;
                }
                if(m == mr) {
                    break;
                }
            }
            return index;
        }

        public MethodDef GetFactory(int numberOfParams) {
            MethodDef m;
            callFactories.TryGetValue(numberOfParams, out m);
            return m;
        }

    }
}
