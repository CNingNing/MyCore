using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Hr
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(30),Required]
        public string Name { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        [MaxLength(1)]
        public int Mark { get; set; } = 1;
    }
}
