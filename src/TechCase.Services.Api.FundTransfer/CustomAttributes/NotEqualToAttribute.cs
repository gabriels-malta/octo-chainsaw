namespace System.ComponentModel.DataAnnotations
{
    public class NotEqualToAttribute : ValidationAttribute
    {
        private string _targetProperty;
        public NotEqualToAttribute(string targetProperty) => _targetProperty = targetProperty;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string actualValue = value as string;
            var property = validationContext.ObjectType.GetProperty(_targetProperty);

            string targetValue = property.GetValue(validationContext.ObjectInstance) as string;
            return string.Equals(actualValue, targetValue, StringComparison.InvariantCultureIgnoreCase) ? new ValidationResult(ErrorMessageString) : ValidationResult.Success;
        }
    }
}
