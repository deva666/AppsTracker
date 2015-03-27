#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace AppsTracker
{
    public class EmailValidation : ValidationRule
    {
        readonly string regexText = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        readonly string errorMessage = "Invalid email address";

        public EmailValidation()
        {
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = ValidationResult.ValidResult;

            if (!String.IsNullOrEmpty(this.regexText))
            {
                string text = value as string ?? String.Empty;

                if (!Regex.IsMatch(text, this.regexText, RegexOptions.IgnoreCase))
                    result = new ValidationResult(false, errorMessage);
            }

            return result;
        }
    }

    public class NumberValidation : ValidationRule
    {
        readonly string regexText = @"^[0-9]+$";
        readonly string errorMessage = "Input only numbers";

        public NumberValidation()
        {
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = ValidationResult.ValidResult;

            if (!String.IsNullOrEmpty(this.regexText))
            {
                string text = value as string ?? String.Empty;

                if (!Regex.IsMatch(text, this.regexText, RegexOptions.IgnoreCase))
                    result = new ValidationResult(false, errorMessage);
            }

            return result;
        }

    }

    public class LengthValidation : ValidationRule
    {
        public int MinimumLength { get; set; }
        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            if ((value ?? string.Empty).ToString().Length < MinimumLength)
            {
                result = new ValidationResult(false, ErrorMessage);
            }
            return result;
        }
    }
}
