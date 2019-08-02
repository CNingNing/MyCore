using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBModels.Warehouse
{
   public  class Order:BaseTable
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        [MaxLength(30),Required]
        public long ProductId { get; set; }

    }
}
