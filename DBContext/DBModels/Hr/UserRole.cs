using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Hr
{
    public class UserRole
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public long UserId { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required]
        public long RoleId { get; set; }
        /// <summary>
        /// 用户列表
        /// </summary>
        public List<User> Users { get; set; }
        /// <summary>
        /// 角色列表
        /// </summary>
        public List<Role> Roles { get; set; }
        /// <summary>
        /// 删除数据，置为0
        /// </summary>
        [MaxLength(1)]
        public int Mark { get; set; } = 1;
    }
}
