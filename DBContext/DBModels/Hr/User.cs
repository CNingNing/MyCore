using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Hr
{
    public class User
    {
        public int Id { get; set; }
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
        [MaxLength(30)]
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
