using Component.Extension;
using System;
using System.Collections.Generic;

namespace Entity.InternationalFlight2
{
    public class FlightInternationalSegmentEntity
    {
        // <summary>
        /// 对应在返回结果的Id,可以为空
        /// </summary>
        public int FlightId { get; set; }
        /// <summary>
        /// 出发地
        /// </summary>
        public string Origin { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// 出发机场
        /// </summary>
        public string FromAirport { get; set; }
        /// <summary>
        /// 到达机场
        /// </summary>
        public string ToAirport { get; set; }
        /// <summary>
        /// 航班号
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 代码共享实际承运航班号
        /// </summary>
        public string CodeShareNumber { get; set; }
        /// <summary>
        /// 承运
        /// </summary>
        public string Carrier { get; set; }
        /// <summary>
        /// 承运公司
        /// </summary>
        public string CarrierName { get; set; }
        /// <summary>
        /// 起飞时间
        /// </summary>
        public DateTime TakeoffTime { get; set; }
        /// <summary>
        /// 到达时间
        /// </summary>
        public DateTime ArrivalTime { get; set; }
        /// <summary>
        /// 始发航站楼
        /// </summary>
        public string FromTerminal { get; set; }
        /// <summary>
        /// 到达航站楼
        /// </summary>
        public string ToTerminal { get; set; }

        /// <summary>
        /// 餐食
        /// M   Meal        不特定餐食
        /// B   Breakfast   早餐
        /// L   Lunch       午餐
        /// C   Alcoholic Beverages Complimentary   免费酒精饮料
        /// K   Continental Breakfast   大陆式早餐
        /// D   Dinner      晚餐
        /// S   Snack Or Brunch 点心或早午餐
        /// O   Cold Meal   冷食
        /// H   Hot Meal    热食
        /// R   Refreshment 茶点或小吃
        /// </summary>
        public FlightMealType MealType => Meal.Convert<FlightMealType>();
        /// <summary>
        /// 餐食
        /// </summary>
        public string Meal { get; set; }
        /// <summary>
        /// 飞行距离
        /// </summary>
        public string Distance { get; set; }
        /// <summary>
        /// 飞行时长
        /// </summary>
        public string Duration { get; set; }
        /// <summary>
        /// 是否经停
        /// </summary>
        public bool IsStop { get; set; }
        /// <summary>
        /// 经停城市
        /// </summary>
        public List<string> StopCities { get; set; }
        /// <summary>
        /// 经停数量
        /// </summary>
        public int StopCitiesCount { get; set; }
        /// <summary>
        /// 经停时长
        /// </summary>
        public List<int> StopTimes { get; set; }
        /// <summary>
        /// 是否接受选座
        /// ASR=^
        /// </summary>
        public bool IsChooseSeat { get; set; }
        /// <summary>
        /// 机型代码
        /// </summary>
        public string PlaneType { get; set; }
        /// <summary>
        /// 航班是否变更
        /// </summary>
        public bool AircraftChange { get; set; }
        /// <summary>
        /// 可选座位
        /// </summary>
        public string ChooseSeat { get; set; }
        /// <summary>
        /// 座位
        /// </summary>
        //public IList<FlightInternationalCabinEntity> FlightInternationalCabins { get; set; }


    }
    public enum FlightMealType
    {
        /// <summary>
        /// 不特定餐食
        /// </summary>
        M = 1,
        /// <summary>
        /// 早餐
        /// </summary>
        B = 2,
        /// <summary>
        /// 午餐
        /// </summary>
        L = 3,
        /// <summary>
        /// 免费酒精饮料
        /// </summary>
        C = 4,
        /// <summary>
        /// 大陆式早餐
        /// </summary>
        K = 5,
        /// <summary>
        /// 晚餐
        /// </summary>
        D = 6,
        /// <summary>
        /// 点心或早午餐
        /// </summary>
        S = 7,
        /// <summary>
        /// 冷食
        /// </summary>
        O = 8,
        /// <summary>
        /// 热食
        /// </summary>
        H = 9,
        /// <summary>
        /// 茶点或小吃
        /// </summary>
        R = 10
    }
}
