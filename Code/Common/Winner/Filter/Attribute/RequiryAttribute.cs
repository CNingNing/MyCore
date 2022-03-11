using System.Collections.Generic;

namespace Winner.Filter.Attribute
{
    public class RequiryAttribute: ValidationAttribute
    {
        public RequiryAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "Requiry",Message= message,ValidationTypes = GetValidateTypes()};
        }
        public RequiryAttribute(ValidationType validationType, string message = null)
        {
            Rule=new RuleInfo{Name= "Requiry",Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
