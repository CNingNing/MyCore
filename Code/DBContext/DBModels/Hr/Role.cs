﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Hr
{
    public class Role : BaseTable
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(30),Required]
        public string Name { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Status { get; set; } = true;
        /// <summary>
        /// 角色对应多个对象
        /// </summary>
        public List<UserRole> UserRoles { get; set; }
    }
}
