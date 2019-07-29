using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Hr
{
    public class User : BaseTable
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(30),Required]
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [MaxLength(30), Required]
        public string Password { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [MaxLength(30)]
        public string Email { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        [MaxLength(30)]
        public string Phone { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        [MaxLength(2000)]
        public byte[] Image { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Status { get; set; } = true;
      
    }
}
