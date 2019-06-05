using System;
using McMaster.Extensions.CommandLineUtils;

namespace Fialka {
    [Command(Description = "Console application showcasing symmetric crypto in .NET Core 3.0")]
    [Subcommand(typeof(EncryptCommand))]
    internal class Program {
        private static void Main(string[] args) {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Fialka.net version {0} | https://github.com/ridercz/Fialka.net", version);
            Console.WriteLine("(c) Michal A. Valasek - Altairis, 2019 | www.rider.cz | www.altairis.cz");
            Console.WriteLine();
            CommandLineApplication.Execute<Program>(args);
        }

        public int OnExecute(CommandLineApplication app) {
            app.ShowHelp();
            return 1;
        }
    }
}
