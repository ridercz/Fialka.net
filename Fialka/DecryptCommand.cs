using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Fialka {
    [Command(Description = "Decrypts a file using AES-GCM.")]
    public class DecryptCommand {
        EncryptedFile inputFile;
        byte[] key;

        // Command options

        [Required, FileExists]
        [Argument(0, "infile", "Input file name")]
        public string InputFileName { get; set; }

        [Option("-o <outfile>", "Output file name (default to <infile> without extension)", CommandOptionType.SingleValue)]
        [LegalFilePath]
        public string OutputFileName { get; set; }

        [Option("-p <password>", "Decryption password", CommandOptionType.SingleValue)]
        public string Password { get; set; }

        [Option("-k <keyfile>", "Encryption key file name", CommandOptionType.SingleValue)]
        [FileExists]
        public string KeyFile { get; set; }

        // Command action

        public int OnExecute(CommandLineApplication app) {
            // Read data file
            Console.Write($"Reading encrypted file {this.InputFileName}...");
            var json = File.ReadAllText(this.InputFileName);
            this.inputFile = JsonSerializer.Parse<EncryptedFile>(json);
            Console.WriteLine("OK");

            // Get key
            if (this.inputFile.PasswordOptions is null) {
                // Keyfile requested
                if (string.IsNullOrEmpty(this.KeyFile)) {
                    Console.WriteLine("This file was encrypted using key file. Please specify it using -k option.");
                    return 0;
                }

                // Read key from file
                Console.Write($"Reading key from {this.KeyFile}...");
                this.key = File.ReadAllBytes(this.KeyFile);
                var keyLength = this.key.Length * 8;
                if (keyLength != 128 && keyLength != 256) {
                    Console.WriteLine("Failed!");
                    Console.WriteLine($"File has {keyLength} bits, expected 128 or 256.");
                    return 1;
                }
                Console.WriteLine($"OK, {keyLength} bits");
            } else {
                // If password not specified, get key from user
                if (string.IsNullOrEmpty(this.Password)) this.Password = Prompt.GetPassword("Enter decryption password: ");

                // Derive key from password
                Console.Write($"Generating {this.inputFile.PasswordOptions.Length} bit key from password using PBKDF2 with {this.inputFile.PasswordOptions.Iterations} iterations...");
                var pbkdf = new Rfc2898DeriveBytes(this.Password, Convert.FromBase64String(this.inputFile.PasswordOptions.Salt), this.inputFile.PasswordOptions.Iterations);
                this.key = pbkdf.GetBytes(this.inputFile.PasswordOptions.Length / 8);
                Console.WriteLine("OK");
            }

            // Get authenticated data
            var adData = string.IsNullOrEmpty(this.inputFile.AuthenticatedData) ? null : Encoding.UTF8.GetBytes(this.inputFile.AuthenticatedData);

            // Get nonce and tag
            var nonce = Convert.FromBase64String(this.inputFile.Nonce);
            var tag = Convert.FromBase64String(this.inputFile.Tag);
            var cipherData = Convert.FromBase64String(this.inputFile.Data);
            var plainData = new byte[cipherData.Length];

            // Decrypt
            try {
                Console.Write("Decrypting using AES-GCM...");
                using var aes = new AesGcm(this.key);
                aes.Decrypt(nonce, cipherData, tag, plainData, adData);
                Console.WriteLine("OK");
            }
            catch (CryptographicException cex) {
                Console.WriteLine("Failed!");
                Console.WriteLine(cex.Message);
                return 2;
            }

            // Display AD
            if (!string.IsNullOrEmpty(this.inputFile.AuthenticatedData)) {
                Console.WriteLine("Additional authenticated data:");
                Console.WriteLine(this.inputFile.AuthenticatedData);
            }

            // Save to file
            if (string.IsNullOrEmpty(this.OutputFileName)) this.OutputFileName = Path.Combine(Path.GetDirectoryName(this.InputFileName), Path.GetFileNameWithoutExtension(this.InputFileName));
            Console.Write($"Saving decrypted file to {this.OutputFileName}...");
            File.WriteAllBytes(this.OutputFileName, plainData);
            Console.WriteLine("OK");

            return 0;
        }
    }
}
