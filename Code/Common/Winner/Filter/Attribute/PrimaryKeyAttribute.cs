using System.Collections.Generic;

namespace Winner.Filter.Attribute
{
    public class PrimaryKeyAttribute : ValidationAttribute
    {
        public PrimaryKeyAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "PrimaryKey", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public PrimaryKeyAttribute(ValidationType validationType, string message = null)
        {
            
            Rule =new RuleInfo{Name= "PrimaryKey", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
