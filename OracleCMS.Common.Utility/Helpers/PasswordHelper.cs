using System.Text.RegularExpressions;
namespace OracleCMS.Common.Utility.Helpers
{
    public static class PasswordHelper
    {
        public static List<string> ValidatePassword(string password)
        {
            List<string> validationMessages = new();
            if (password.Length < 8)
            {
                validationMessages.Add("Password should have a minimum length of 8 characters.");
            }
            if (!password.Any(char.IsUpper))
            {
                validationMessages.Add("Password should contain at least one uppercase letter.");
            }
            if (!password.Any(char.IsLower))
            {
                validationMessages.Add("Password should contain at least one lowercase letter.");
            }
            if (!password.Any(char.IsDigit))
            {
                validationMessages.Add("Password should contain at least one numeric digit.");
            }
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?]"))
            {
                validationMessages.Add("Password should contain at least one special character (!@#$%^&*()_+-=[]{};':\"\\|,.<>/?).");
            }
            if (password.Contains('\''))
            {
                validationMessages.Add("Password should not contain the single quote character (').");
            }
            return validationMessages;
        }        
    }
}
