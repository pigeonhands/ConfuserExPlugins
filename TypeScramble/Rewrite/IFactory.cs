using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Rewrite {
    interface IFactory {
        IEnumerable<MethodDef> FactoryMethods { get; }
        void CreateFactories(ITypeService service, ModuleDef module);
        MethodDef GetFactory(int numberOfParams);

        void Reset();
    }
}
