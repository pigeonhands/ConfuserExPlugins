using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis {
    class ScannedType : ScannedItem {
       
        public TypeDef TargetType { get; }

        public override MDToken MDToken => TargetType.MDToken;

        public ScannedType(TypeDef _target) {
            TargetType = _target;
        }

        public override void AddGenerticParam(GenericParam param) => TargetType.GenericParameters.Add(param);

        public override void CreateGenerics() {
            Generics.Clear();
            GenericCallTypes.Clear();

            foreach (TypeSig t in AssociatedTypes) {
                if (!Generics.ContainsKey(t.ScopeType.MDToken.Raw)) {
                    Generics.Add(t.ScopeType.MDToken.Raw,
                        new GenericParamUser(
                            (ushort)(TargetType.GenericParameters.Count + Generics.Count()),
                            GenericParamAttributes.NoSpecialConstraint, GenericParamName)); //gen name
                    GenericCallTypes.Add(t);
                }
            }
        }

    }
}
