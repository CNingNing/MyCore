using System;
using System.Collections.Generic;
using System.Text;
using CSRedis;

namespace Confi
{
    public  class RedisHelper
    {
        public CSRedisClient Redis { get; set; }
        
        public RedisHelper()
        {
            
            Redis = new CSRedisClient("139.196.10.246:6379,password:ning@!@#");
        }
    }
}
