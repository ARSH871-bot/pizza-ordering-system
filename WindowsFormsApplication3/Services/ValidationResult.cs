namespace WindowsFormsApplication3.Services
{
    public class ValidationResult
    {
        public bool   IsValid      { get; private set; }
        public string ErrorMessage { get; private set; }

        private ValidationResult() { }

        public static ValidationResult Ok()             => new ValidationResult { IsValid = true };
        public static ValidationResult Fail(string msg) => new ValidationResult { IsValid = false, ErrorMessage = msg };
    }
}
