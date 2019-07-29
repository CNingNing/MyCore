using System;
using System.Collections.Generic;
using System.Text;

namespace DBModels
{
    public class BaseTable
    {
        /// <summary>
        /// 插入时间
        /// </summary>
        public DateTime InsertTime { get; set; } = new DateTime(1800, 1, 1);
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; } = new DateTime(1800, 1, 1);
        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime DeleteTime { get; set; } = new DateTime(1800, 1, 1);
        /// <summary>
        /// 删除数据 将置为0
        /// </summary>
        public int Mark { get; set; } = 1;
    }
}
