using System;
using System.Collections.Generic;

namespace Entity.InternationalFlight
{
    public class FlightInternationalRoutePriceEntity
    {
        /// <summary>
        /// 解决方案的ID
        /// </summary>
        public int SID { get; set; }
        /// <summary>
        /// 总价格（票价和税收）
        /// </summary>
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// 总票价（不含税）
        /// </summary>
        public decimal TotalBase { get; set; }
        /// <summary>
        /// 总税费
        /// </summary>
        public decimal TotalTaxIata { get; set; }
        /// <summary>
        /// YQYR费
        /// </summary>
        public decimal TotalTaxYQYR { get; set; }
        /// <summary>
        /// 代理费，百分比
        /// </summary>
        public float Percent { get; set; }
        /// <summary>
        /// 代理费类型
        /// </summary>
        public CommissionType Commission { get; set; }
        /// <summary>
        /// 代理费来源
        /// </summary>
        public string CommissionSource { get; set; }
        /// <summary>
        /// 最晚出票时限，精确到分钟
        /// </summary>
        public DateTime TicketTimeLimint { get; set; }
        /// <summary>
        /// 是否可改签
        /// </summary>
        public ChangeRefundNoShowRule Change { get; set; }
        /// <summary>
        /// 是否可以退票
        /// </summary>
        public ChangeRefundNoShowRule Refund { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ChangeRefundNoShowRule NoShow { get; set; }

        public string Rule1 { get; set; }
        public string Rule2 { get; set; }
        /// <summary>
        /// 运价基础
        /// </summary>
        public string FareBasic { get; set; }
        /// <summary>
        /// 座位
        /// </summary>
        public string Cabin { get; set; }
        public IList<FlightInternationalRoutePriceBaggageEntity> FlightInternationalRoutePriceBaggages { get;set;}
    }
    /// <summary>
    /// 代理费类型
    /// </summary>
    public enum CommissionType
    {
        GROSS = 1,
        NET = 2
    }
    public class ChangeRefundNoShowRule
    {
        /// <summary>
        ///起飞前是否允许操作
        /// </summary>
        public bool BeforeAllowed { get; set; }
        /// <summary>
        ///起飞后是否允许操作
        /// </summary>
        public bool AfterAllowed { get; set; }
        /// <summary>
        /// 起飞前费用
        /// </summary>
        public decimal BeforePrice { get; set; }
        /// <summary>
        /// 起飞前费用
        /// </summary>
        public decimal AfterPrice { get; set; }
        /// <summary>
        /// 规则
        /// </summary>
        public string Rule { get; set; }
    }
}
