namespace Winner.Filter.Attribute
{
    public class ValueRangeAttribute : ValidationAttribute
    {
        public ValueRangeAttribute(double minLength, double maxLength, string message = null)
        {
            Paramters = new object[] { minLength, maxLength };
            Rule = new RuleInfo { Name = "ValueRange", Message = message, ValidationTypes = GetValidateTypes(),IsRange=true};
        }
        public ValueRangeAttribute(double minLength, double maxLength, ValidationType validationType, string message = null)
        {
            Paramters = new object[] { minLength, maxLength };
            Rule = new RuleInfo { Name = "ValueRange", Message = message, ValidationTypes = GetValidateTypes(validationType), IsRange = true };
        }
    }
}
