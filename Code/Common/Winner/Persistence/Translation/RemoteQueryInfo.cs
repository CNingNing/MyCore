namespace Winner.Persistence.Translation
{
    public class RemoteQueryInfo:QueryInfo
    {
    
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }
   
        /// <summary>
        /// 表达式
        /// </summary>
        /// <param name="propertyName"></param>
        public RemoteQueryInfo(string propertyName)
        {
            PropertyName = propertyName;
        }
 
    }
}
