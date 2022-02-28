using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Winner.Creation
{


    public class Proxy : DispatchProxy
    {
        #region 属性
      
        /// <summary>
        /// 加载查询
        /// </summary>
        public IList<AopInfo> Aops { get; set; }

        /// <summary>
        /// 加载查询
        /// </summary>
        public Object Target { get; set; }
        #endregion

        #region 构造函数

        public Proxy()
        {

        }
        /// <summary>
        /// 对象
        /// </summary>
        /// <param name="target"></param>
        /// <param name="aops"></param>
        public virtual void Set(Object target, IList<AopInfo> aops)
        {
            Aops = aops;
            Target = target;
        }
        #endregion

        #region 接口的实现
        /// <summary>
        /// 得到代理
        /// </summary>
        /// <returns></returns>
        public static object Create(Type type)
        {
            MethodInfo mi = typeof(DispatchProxy).GetMethod("Create")
                .MakeGenericMethod(new Type[] { type, typeof(Proxy) });
            return mi.Invoke(null, null);
        }

        #endregion

        #region 延迟调用
        /// <summary>
        /// 拦截方法
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (Aops != null)
            {
                foreach (var aop in Aops)
                {
                    if(aop.Handle==null || aop.Type!= AopType.Before)
                        continue;
                    var aopArgs = new AopArgsInfo {Args = args, Name = aop.Name, Type = aop.Type};
                    if (aop.IsAsync)
                    {
                        var task = new Thread(() => { aop.Handle(aopArgs); });
                        task.Start();
                    }
                    else
                    {
                        aop.Handle(aopArgs);
                    }
                        
                }
            }
            object returnValue = targetMethod.Invoke(Target, args);
            if (Aops != null)
            {
                foreach (var aop in Aops)
                {
                    if (aop.Handle == null || aop.Type != AopType.After)
                        continue;
                    var aopArgs = new AopArgsInfo { Args = args, Name = aop.Name, Type = aop.Type };
                    if (aop.IsAsync)
                    {
                        var task = new Thread(() => { aop.Handle(aopArgs); });
                        task.Start();
                    }
                    else
                    {
                        aop.Handle(aopArgs);
                    }
                        
                }
            }
            return returnValue;
        }


        
        #endregion



    }
  
}
