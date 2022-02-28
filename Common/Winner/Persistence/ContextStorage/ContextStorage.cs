using System;
using System.Threading;

namespace Winner.Persistence.ContextStorage
{

    public class ContextStorage : IContextStorage
    {
        AsyncLocal<ContextInfo> _context = new AsyncLocal<ContextInfo>();
        /// <summary>
        /// 得到上下文
        /// </summary>
        /// <returns></returns>
        public virtual ContextInfo Get()
        {
            return _context.Value;
        }
        /// <summary>
        /// 设置上下文
        /// </summary>
        /// <param name="contexnt"></param>
        public virtual void Set(ContextInfo contexnt)
        {
            _context.Value = contexnt;
        }
    }

}
