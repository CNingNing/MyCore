using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Reverse
{

    public class JsonFill : XmlFill
    {
      
       

        /// <summary>
        /// 得到子集合属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override object GetManyProperties(OrmObjectInfo obj, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var property = JsonConvert.DeserializeObject<IDictionary<string, object>>("{" + value + "}");
            var array = property?.Values.FirstOrDefault() as JArray;
            if (array == null) return null;
            var result = new List<IDictionary<string, object>>();
            foreach (var jToken in array)
            {
                var properties = new Dictionary<string, object>();
                foreach (var child in jToken.Children())
                {
                    var cp = child as JProperty;
                    if (cp == null)
                        continue;

                    if (cp.Value.Type == JTokenType.Array)
                        AppendJsonObject(obj, properties, cp.Name, cp.ToString());
                    else
                        AppendJsonObject(obj, properties, cp.Name, (cp.Value as JValue)?.Value);
                }

                result.Add(properties);
            }

            return result;
          
        }

    }
}
