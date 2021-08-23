using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentStore.FullTrust
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Fluent Store";
            Console.WriteLine("This process has access to the entire public desktop API surface");
            Console.WriteLine("Press any key to exit ...");
            Console.ReadLine();
        }
    }
}
