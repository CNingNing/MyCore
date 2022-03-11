using System;
using System.Collections.Generic;

namespace Winner.Filter
{
    public interface IValidation
    {
        /// <summary>
        /// 验证对象
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        /// <param name="propertys"></param>
        /// <returns></returns>
        IList<ErrorInfo> ValidateInfo(object info, ValidationType type, IList<string> propertys = null);

        /// <summary>
        /// 验证对象
        /// </summary>
        /// <param name="info"></param>
        /// <param name="valids"></param>
        /// <param name="type"></param>
        /// <param name="propertys"></param>
        /// <returns></returns>
        IList<ErrorInfo> ValidateInfo(object info, IList<ValidationInfo> valids, ValidationType type, IList<string> propertys = null);
        /// <summary>
        /// 得到验证信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IList<ValidationInfo> GetValidations(string name);
        /// <summary>
        /// 得到验证信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IList<ValidationInfo> GetValidations(Type type);
        /// <summary>
        /// 添加验证信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        bool AddValidations(string name, IList<ValidationInfo> infos);
        /// <summary>
        /// 得到错误信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="propertName"></param>
        /// <returns></returns>
        ErrorInfo GetErrorInfo(string name, string propertName);
    }
}
