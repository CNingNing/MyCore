namespace Winner.Filter.Attribute
{
    public class LowerEnglishAttribute : ValidationAttribute
    {
        public LowerEnglishAttribute(string message = null)
        {
            
            Rule = new RuleInfo { Name = "LowerEnglish", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public LowerEnglishAttribute(ValidationType validationType, string message = null)
        {
           
            Rule = new RuleInfo { Name = "English", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
