namespace Winner.Filter.Attribute
{
    public class NotCharAttribute : ValidationAttribute
    {
        public NotCharAttribute(char c,string message=null)
        {
            Paramters = new object[] { c };
            Rule = new RuleInfo { Name = "NotChar", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public NotCharAttribute(char c, ValidationType validationType, string message = null)
        {
            Paramters = new object[] { c };
            Rule =new RuleInfo{Name= "NotChar", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
