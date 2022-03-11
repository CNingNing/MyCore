using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Hr
{
    /// <summary>
    /// 权限表
    /// </summary>
    public class Menu : BaseTable
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 权限内容
        /// </summary>
       [MaxLength(1000),Required]
        public string MenuName { get; set; }
        /// <summary>
        /// 父级ID
        /// </summary>
        public long ParentMenuId { get; set; }

        public Menu ParentMenu { get; set; } = new Menu { Id = 0 };
    }
}
