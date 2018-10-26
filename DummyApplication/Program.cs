using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DummyApplication {
    class Program {
        [STAThread]
        static void Main(string[] args) {

            var t = new GenericType<int>();
               Console.WriteLine(t.GetValue());


            var ft = new FlatType();
            Console.WriteLine(ft.GetString());
            Console.WriteLine(ft.Getint());
            Console.WriteLine(Fib(1, 2, 5));

            var inst = GenericSingleton.Instance;
            inst.SetValue("test str");
            Console.WriteLine(inst.GetValue());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new winform());

        }




        static int Fib(int a, int b, int c) {
            if(c == 0) {
                return a + b;
            }
            return Fib(b, a + b, --c);
        }
    }
}
