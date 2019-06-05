using System;
using System.Collections.Generic;
using System.Text;

namespace Fialka {

    class EncryptedFile {

        public PasswordInfo PasswordOptions { get; set; }

        public string AuthenticatedData { get; set; }

        public string Nonce { get; set; }

        public string Tag { get; set; }

        public string Data { get; set; }

        public class PasswordInfo {

            public string Salt { get; set; }

            public int Iterations { get; set; }

            public int Length { get; set; }

        }


    }
}
