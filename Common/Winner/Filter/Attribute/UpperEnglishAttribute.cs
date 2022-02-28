namespace Winner.Filter.Attribute
{
    public class UpperEnglishAttribute : ValidationAttribute
    {
        public UpperEnglishAttribute(string message = null)
        {
            
            Rule = new RuleInfo { Name = "UpperEnglish", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public UpperEnglishAttribute(ValidationType validationType, string message = null)
        {
           
            Rule = new RuleInfo { Name = "UpperEnglish", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
