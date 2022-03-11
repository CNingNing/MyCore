namespace Winner.Filter.Attribute
{
    public class LengthRangeAttribute : ValidationAttribute
    {
        public LengthRangeAttribute(int minLength, int maxLength, string message = null)
        {
            Paramters = new object[] { minLength , maxLength };
            Rule = new RuleInfo { Name = "LengthRange", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public LengthRangeAttribute(int minLength, int maxLength, ValidationType validationType, string message = null)
        {
            Paramters = new object[] { minLength, maxLength };
            Rule = new RuleInfo { Name = "LengthRange", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
