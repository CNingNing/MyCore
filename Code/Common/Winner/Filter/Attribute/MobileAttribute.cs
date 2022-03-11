namespace Winner.Filter.Attribute
{
    public class MobileAttribute : ValidationAttribute
    {
        public MobileAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "Mobile", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public MobileAttribute(ValidationType validationType, string message = null)
        {
            Rule=new RuleInfo{Name= "Mobile", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
