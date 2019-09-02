using System;
using System.Collections.Generic;
namespace Entity.InternationalFlight
{
    public class FlightInternationalRoutePriceBaggageEntity
    {
        /// <summary>
        /// 承运
        /// </summary>
        public string Carrier { get; set; }
        /// <summary>
        /// 航班号
        /// </summary>
        public string FlightNumber { get; set; }
        /// <summary>
        /// 计件行李
        /// </summary>
        public int AllowedPieces { get; set; }
        /// <summary>
        /// 计重行李
        /// </summary>
        public float AllowedWeight { get; set; }
        /// <summary>
        /// 计重返回K，表示KG，计件返回空
        /// </summary>
        public string AllowedWeightUnit { get; set; }
        /// <summary>
        /// 免费携带数
        /// </summary>
        public int FreeAllowedPieces { get; set; }
        /// <summary>
        /// 计重行李免费重量
        /// </summary>
        public float FreeAllowedWeight { get; set; }
    }
}
