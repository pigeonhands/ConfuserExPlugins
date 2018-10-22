using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScramble.Analysis.Context.Methods {
    internal abstract class MethodContextAnalyzer {

        public abstract Type TargetType { get; }
        public abstract void ProcessOperand(ITypeService service, MethodDef m, Instruction i);
    }


    internal abstract class MethodContextAnalyzer<T> : MethodContextAnalyzer {
        public override Type TargetType => typeof(T);
        public abstract void Process(ITypeService service, MethodDef m, T o);
        public override void ProcessOperand(ITypeService service, MethodDef m, Instruction i) {
            Process(service, m, (T)i.Operand);
        }
    }
}