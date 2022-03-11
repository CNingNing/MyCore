using System.Collections.Generic;

namespace Winner.Filter.Attribute
{
    public class UserNameAttribute : ValidationAttribute
    {
        public UserNameAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "UserName", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public UserNameAttribute(ValidationType validationType, string message = null)
        {
            Rule=new RuleInfo{Name= "UserName", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
