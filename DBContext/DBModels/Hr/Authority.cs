using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Hr
{
    /// <summary>
    /// 权限表
    /// </summary>
    public class Authority
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 角色Id
        /// </summary>
        [MaxLength(18),Required]
        public long RoleId { get; set; }
        /// <summary>
        /// 权限内容
        /// </summary>
       [MaxLength(1000),Required]
        public string AbilityName { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        [MaxLength(1)]
        public int Mark { get; set; } = 1;
    }
}
