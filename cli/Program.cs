using System;
using System.Threading.Tasks;

namespace tellick_admin.Cli {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Please provide a command");
                return;
            }

            Cli cli = new Cli();
            cli.ParseAndRun(args).Wait();
        }
    }
}
