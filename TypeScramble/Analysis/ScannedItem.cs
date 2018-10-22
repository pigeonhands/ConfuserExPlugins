using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis {
    abstract class ScannedItem {
        public IList<TypeSig> AssociatedTypes { get; } = new List<TypeSig>();
        public readonly IList<TypeSig> GenericCallTypes = new List<TypeSig>();


        public GenericInstMethodSig GenericSig { get; private set; }
        public IEnumerable<GenericParam> GenericParams => Generics.Values;

        public readonly Dictionary<uint, GenericParam> Generics = new Dictionary<uint, GenericParam>();

        internal const string GenericParamName = "|";

        public abstract MDToken MDToken { get; }

        public abstract void CreateGenerics();
        public abstract void AddGenerticParam(GenericParam param);

        public TypeSig ToGenericIfAvalible(TypeSig t) {

            if (t.ContainsGenericParameter || t.ScopeType == null) {
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


        public void AddAssociation(TypeSig sig) {
            if (sig?.ScopeType != null && !AssociatedTypes.Contains(sig)) {
                AssociatedTypes.Add(sig);
            }
        }
    }
}
