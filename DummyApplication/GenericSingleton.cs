using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyApplication {
    class GenericSingleton {

        public static GenericType<string> Instance { get; } = new GenericType<string>();
    }
}
