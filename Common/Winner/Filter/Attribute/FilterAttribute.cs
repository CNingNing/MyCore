namespace Winner.Filter.Attribute
{
    public class FilterAttribute : System.Attribute
    {
        /// <summary>
        /// 验证属性名
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
    }
}
