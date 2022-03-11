using System;
using System.Linq.Expressions;

namespace Winner.Persistence.Linq
{
    public static class EntityExtension
    {
        #region 扩展方法

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static EntityInfo Where<T>(this EntityInfo entity,Expression<Func<T,bool>> predicate)
        {
            new EntityWhereExpression(entity).Translate(predicate);
            return entity;
        }
        /// <summary>
        /// 设置属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static EntityInfo SetProperty<T>(this EntityInfo entity,Expression<Func<T, object>> expression)
        {
            var exp = expression.ToString();
            if (expression.Parameters.Count > 0)
            {
                exp = exp.Replace(expression.Parameters[0].Type.FullName, "");
            }
            var index = exp.IndexOf(".");
            var lastIndex = exp.IndexOf(",");
            if (lastIndex < 0)
                lastIndex = exp.IndexOf(")");
            if (lastIndex < 0)
                lastIndex = exp.Length;
            var pname=exp.Substring(index + 1, lastIndex - index - 1);
            entity.SetProperty(pname);
            return entity;
        }


        #endregion
       

    }

}