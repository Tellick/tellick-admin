using System;
using System.Threading.Tasks;

namespace tellick_admin.Cli {
    class Program {
        static void Main(string[] args) {
            TpConfigReaderWriter tpConfigReader = new TpConfigReaderWriter();
            TpConfig tpConfig = tpConfigReader.TpConfig;
            if (String.IsNullOrEmpty(tpConfig.Origin)) {
                Console.WriteLine("Please use 'tp connect' to connect to an origin.");
            }

            if (args.Length == 0) {
                Console.WriteLine("Please provide a command");
                return;
            }

            Cli cli = new Cli(tpConfig);
            cli.ParseAndRun(args).Wait();
        }
    }
}
