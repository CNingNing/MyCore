using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Winner.Filter.Attribute
{
    public class AttributeXmlValidation : XmlValidation
    {
        protected virtual void AddValidation(IDictionary<string, RuleInfo> rules,Type type)
        {
            if(type.FullName==null || rules==null)
                return;

           if(!Validations.ContainsKey(type.FullName))
               Validations.Add(type.FullName,new List<ValidationInfo>());
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if(property.CustomAttributes==null || property.CustomAttributes.Count()==0)
                    continue;
                var des = property.GetCustomAttribute<FilterAttribute>();
                if (des == null)
                    continue;
                var name = string.IsNullOrWhiteSpace(des.PropertyName) ? property.Name : des.PropertyName;
                if (Validations[type.FullName].Count(it => it.PropertName == name) > 0)
                    continue;
                var item = GetValidationPropertyName(des,rules, property);
                if (item == null || item.Rules == null || item.Rules.Count == 0)
                    continue;
                Validations[type.FullName].Add(item);
            }
            if (Validations[type.FullName].Count == 0)
                Validations.Remove(type.FullName);
        }

        /// <summary>
        /// 得到验证实体
        /// </summary>
        /// <param name="des"></param>
        /// <param name="rules"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual ValidationInfo GetValidationPropertyName(FilterAttribute des,IDictionary<string, RuleInfo> rules,
            PropertyInfo property)
        {
            var info = new ValidationInfo
            {
                Message = des.Message,
                PropertName = string.IsNullOrWhiteSpace(des.PropertyName) ? property.Name : des.PropertyName,
                Rules = new List<RuleInfo>()
            };
            var attributes = property.GetCustomAttributes();
            foreach (var attribute in attributes)
            {
                var baseAttr = attribute as ValidationAttribute;
                if (baseAttr == null || baseAttr.Rule == null || !rules.ContainsKey(baseAttr.Rule.Name))
                    continue;
                baseAttr.Rule.Pattern = ReplacePatternArgs(rules[baseAttr.Rule.Name].Pattern, baseAttr.Paramters);
                info.Rules.Add(baseAttr.Rule);
            }

            return info;
        }

        /// <summary>
        /// 替换正则表达式里的参数
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual string ReplacePatternArgs(string rule,object[] args)
        {
            if (args == null)
                return rule;
            for (int i = 0; i < args.Length; i++)
            {
                rule = rule.Replace(string.Format("P{0}", i), args[i]==null?"": args[i].ToString());
            }
            return rule;
        }
    } 
}
