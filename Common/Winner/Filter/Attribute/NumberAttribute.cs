using System.Collections.Generic;

namespace Winner.Filter.Attribute
{
    public class NumberAttribute : ValidationAttribute
    {
        public NumberAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "Number", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public NumberAttribute(ValidationType validationType, string message = null)
        {
            Rule=new RuleInfo{Name= "Number", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }

    }
}
