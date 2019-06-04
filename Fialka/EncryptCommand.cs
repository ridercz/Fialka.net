using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fialka {
    [Command(Description = "Encrypt a file using AES", ExtendedHelpText = "The -p and -k options are mutually exclusive. If neither of them is present, user will be asked for password. The -ko option can be used only with -k.")]
    [EncryptCommandValidation]
    public class EncryptCommand {

        [Required, FileExists]
        [Argument(0, "infile", "Input file name")]
        public string InputFileName { get; set; }

        [Option("-o <outfile>", "Output file name, defaults to <infile>.aes", CommandOptionType.SingleValue)]
        [LegalFilePath]
        public string OutputFileName { get; set; }

        [Option("-p <password>", "Encryption password", CommandOptionType.SingleValue)]
        public string Password { get; set; }

        [Option("-k <keyfile>", "Encryption key file name", CommandOptionType.SingleValue)]
        [LegalFilePath]
        public string KeyFile { get; set; }

        [Option("-f", "Force overwrite key file", CommandOptionType.NoValue)]
        public bool KeyFileOverwrite { get; set; }

        [Option("-ad <string>", "Additional authenticated data", CommandOptionType.SingleValue)]
        public string AuthenticatedData { get; set; }

        public int OnExecute(CommandLineApplication app) {
            return 0;
        }
    }
}
