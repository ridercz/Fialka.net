using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using McMaster.Extensions.CommandLineUtils;

namespace Fialka {
    [Command(Description = "Encrypts a file using AES-GCM.")]
    public class EncryptCommand {
        private byte[] key;
        private EncryptedFile outputFile;

        // Command options

        [Required, FileExists]
        [Argument(0, "infile", "Input file name")]
        public string InputFileName { get; set; }

        [Option("-o <outfile>", "Output file name (default <infile>.fjd)", CommandOptionType.SingleValue)]
        [LegalFilePath]
        public string OutputFileName { get; set; }

        [Option("-a <string>", "Additional authenticated data", CommandOptionType.SingleValue)]
        public string AuthenticatedData { get; set; }

        [Option("-p <password>", "Encryption password", CommandOptionType.SingleValue)]
        public string Password { get; set; }

        [Option("-i <number>", "Number of PBKDF2 password iterations (default 10000)", CommandOptionType.SingleValue)]
        [Range(1000, int.MaxValue)]
        public int PasswordIterations { get; set; } = 10000;

        [Option("-s <bits>", "Length of PBKDF2 password salt (default 256)", CommandOptionType.SingleValue)]
        [Range(128, 1024), DivisibleBy(8)]
        public int PasswordSaltLenght { get; set; } = 256;

        [Option("-k <keyfile>", "Encryption key file name", CommandOptionType.SingleValue)]
        [LegalFilePath]
        public string KeyFile { get; set; }

        [Option("-l <bits>", "Length of generated or derived key (128 or 256, default 256)", CommandOptionType.SingleValue)]
        [Range(128, 256), DivisibleBy(128)]
        public int KeyLength { get; set; } = 256;

        [Option("-f", "Force overwrite key file", CommandOptionType.NoValue)]
        public bool KeyFileOverwrite { get; set; }

        // Command action

        public int OnExecute(CommandLineApplication app) {
            // Prepare output data structure
            this.outputFile = new EncryptedFile();

            // Get encryption key
            if (!string.IsNullOrEmpty(this.KeyFile)) {
                // Get key from file (or create new random)
                if (!this.GetKeyFromFile()) return 1;
            } else if (!string.IsNullOrEmpty(this.Password)) {
                // Derive key from password specified as option
                this.GetKeyFromPassword();
            } else {
                // Derive key from password entered interactively
                var pwd1 = Prompt.GetPassword("Enter encryption password: ");
                if (string.IsNullOrEmpty(pwd1)) {
                    Console.WriteLine("Password cannot be empty.");
                    return 1;
                }
                var pwd2 = Prompt.GetPassword("Confirm encryption password: ");
                if (!pwd1.Equals(pwd2, StringComparison.Ordinal)) {
                    Console.WriteLine("Confirmation does not match the password.");
                    return 1;
                }
                this.Password = pwd1;
                Console.WriteLine();
                this.GetKeyFromPassword();
            }

            // Read data for encryption
            Console.Write($"Reading plaintext file {this.InputFileName}...");
            var plainBytes = File.ReadAllBytes(this.InputFileName);
            Console.WriteLine($"OK, {plainBytes.Length} bytes read");

            // Prepare authenticated data
            var adBytes = string.IsNullOrEmpty(this.AuthenticatedData)
                ? null
                : System.Text.Encoding.UTF8.GetBytes(this.AuthenticatedData);

            // Create 96-bit nonce
            Console.Write("Preparing 96-bit nonce...");
            var nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);
            Console.WriteLine("OK");

            // Prepare arrays in target
            var tag = new byte[16];
            var data = new byte[plainBytes.Length];

            // Encrypt
            Console.Write("Encrypting with AES-GCM...");
            using var aes = new AesGcm(this.key);
            aes.Encrypt(nonce, plainBytes, data, tag, adBytes);
            Console.WriteLine("OK");

            // Serialize to JSON
            this.outputFile.AuthenticatedData = this.AuthenticatedData;
            this.outputFile.Nonce = Convert.ToBase64String(nonce);
            this.outputFile.Tag = Convert.ToBase64String(tag);
            this.outputFile.Data = Convert.ToBase64String(data);
            var json = JsonSerializer.ToString(this.outputFile, new JsonSerializerOptions {
                IgnoreNullValues = true,
                WriteIndented = true
            });

            // Save to output file
            if (string.IsNullOrEmpty(this.OutputFileName)) this.OutputFileName = this.InputFileName + ".fjd";
            Console.Write($"Saving to {this.OutputFileName}...");
            File.WriteAllText(this.OutputFileName, json);
            Console.WriteLine("OK");

            return 0;
        }

        // Helper methods

        private void GetKeyFromPassword() {
            // Generate random salt
            Console.Write("Generating password salt...");
            var salt = new byte[this.PasswordSaltLenght / 8];
            RandomNumberGenerator.Fill(salt);
            this.outputFile.PasswordOptions = new EncryptedFile.PasswordInfo {
                Iterations = this.PasswordIterations,
                Salt = Convert.ToBase64String(salt),
                Length = this.KeyLength
            };
            Console.WriteLine("OK");

            // Derive key from password
            Console.Write($"Generating key from password using PBKDF2 with {this.PasswordIterations} iterations...");
            var pbkdf = new Rfc2898DeriveBytes(this.Password, salt, this.PasswordIterations);
            this.key = pbkdf.GetBytes(this.KeyLength / 8);
            Console.WriteLine("OK");
        }

        private bool GetKeyFromFile() {
            if (this.KeyFileOverwrite || !File.Exists(this.KeyFile)) {
                // Create new key file
                Console.Write($"Generating random key file {this.KeyFile}...");
                this.key = new byte[this.KeyLength / 8];
                RandomNumberGenerator.Fill(this.key);
                File.WriteAllBytes(this.KeyFile, this.key);
            } else {
                // Load data from key file
                Console.Write($"Reading {this.KeyLength} bit key from file {this.KeyFile}...");
                var expectedLength = this.KeyLength / 8;
                this.key = File.ReadAllBytes(this.KeyFile);
                if (this.key.Length != expectedLength) {
                    Console.WriteLine($"Failed! File has {this.key.Length * 8} bits");
                    return false;
                }
            }
            Console.WriteLine("OK");
            return true;
        }
    }
}
