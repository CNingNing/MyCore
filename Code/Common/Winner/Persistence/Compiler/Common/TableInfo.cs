using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winner.Persistence.Compiler.Common
{
    public  class TableInfo
    {
        /// <summary>
        /// 数量
        /// </summary>
        public int Count;
        /// <summary>
        /// 标记
        /// </summary>
        public string Tag { get; set; } = "t";

        private string _asName;
        /// <summary>
        /// 别名
        /// </summary>
        public string AsName
        {
            set { _asName = value; }
            get
            {
                if (string.IsNullOrEmpty(_asName))
                    _asName = CreateAsName();
                return _asName;
            }
        }
        /// <summary>
        /// 连接对象
        /// </summary>
        public IDictionary<string, JoinInfo> Joins { get; set; }
        /// <summary>
        /// 得到别名
        /// </summary>
        /// <returns></returns>
        public virtual string CreateAsName()
        {
            var tag= string.Format("{0}{1}", Tag, Count);
            Count++;
            return tag;
        }
        /// <summary>
        /// 得到别名
        /// </summary>
        /// <returns></returns>
        public virtual string CreateSubTag()
        {
            var tag = string.Format("{0}{1}t", Tag, Count);
            Count++;
            return tag;
        }
    }
}
