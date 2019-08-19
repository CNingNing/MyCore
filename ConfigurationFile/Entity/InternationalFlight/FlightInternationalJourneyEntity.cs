using System;
using System.Collections.Generic;

namespace Entity.InternationalFlight
{
    public class FlightInternationalJourneyEntity
    {
        /// <summary>
        /// 旅行日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 星期
        /// </summary>
        public string Week { get; set; }

        /// <summary>
        /// 出发城市
        /// </summary>
        public string FromCity { get; set; }
        /// <summary>
        /// 到达城市
        /// </summary>
        public string ToCity { get; set; }
        /// <summary>
        /// 航段信息
        /// </summary>
        public IList<FlightInternationalRouteEntity> FlightInternationalRoutes { get; set; }
    }
}
