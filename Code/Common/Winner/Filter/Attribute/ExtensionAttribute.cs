namespace Winner.Filter.Attribute
{
    public class ExtensionAttribute : ValidationAttribute
    {
        public ExtensionAttribute(string extension, string message = null)
        {
            Paramters = new object[] { extension };
            Rule = new RuleInfo { Name = "Extension", Message = message, ValidationTypes = GetValidateTypes() };
        }
        public ExtensionAttribute(string extension, ValidationType validationType, string message = null)
        {
            Paramters = new object[] { extension };
            Rule = new RuleInfo { Name = "Extension", Message = message, ValidationTypes = GetValidateTypes(validationType) };
        }
    }
}
