using System;
using System.Collections.Generic;

namespace Winner.Persistence.Route
{
    public class DbRouteInfo
    {
        /// <summary>
        /// 查询数量
        /// </summary>
        public int TopCount { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 是否返回所以分片
        /// </summary>
        public DbRouteType Type{ get; set; }

        /// <summary>
        /// 写取数据库表数量
        /// </summary>
        public int DataBaseTableCount { get; set; }

        /// <summary>
        /// 表的数量
        /// </summary>
        public int TableIndex { get; set; }

        /// <summary>
        /// 步长
        /// </summary>
        public int TableCount { get; set; } = 1;
        /// <summary>
        /// 读库
        /// </summary>
        public string GetDataBase { get; set; }
        /// <summary>
        /// 写库
        /// </summary>
        public string SetDataBase { get; set; }

     

        /// <summary>
        /// 存储名称
        /// </summary>
        public IList<RuleInfo> Rules { get; set; } 
      
        /// <summary>
        /// 得到存储分片
        /// </summary>
        public Func<object, ShardingInfo> GetSaveShardingHandle { get; set; }
        /// <summary>
        /// 得到查询分片
        /// </summary>
        public Func<QueryInfo, IList<ShardingInfo>> GetQueryShardingHandle { get; set; }

        #region 得到所有分片
        /// <summary>
        ///  得到所有分片
        /// </summary>
        /// <returns></returns>
        public virtual IList<ShardingInfo> GetAllShardings()
        {
            var shardings = new List<ShardingInfo>();
            for (int i = 0; i < TableCount; i++)
            {
                var sharding = new ShardingInfo
                {
                    DatabaseIndex = GetDataBaseIndex(i),
                    TableIndex = i.ToString()
                };
                shardings.Add(sharding);
            }
            return shardings;
        }
 

        /// <summary>
        /// 得到查询数据库
        /// </summary>
        public virtual string GetDataBaseIndex(int step)
        {
            if (DataBaseTableCount == 0 || step == -1)
                return "";
            var value = (double)(step / DataBaseTableCount);
            return ((int)Math.Floor(value)).ToString();
        }

        #endregion

        #region 得到分片
        /// <summary>
        /// 匹配
        /// </summary>
        /// <param name="routeIndex"></param>
        /// <returns></returns>
        public virtual ShardingInfo GetSharding(int routeIndex)
        {
            if (TableCount == 0)
                return null;
            var tableIndex = routeIndex % TableCount;
            return new ShardingInfo { TableIndex = tableIndex.ToString(), DatabaseIndex = GetDataBaseIndex(routeIndex) };
        }
        /// <summary>
        /// 匹配
        /// </summary>
        /// <param name="routeValue"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public virtual ShardingInfo GetSharding(object routeValue, RuleInfo rule)
        {
            routeValue = GetRouteValue(routeValue, rule);
            if (routeValue == null || string.IsNullOrWhiteSpace(routeValue?.ToString()))
                return null;
            if (routeValue.ToString() == rule.UnRouteValue)
                return null;
            var tableIndex = GetTableIndex(routeValue, rule);
            if (tableIndex == -1)
                return null;
            return new ShardingInfo{TableIndex=tableIndex.ToString(),DatabaseIndex = GetDataBaseIndex(tableIndex) };
        }


        /// <summary>
        /// 得到路由值
        /// </summary>
        /// <param name="routeValue"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected virtual object GetRouteValue(object routeValue, RuleInfo rule)
        {
            if (!string.IsNullOrEmpty(rule.Tag) &&
                 !routeValue.ToString().ToLower().Contains(rule.Tag.ToLower()))
                return null;
            routeValue = string.IsNullOrEmpty(rule.Tag)
                ? routeValue
                : routeValue.ToString().ToLower().Replace(rule.Tag.ToLower(), "");
            return routeValue;

        }
     
        /// <summary>
        /// 匹配正常
        /// </summary>
        /// <param name="routeValue"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected virtual int GetTableIndex(object routeValue, RuleInfo rule)
        {
            switch (rule.ShardingType)
            {
                case ShardingType.Fixed:
                    return GetFixedTableIndex(routeValue, rule);
                case ShardingType.Value:
                    return GetValueTableIndex(routeValue, rule);
                case ShardingType.Remainder:
                   return GetRemainderTableIndex(routeValue, rule);
                case ShardingType.Random:
                    return GetRandomTableIndex(routeValue, rule);
            }
            return -1;
        }

        /// <summary>
        /// 匹配正常
        /// </summary>
        /// <param name="routeValue"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected virtual int GetRemainderTableIndex(object routeValue, RuleInfo rule)
        {
            if (TableCount == 0)
                return -1;
            var value = Math.Abs(long.Parse(routeValue.ToString()));
            var step = (int)(value % TableCount);
            return step;
        }

        /// <summary>
        /// 匹配正常
        /// </summary>
        /// <param name="routeValue"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected virtual int GetValueTableIndex(object routeValue, RuleInfo rule)
        {
            if (TableCount == 0)
                return -1;
            var value = Math.Abs(long.Parse(routeValue.ToString()));
            //if (value > rule.StartValue + (rule.EndValue - rule.StartValue + 1) * TableCount)
            //    return -1;
            //var step = (int)(Math.Floor((double)((value - rule.StartValue) / (rule.EndValue - rule.StartValue))));
            //return step;
            var step = (int)Math.Floor((double)(value / rule.EndValue));
            return step % TableCount;
        }
        /// <summary>
        /// 匹配正常
        /// </summary>
        /// <param name="routeValue"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected virtual int GetRandomTableIndex(object routeValue, RuleInfo rule)
        {
            if (TableCount == 0)
                return -1;
            var random = new Random(Guid.NewGuid().ToString().GetHashCode());
            return random.Next(0, TableCount - 1);
        }
        /// <summary>
        /// 匹配正常
        /// </summary>
        /// <param name="routeValue"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        protected virtual int GetFixedTableIndex(object routeValue, RuleInfo rule)
        {
            if (rule.StartValue > 0 && rule.EndValue > 0)
            {
                var value = Math.Abs(long.Parse(routeValue.ToString()));
                if (value >= rule.StartValue && value <= rule.EndValue)
                {
                    return TableIndex;
                }
            }
            else if (!string.IsNullOrEmpty(rule.FixedValue))
            {
                if (routeValue.ToString() == rule.FixedValue)
                    return TableIndex;
            }
            return -1;
        }
        #endregion
    }
}
