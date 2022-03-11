using System.Collections.Generic;

namespace Winner.Filter.Attribute
{
    public class ChineseAttribute : ValidationAttribute
    {
        public ChineseAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "Chinese", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public ChineseAttribute(ValidationType validationType, string message = null)
        {
            Rule=new RuleInfo{Name= "Number", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
