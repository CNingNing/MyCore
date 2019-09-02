using System;
using System.Collections.Generic;
namespace Entity.InternationalFlight
{
    public class FlightInternationalCabinEntity
    {
        /// <summary>
        /// 航班号
        /// </summary>
        public string FlightNumber { get; set; }
        /// <summary>
        /// 舱位代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
    }
}
