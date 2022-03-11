namespace Winner.Filter.Attribute
{
    public class EnglishAttribute : ValidationAttribute
    {
        public EnglishAttribute(string message = null)
        {
            
            Rule = new RuleInfo { Name = "English", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public EnglishAttribute(ValidationType validationType, string message = null)
        {
           
            Rule = new RuleInfo { Name = "English", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
