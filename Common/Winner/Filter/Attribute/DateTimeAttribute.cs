namespace Winner.Filter.Attribute
{
    public class DateTimeAttribute : ValidationAttribute
    {
        public DateTimeAttribute(string message=null)
        {
            Rule = new RuleInfo { Name = "DateTime", Message= message,ValidationTypes = GetValidateTypes()};
        }
        public DateTimeAttribute(ValidationType validationType, string message = null)
        {
            Rule=new RuleInfo{Name= "DateTime", Message=message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
