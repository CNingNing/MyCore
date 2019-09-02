using System;
using System.Collections.Generic;

namespace Entity.InternationalFlight
{
    public class FlightInternationalResultEntity
    {
         /// <summary>
         /// 行程
         /// </summary>
        public IList<FlightInternationalJourneyEntity> FlightJourneys { get; set; }
    
    }
}
