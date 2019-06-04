using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fialka {
    [AttributeUsage(AttributeTargets.Class)]
    public class EncryptCommandValidationAttribute : ValidationAttribute {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            if (!(value is EncryptCommand options)) throw new ArgumentException();

            if (!string.IsNullOrEmpty(options.Password) && !string.IsNullOrEmpty(options.KeyFile)) {
                return new ValidationResult("The -p and -k options are mutually exclusive.");
            } else if (options.KeyFileOverwrite && string.IsNullOrEmpty(options.KeyFile)) {
                return new ValidationResult("The -f option can be used only when -k is used as well.");
            } else {
                return ValidationResult.Success;
            }
        }
    }
}
