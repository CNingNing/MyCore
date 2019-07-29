using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace DBModels.Hr
{
    /// <summary>
    /// 权限表
    /// </summary>
    public class Authority:BaseTable
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 角色Id
        /// </summary>
        [MaxLength(18),Required]
        public List<Role> Role { get; set; }
        /// <summary>
        /// 权限内容
        /// </summary>
       [MaxLength(1000),Required]
        public string AbilityName { get; set; }
    
    }
}
