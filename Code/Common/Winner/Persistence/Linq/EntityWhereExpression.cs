using System;
using System.Linq.Expressions;
using System.Text;

namespace Winner.Persistence.Linq
{
    public class EntityWhereExpression : WhereExpression
    {
        #region 属性
        /// <summary>
        /// 查询内容
        /// </summary>
        public EntityInfo Entity { get; private set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entity"></param>
        public EntityWhereExpression(EntityInfo entity)
        {
            Entity = entity;
           

        }
        #endregion

        #region 解析函数

        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override void Translate(Expression expression)
        {
            var unaryExpression = expression as UnaryExpression;
            var builder = new StringBuilder();
            if (unaryExpression != null)
            {
                BuilderWhereExpression(((LambdaExpression)unaryExpression.Operand).Body, builder);
            }
            else
            {
                var lambdaExpression = expression as LambdaExpression;
                if (lambdaExpression != null)
                    BuilderWhereExpression(lambdaExpression.Body, builder);
                else
                    return;
            }
            Entity.WhereExp = builder.ToString();
 
        }
        public override string CreateParamterName()
        {
            return Entity.CreateParameterName();
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void SetParameter(string name, object value)
        {
            Entity.SetParameter(name, value);
        }
        /// <summary>
        /// 转换UnaryExpression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override string ConvertConstantExpression(ConstantExpression expression, Type type = null)
        {
            return expression.ConvertConstantExpression(Entity, type);
        }
      
        #endregion

    }
}
