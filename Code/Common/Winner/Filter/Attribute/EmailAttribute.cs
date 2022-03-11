using System.Collections.Generic;

namespace Winner.Filter.Attribute
{
    public class EmailAttribute : ValidationAttribute
    {
        public EmailAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "Email", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public EmailAttribute(ValidationType validationType, string message = null)
        {
            Rule=new RuleInfo{Name= "Email", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
