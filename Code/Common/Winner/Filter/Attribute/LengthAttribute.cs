namespace Winner.Filter.Attribute
{
    public class LengthAttribute : ValidationAttribute
    {
        public LengthAttribute(int length, string message = null)
        {
            Paramters = new object[] { length };
            Rule = new RuleInfo { Name = "Length", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public LengthAttribute(int length,ValidationType validationType, string message = null)
        {
            Paramters = new object[] { length };
            Rule = new RuleInfo { Name = "Length", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
