using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis {
    internal class ScannedMethod {

        public IList<TypeSig> AssociatedTypes { get; } = new List<TypeSig>();
        public readonly IList<TypeSig> GenericCallTypes = new List<TypeSig>();

        public MethodDef TargetMethod { get; }

        public GenericInstMethodSig GenericSig { get; private set; }
        public IEnumerable<GenericParam> GenericParams => Generics.Values;

        public readonly Dictionary<uint, GenericParam> Generics = new Dictionary<uint, GenericParam>();

        public ScannedMethod(MethodDef target) {
            TargetMethod = target;
        }

        public void AddAssociation(TypeSig sig) {
            AssociatedTypes.Add(sig);
        }


        public void CreateGenerics() {
            Generics.Clear();
            GenericCallTypes.Clear();

            foreach (TypeSig t in AssociatedTypes) {
                if (!Generics.ContainsKey(t.ScopeType.MDToken.Raw)) {
                    Generics.Add(t.ScopeType.MDToken.Raw, 
                        new GenericParamUser(
                            (ushort)(TargetMethod.GenericParameters.Count + Generics.Count()), 
                            GenericParamAttributes.NoSpecialConstraint, t.TypeName)); //gen name
                    GenericCallTypes.Add(t);
                }
            }
            
        }

        public TypeSig ToGenericIfAvalible(TypeSig t) {

            if(t.ContainsGenericParameter || t.ScopeType == null) {
                return t;
            }

            GenericParam gp;
            if (!Generics.TryGetValue(t.ScopeType.MDToken.Raw, out gp)) {
                return t;
            }

            TypeSig newSig = new GenericMVar(gp.Number);
            if (t.IsSingleOrMultiDimensionalArray) {
                var arraySig = t as SZArraySig;
                if (arraySig == null || arraySig.IsMultiDimensional) {
                    return t;
                } else {
                    return new ArraySig(newSig, arraySig.Rank);
                }
            }

            return newSig;

        }

    }
}
