using System;
using System.ComponentModel.DataAnnotations;

namespace Fialka {
    public class DivisibleByAttribute : ValidationAttribute {
        public DivisibleByAttribute(int divider) : base("{0} must be divisible by {1}.") {
            this.Divider = divider;
        }

        public DivisibleByAttribute(int divider, Func<string> errorMessageAccessor) : base(errorMessageAccessor) {
            this.Divider = divider;
        }

        public DivisibleByAttribute(int divider, string errorMessage) : base(errorMessage) {
            this.Divider = divider;
        }

        public int Divider { get; set; }

        public override bool IsValid(object value) => !(value is int number) ? true : number % this.Divider == 0;

        public override string FormatErrorMessage(string name) => string.Format(this.ErrorMessage, name, this.Divider);
    }
}
