using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyApplication {
    class GenericType<T> {

        class NestedGeneric<T2> {
            public T type1;
            public T2 type2;
            public NestedGeneric() {
                type1 = default(T);
                type2 = default(T2);
            }
        }


        NestedGeneric<T> nextedType;

        public GenericType() {
            nextedType = new NestedGeneric<T>();

            if(typeof(T) == typeof(string) && typeof(T) == typeof(int)) {
                Console.WriteLine("impossable");
            }
        }


        public T GetValue() {
            return nextedType.type1;
        }

    }
}
