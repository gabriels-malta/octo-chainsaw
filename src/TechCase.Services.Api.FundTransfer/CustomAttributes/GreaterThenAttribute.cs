namespace System.ComponentModel.DataAnnotations
{
    public class GreaterThenAttribute : ValidationAttribute
    {
        private readonly double _minValue;
        public GreaterThenAttribute(double minValue) => _minValue = minValue;

        public override bool IsValid(object value)
        {
            if (value is not double d)
                return false;

            return d > _minValue;
        }
    }
}
