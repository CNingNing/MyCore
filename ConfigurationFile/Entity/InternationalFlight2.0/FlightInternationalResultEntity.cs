using System;
using System.Collections.Generic;

namespace Entity.InternationalFlight2
{
    public class FlightInternationalResultEntity
    {
         /// <summary>
         /// 行程
         /// </summary>
      public IList<FlightInternationalRouteEntity> FlightInternationalRoutes { get; set; }

      public IList<FlightInternationalRoutePriceEntity> FlightInternationalRoutePrices { get; set; }
    
    }
}
