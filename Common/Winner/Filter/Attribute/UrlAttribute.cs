namespace Winner.Filter.Attribute
{
    public class UrlAttribute : ValidationAttribute
    {
        public UrlAttribute(int length, string message = null)
        {
            Paramters = new object[] { length };
            Rule = new RuleInfo { Name = "Url", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public UrlAttribute(int length,ValidationType validationType, string message = null)
        {
            Paramters = new object[] { length };
            Rule = new RuleInfo { Name = "Url", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
