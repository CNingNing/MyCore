using System.Collections.Generic;

namespace Winner.Filter.Attribute
{
    public class ValidationAttribute : System.Attribute
    {
       
        /// <summary>
        /// 规则
        /// </summary>
        public RuleInfo Rule { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public object[] Paramters { get; set; }

      
        /// <summary>
        /// 得到默认
        /// </summary>
        /// <returns></returns>
        protected ValidationType[] GetValidateTypes(ValidationType validateType=0)
        {
            if((int)validateType==1)
                return new[] { ValidationType.Add};
            if ((int)validateType == 2)
                return new[] { ValidationType.Modify };
            if ((int)validateType == 3)
                return new[] { ValidationType.Add, ValidationType.Modify };
            if ((int)validateType == 4)
                return new[] { ValidationType.Remove };
            if ((int)validateType == 5)
                return new[] { ValidationType.Add,ValidationType.Remove };
            if ((int)validateType == 5)
                return new[] { ValidationType.Modify, ValidationType.Remove };
            if ((int)validateType == 7)
                return new[] { ValidationType.Add,ValidationType.Modify, ValidationType.Remove };
            return new[] { ValidationType.Add, ValidationType.Modify };
        }
    }
}
