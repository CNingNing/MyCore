using System;
using System.Collections.Generic;

namespace Entity.InternationalFlight2
{
    public class FlightInternationalRouteEntity
    {
        /// <summary>
        /// 出发地
        /// </summary>
        public string Origin { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 首段航班出发时间
        /// </summary>
        public DateTime DepartureTime { get; set; }
        /// <summary>
        /// 尾段航班到达时间
        /// </summary>
        public DateTime ArrivalTime { get; set; }
        /// <summary>
        /// 解决方案ID，也可以换成GUID
        /// </summary>
        public int SId { get; set; }
        /// <summary>
        /// 保存的应该为1-2,3-4,5-6格式，好判断是第几段有哪些航班，页面好判断展示
        /// </summary>
        public string FlightId { get; set; }
        /// <summary>
        /// 航段
        /// </summary>
        public IList<FlightInternationalSegmentEntity> FlightInternationalSegments { get; set; }
    }

}
