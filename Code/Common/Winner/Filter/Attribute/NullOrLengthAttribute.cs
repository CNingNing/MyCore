namespace Winner.Filter.Attribute
{
    public class NullOrLengthAttribute : ValidationAttribute
    {
        public NullOrLengthAttribute(int length, string message = null)
        {
            Paramters = new object[] { length };
            Rule = new RuleInfo { Name = "NullOrLength", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public NullOrLengthAttribute(int length,ValidationType validationType, string message = null)
        {
            Paramters = new object[] { length };
            Rule = new RuleInfo { Name = "NullOrLength", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
