using System;
using System.Collections.Generic;

namespace Entity.InternationalFlight
{
    public class FlightInternationalRouteEntity
    {
        /// <summary>
        /// 首段航班起飞时间
        /// </summary>
        public DateTime FirstTime { get; set; }
        /// <summary>
        /// 组合ID
        /// </summary>
        public int SID { get; set; }
        /// <summary>
        /// 去程航班key
        /// </summary>
        public string FlightKey { get; set; }
        /// <summary>
        /// 搭配最低价
        /// </summary>
        public decimal TotalLowestPrice { get; set; }
        /// <summary>
        /// 第几段行程
        /// </summary>
        public int SIDFlightTripIndex { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public IList<FlightInternationalRoutePriceEntity> FlightInternationalRoutePrices { get; set; }
        /// <summary>
        /// 航段
        /// </summary>
        public IList<FlightInternationalSegmentEntity> FlightInternationalSegments { get; set; }
    }

}
