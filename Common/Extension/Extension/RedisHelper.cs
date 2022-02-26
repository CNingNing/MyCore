using System;
using System.Collections.Generic;
using System.Text;
using CSRedis;
namespace Component.Extension
{
    public  class RedisHelper
    {
        public CSRedisClient redis { get; set; }

        public RedisHelper()
        {
            redis = new CSRedisClient("139.196.10.246:6379,password:ning@!@#");
        }
    }
}
