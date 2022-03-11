using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{

    public class AirFareSearchOneDetailSearchResult
    {
        public List<AirFareSearchOneDetailFlight> flights { get; set; }
        public List<Fare> fares { get; set; }
        public List<Solution> solutions { get; set; }
        public List<MulticabinSolution> multicabinSolutions { get; set; }
        public List<Agency> agencies { get; set; }
        public Debuginformation debugInformation { get; set; }
        public Error error { get; set; }
        public string uuid { get; set; }
    }

    public class Error
    {
        public string error { get; set; }
    }
    public class Debuginformation
    {
        public string elapseTime { get; set; }
        public Versions versions { get; set; }
    }

    public class MulticabinSolution
    {
        //public List<Requestsegment> requestSegments { get; set; }

        //public List<MulticabinTicket> tickets { get; set; }

    }

    public class MulticabinTicket
    {
        public List<TicketFare> fares { get; set; }
        public string platingCarrier { get; set; }

        public List<MulticabinPassenger> passengers { get; set; }
        public List<MulticabinCombo> multicabinCombos { get; set; }

        public List<pricingUnit> pricingUnits { get; set; }

    }

    public class MulticabinPassenger
    {
        public bool idRequired { get; set; }

        public List<FareInfo> fareInfo { get; set; }
    }


    public class Versions
    {
        public string atpcoDbVersion { get; set; }
        public string flightlineDbVersion { get; set; }
        public string currenciesDbVersion { get; set; }
        public string codeVersion { get; set; }
    }

    public class AirFareSearchOneDetailFlight
    {
        public int id { get; set; }
        public string carrier { get; set; }
        public string flightNumber { get; set; }
        public string operatingCarrier { get; set; }
        public string operatingFlightNumber { get; set; }
        public string departureAirport { get; set; }
        public string departureTerminal { get; set; }
        public string arrivalAirport { get; set; }
        public string arrivalTerminal { get; set; }
        public Date departureDate { get; set; }
        public Time departureTime { get; set; }
        public Date arrivalDate { get; set; }
        public Time arrivalTime { get; set; }
        public bool aircraftChange { get; set; }
        public List<IntermediateAirports> intermediateAirports { get; set; }
        public List<Aircrafttype> aircraftTypes { get; set; }
        public string serviceType { get; set; }
        public int distance { get; set; }
        public int duration { get; set; }
        public string displayCarrier { get; set; }
        public string mealService { get; set; }
        public bool et { get; set; }
        public bool ASR { get; set; }
        public int stopQuantity { get; set; }
        public List<int> groundTimes { get; set; }
    }


    public class IntermediateAirports
    {
        public string type { get; set; }
    }

    public class Date
    {
        public int year { get; set; }
        public int day { get; set; }
        public int month { get; set; }
    }

    public class Time
    {
        public int hour { get; set; }
        public int minutes { get; set; }
    }




    public class Aircrafttype
    {
        public string aircraftTypeCode { get; set; }
        public string aircraftGroupCode { get; set; }
        public string category { get; set; }
    }

    public class Fare
    {
        public int id { get; set; }
        public string carrier { get; set; }
        public string origin { get; set; }
        public string destination { get; set; }
        public string fbc { get; set; }
        public bool @private { get; set; }
        public string fareTypeCode { get; set; }
        public string globalIndicator { get; set; }

        //public string oAddonOrigin { get; set; }
        //public string oAddonDestination { get; set; }
        //public string dAddonOrigin { get; set; }
        //public string dAddonDestination { get; set; }
    }

    public class Solution
    {
        public int sId { get; set; }
        public int sortScore { get; set; }
        public BestPrice bestPointsAcrossAgenciesBaseFare { get; set; }
        public BestPrice bestPriceAcrossAgenciesBaseFare { get; set; }
        public BestPrice bestPriceAcrossAgenciesTax { get; set; }
        public BestPrice bestPriceAcrossAgenciesTotal { get; set; }
        public List<Requestsegment> requestSegments { get; set; }
        public List<Ticket> tickets { get; set; }
    }
    public class BestPrice
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }

    public class Requestsegment
    {
        public string origin { get; set; }
        public string destination { get; set; }
        public List<int> flights { get; set; }
    }

    public class Ticket
    {
        public BestPrice bestPointsAcrossAgenciesBaseFare { get; set; }
        public BestPrice bestPriceAcrossAgenciesBaseFare { get; set; }
        public BestPrice bestPriceAcrossAgenciesTax { get; set; }
        public BestPrice bestPriceAcrossAgenciesTotal { get; set; }
        public string pricingIndicator { get; set; }
        public List<TicketAgency> agencies { get; set; }
        public string platingCarrier { get; set; }
        public List<TicketFare> fares { get; set; }
    }
    public class TicketAgency
    {
        public int agencyId { get; set; }
        public List<Passenger> passengers { get; set; }
        public bool @private { get; set; }
        public string accountCode { get; set; }
        public string tourCode { get; set; }
        public List<string> avls { get; set; }

        //public string fareCurrencySelection { get; set; }
    }




    public class Passenger
    {
        public PriceBreakdown price { get; set; }
        public string ptc { get; set; }
        public bool idRequired { get; set; }
        public List<FareInfo> fareInfo { get; set; }
        public string fareline { get; set; }
    }

    public class PriceBreakdown
    {
        public BestPrice total { get; set; }
        public BestPrice totalBase { get; set; }
        public BestPrice totalTaxIata { get; set; }
        public BestPrice totalTaxYQYR { get; set; }
        public List<BestPrice> @base { get; set; }
        public List<Tax> tax { get; set; }
        public List<Surcharge> surcharges { get; set; }
        public List<BagInfo> baggageInfo { get; set; }
        [JsonProperty(PropertyName = "freeCarry-onBagInfo")]
        public List<BagInfo> freeCarryonBagInfo { get; set; }
        public Commission commission { get; set; }
       
        public Tickettimelimit ticketTimeLimit { get; set; }
    }

    public class Surcharge
    {
        public string type { get; set; }
        public BestPrice price { get; set; }
    }

    public class MulticabinCombo
    {
        public List<MulticabinComboFare> fares { get; set; }
        public bool @private { get; set; }

        public string accountCode { get; set; }

        public string tourCode { get; set; }

        public BestPrice ticketPrice { get; set; }

        public List<MulticabinComboPassenger> passengers { get; set; }
    }
    public class MulticabinComboPassenger
    {
        public string ptc { get; set; }
        public PriceBreakdown price { get; set; }
    }
    public class MulticabinComboFare
    {
        public int type { get; set; }
    }



    public class Commission
    {
        /// <summary>
        /// GROSS,NET
        /// </summary>
        public string type { get; set; }
        public float percent { get; set; }
        public BestPrice amount { get; set; }
        public string commissionSource { get; set; }
    }



    public class Tickettimelimit
    {
        public Date date { get; set; }
        public Time time { get; set; }
    }





    public class Tax
    {
        public string code { get; set; }
        public BestPrice price { get; set; }
    }


    public class BagInfo
    {
        public int allowedPieces { get; set; }
        public int allowedWeight { get; set; }
        public string allowedWeightUnit { get; set; }
    }

    public class FareInfo
    {
        public bool negotiatedFare { get; set; }
        public bool accompaniedTravel { get; set; }
        public string fbcOverride { get; set; }
        public string ruleRef1 { get; set; }
        public string ruleRef2 { get; set; }
        public string dataSource { get; set; }
        public Penalty penalty { get; set; }
    }

    public class Penalty
    {
        public PenaltyDetails change { get; set; }
        public PenaltyDetails refund { get; set; }
        public PenaltyDetails noshow { get; set; }
    }
    public class PenaltyDetails
    {
        public PenaltyCharges beforeDeparture { get; set; }
        public PenaltyCharges afterDeparture { get; set; }
    }


    public class PenaltyCharges
    {
        public bool allowed { get; set; }
        public BestPrice price { get; set; }
    }

    public class TicketFare
    {
        public int fareId { get; set; }
        public List<TicketFareFlight> flights { get; set; }
        public bool changeable { get; set; }
        public bool refundable { get; set; }
        public bool upgradable { get; set; }
        public int io { get; set; }
        public int id { get; set; }
        public string cabin { get; set; }
    }

    public class TicketFareFlight
    {
        public int flightId { get; set; }
        public FlightPassenger passengers { get; set; }
        public List<Avl> avl { get; set; }
    }

    public class FlightPassenger
    {
        public List<Rbdinfo> rbdInfos { get; set; }
        public string cabin { get; set; }
        public int seats { get; set; }
    }

    public class Rbdinfo
    {
        public string rbd { get; set; }
        public string restriction { get; set; }
    }

    public class Agency
    {
        public int id { get; set; }
        public string channel { get; set; }
        public string pos { get; set; }
        public string iataNumber { get; set; }
        public string departmentCode { get; set; }
        public string travelAgencyCode { get; set; }
    }

    public class Avl
    {
        public string cabin { get; set; }
        public string seats { get; set; }
    }

    public class pricingUnit
    {
        public string puType { get; set; }
        public List<FareComponent> fareComponents { get; set; }
    }

    public class FareComponent
    {
        public List<int> fareId { get; set; }
        public string owrt { get; set; }
        public string io { get; set; }
        public string cxr { get; set; }
        public string ruletarrif { get; set; }
        public string rule { get; set; }
        public string routing { get; set; }
        public string routing_left_addon { get; set; }
        public string routing_right_addon { get; set; }
        public string ftnt { get; set; }
    }

}
