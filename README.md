# Fialka.net
Console application showcasing symmetric crypto in .NET Core 3.0. The name is a homage to [Fialka (ФИАЛКА)](https://www.cryptomuseum.com/crypto/fialka/), russian crypto machine used by Warszaw Pact countries, including Czechoslovakia, in second half of 20th century.

> **Please note:** This application is not intended to be used for real-world production security applications. It's intended to be a teaching tool, showcasing how to perform some tasks in .NET Core 3.0.

## Features

* Encrypt or decrypt any file using AES-GCM.
* Use raw key from keyfile (may be generated randomly).
* Generate encryption key from password using PBKDF2.
* The encrypted data structure is JSON, so open to various tinkering and demo of attacks.
* Uses the new `System.Text.Json` API.
* Commandline application using the [McMaster.Extensions.CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) library.

## Usage

### Encrypt a file

    fialka encrypt [options] <infile>

Argument        | Meaning
----------------|------------------------------------------------------------------
`infile       ` | Input file name
`-o <outfile> ` | Output file name (default `<infile>.fjd`)
`-a <string>  ` | Additional authenticated data
`-p <password>` | Encryption password
`-i <number>  ` | Number of PBKDF2 password iterations (default 10000)
`-s <bits>    ` | Length of PBKDF2 password salt (default 256)
`-k <keyfile> ` | Encryption key file name
`-l <bits>    ` | Length of generated or derived key (128 or 256, default 256)
`-f           ` | Force overwrite key file

### Decrypt a file

    fialka decrypt [options] <infile>

Argument        | Meaning
----------------|------------------------------------------------------------------
`infile       ` | Input file name
`-o <outfile> ` | Output file name (default `<infile>` without extension)
`-p <password>` | Decryption password
`-k <keyfile> ` | Decryption key file name


## Contributor Code of Conduct

This project adheres to No Code of Conduct. We are all adults. We accept anyone's contributions. Nothing else matters.

For more information please visit the [No Code of Conduct](https://github.com/domgetter/NCoC) homepage.