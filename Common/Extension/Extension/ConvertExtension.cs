using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = System.Xml.Formatting;

namespace Component.Extension
{
    public static class ConvertExtension
    {
        #region XML
        /// <summary>
        /// 序列化xml
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeXml(this object obj)
        {
            if (obj == null)
                return null;
            try
            {
                if (Equals(null, obj))
                {
                    return null;
                }
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.Default
                };
                //去除xml声明
                var mem = new MemoryStream();
                using (var writer = XmlWriter.Create(mem, settings))
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    var formatter = new XmlSerializer(obj.GetType());
                    formatter.Serialize(writer, obj, ns);
                }
                return Encoding.Default.GetString(mem.ToArray());
            }
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// 反序列化xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeXml<T>(this string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return default(T);
            try
            {
                if (string.IsNullOrEmpty(xml))
                    return default(T);
                using (var sr = new StringReader(xml))
                {
                    var xmldes = new XmlSerializer(typeof(T), "");
                    return (T)xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return default(T);
            }
        }
        #endregion

        #region JSON
       
        /// <summary>
        /// 序列化json
        /// </summary>
        /// <param name="input"></param>
        /// <param name="settings"></param>
        public static string SerializeJson(this object input, JsonSerializerSettings settings=null)
        {
            if (input == null)
                return null;
            try
            {
                if(settings==null)
                    return JsonConvert.SerializeObject(input);
                return JsonConvert.SerializeObject(input, settings);
               
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 反序列化json
        /// </summary>
        /// <param name="input"></param>
        public static T DeserializeJson<T>(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return default(T);
            try
            {
                if (string.IsNullOrEmpty(input))
                    return default(T);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                return default(T); 
            }
        }
        public static T DeserializeJson<T>(this object input)
        {
            return DeserializeJson<T>(input?.ToString());
        }
        #endregion

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        public static T Convert<T>(this object input)
        {
            try
            {
                if (input == null)
                    return default(T);
                if (string.IsNullOrWhiteSpace(input.ToString()))
                    return (T) input;

                var value = System.Convert.ChangeType(input, typeof(T));
                return (T) value;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 得到
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveHtml(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var reg = new System.Text.RegularExpressions.Regex("<[^>]*[>]?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (reg.IsMatch(input))
            {
                input = reg.Replace(input, "");
            }
            return input;
        }

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        public static string Get(this IDictionary<string,string> input,string key)
        {
            if (input == null || !input.ContainsKey(key))
                return null;
            return input[key];
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        public static object Get(this IDictionary<string, object> input, string key)
        {
            if (input == null || !input.ContainsKey(key))
                return null;
            return input[key];
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        public static T Get<T>(this IDictionary<string, object> input, string key)
        {
            if (input == null || !input.ContainsKey(key))
                return default(T);
            return input[key].Convert<T>();
        }
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        public static valueT Get<keyT,valueT>(this IDictionary<keyT, valueT> input, keyT key)
        {
            if (input == null || !input.ContainsKey(key))
                return default(valueT);
            return input[key];
        }
        #region 金额大写
        /// <summary>
        /// 转成中文大写数值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertToChineseMoney(this decimal value)
        {
            var sb = new StringBuilder();
            if (value == 0)
                return "零元整";
            if (value < 0) sb.Append("负");
            value = Math.Abs(value);
            var zhengwei = new Dictionary<int, string> { { 1000, "仟" }, { 100, "佰" }, { 10, "拾" } };
            var xiaowei = new Dictionary<int, string> { { 10, "角" }, { 1, "分" } };
            var amount = System.Convert.ToInt32(value);
            int yi = amount / 100000000;
            if (yi != 0)
            {
                AppendChineseMoney(sb, yi, zhengwei,1000);
                sb.Append("亿");
            }
            amount = amount % 100000000;
            int wan = amount / 10000;
            if (wan != 0)
            {
                AppendChineseMoney(sb, wan, zhengwei, 1000);
                sb.Append("万");
            }
            int qian = amount % 10000;
            AppendChineseMoney(sb, qian, zhengwei, 1000);
            var xiaoshu = (value - amount)*100;
            sb.Append("元");
            if (xiaoshu == 0)
                sb.Append("整");
            else
                AppendChineseMoney(sb,System.Convert.ToInt32(xiaoshu), xiaowei,10);
            return sb.ToString();
        }

        /// <summary>
        /// 转成4位阿拉伯数值
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="value"></param>
        /// <param name="wei"></param>
        /// <param name="divor"></param>
        /// <returns></returns>
        private static void AppendChineseMoney(StringBuilder sb, int value, IDictionary<int, string> wei, int divor)
        {
            while (divor >= 1)
            {
                int t = value / divor;
                if (t != 0)
                {
                    sb.Append(GetChinesenMoneyName(t));
                    if (sb.Length > 0 && wei.ContainsKey(divor)) sb.Append(wei[divor]);
                }
                else if (divor != 1 && sb.Length > 0 && !sb[sb.Length - 1].Equals('零'))
                {
                    sb.Append("零");
                }
                value = value % divor;
                divor = divor / 10;
                if (value == 0)break;
            }
        }
 

        /// <summary>
        /// 得到名称
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetChinesenMoneyName(int value)
        {
            if (value > 10 || value < 0) return "";
            var names = new Dictionary<int, string>{
                {0,"零"},{1,"壹"},{2,"贰"},{3,"叁"},{4,"肆"},{5,"伍"},{6,"陆"},{7,"柒"},{8,"捌"},{9,"玖"} };
            return names[value];
        }
        #endregion

        #region 枚举
        /// <summary>
        /// 转换枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertEnum<T>(this int value)
        {
            try
            {
                return (T)(object)value;
            }
            catch (Exception)
            {

                return default(T);
            }
        }
        /// <summary>
        /// 转换枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertEnum<T>(this string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            try
            {
                return (T)Enum.Parse(typeof (T), value);
            }
            catch (Exception)
            {

                return default(T);
            }
        }

        /// <summary>
        /// 得到枚举组合值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int GetEnumSumValue<T>(this string values)
        {
            return ((T[])Enum.GetValues(typeof(T))).Where(en => values.Contains(en.ToString()))
                .Sum(en => en.Convert<int>());
        }

        /// <summary>
        /// 得到枚举组合值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetEnums<T>(this int sum)
        {
            return (from t in (T[]) Enum.GetValues(typeof (T)) where (t.Convert<int>() & sum) > 0 select t.Convert<T>()).ToArray();
        }
        /// <summary>
        /// 得到枚举组合值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int[] GetEnumValues<T>(this int sum)
        {
            return (from t in (T[])Enum.GetValues(typeof(T)) where (t.Convert<int>() & sum) > 0 select t.Convert<int>()).ToArray();
        }
        /// <summary>
        /// 得到名称
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T[] GetEnums<T>(this string values)
        {
            if (values == null) return null;
            return (from value in (T[]) Enum.GetValues(typeof (T)) where values.Contains((value.Convert<int>()).ToString(CultureInfo.InvariantCulture))
                    || values.Contains(value.ToString())
                    select value.Convert<T>()).ToArray();

        }


        /// <summary>
        /// 绑定星期名称
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string GetEnumComboStringValue<T>(this string values)
        {
            if (values == null) return null;
            var mess = new StringBuilder();
            foreach (var value in (T[])Enum.GetValues(typeof(T)))
            {
                if (values.Contains((value.Convert<int>()).ToString(CultureInfo.InvariantCulture)))
                    mess.AppendFormat("{0},", value);
            }
            if (mess.Length > 0) mess.Remove(mess.Length - 1, 1);
            return mess.ToString();
        }
        /// <summary>
        /// 绑定星期名称
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static string GetEnumComboStringValue<T>(this int sum)
        {

            var mess = new StringBuilder();
            foreach (var value in (T[])Enum.GetValues(typeof(T)))
            {
                if ((value.Convert<int>() & sum) > 0)
                    mess.AppendFormat("{0},", value);
            }
            if (mess.Length > 0) mess.Remove(mess.Length - 1, 1);
            return mess.ToString();
        }
        /// <summary>
        /// 设置枚举值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string GetEnumComboIntValue<T>(this string values)
        {
            var mess = new StringBuilder();
            foreach (var en in (T[])Enum.GetValues(typeof(T)))
            {
                if (values.Contains(en.ToString()))
                    mess.AppendFormat("{0},", en.Convert<int>());
            }
            if (mess.Length > 0) mess.Remove(mess.Length - 1, 1);
            return mess.ToString();
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long GenerateLongId(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            byte[] buffer = Encoding.UTF8.GetBytes(EncryptMd5(input));
            return BitConverter.ToInt64(buffer, 0);
        }
        /// <summary>
        /// 得到MD5加密
        /// </summary>
        /// <returns></returns>
        private static string EncryptMd5(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            byte[] bytHash = md5.ComputeHash(bytValue);
            var sTemp = new StringBuilder();
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp.Append(bytHash[i].ToString("X").PadLeft(2, '0'));
            }
            return sTemp.ToString().ToLower();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GenerateStringId(this string input)
        {
            if(string.IsNullOrEmpty(input))
                return input;
            byte[] buffer = Encoding.Default.GetBytes(input);
            long i = 1;
            foreach (byte b in buffer)
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i -(new DateTime(2000,01,01).Ticks));

        }

        /// <summary>
        /// 获取Unix时间戳
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long UnixTimestamp(this DateTime datetime)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            return (datetime - startTime).TotalSeconds.Convert<long>();
        }
        /// <summary>
        /// Unix时间戳转Datetime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timestamp)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            return startTime.AddSeconds(timestamp);
        }

        /// <summary>
        /// 得到状态名称,返回开启或者停止
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static DateTime GetDefault(this DateTime info)
        {
            return new DateTime(1800, 1, 1);
        }
        #region string
        /// <summary>
        /// 获取指定字符串左边length位
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string input, int length)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return "";
                }
                if (input.Length <= length)
                {
                    return input;
                }
                return input.Substring(0, length);
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 获取指定字符串pos位到最后的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static string RightLeft(this string input, int pos)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return "";
                }
                if (input.Length <= pos)
                {
                    return "";
                }
                return input.Substring(pos);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取指定字符串右边length位
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string input, int length)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return "";
                }
                if (input.Length <= length)
                {
                    return input;
                }
                return input.Substring(input.Length - length);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取指定字符串第一位
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string First(this string input)
        {
            return input.Left(1);
        }
        /// <summary>
        /// 获取指定字符串最后一位
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Last(this string input)
        {
            return input.Right(1);
        }
        /// <summary>
        /// 获取指定字符串两个点之间的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string PositionAt(this string input, int begin, int end)
        {
            if (end < begin) return "";
            return input.RightLeft(begin).Left(end - begin + 1);
        }
        #endregion
        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CopyBySerialize<T>(this T obj) where T : class
        {
            if (obj == null) return default(T);
            var input = obj.SerializeJson();
            return input.DeserializeJson<T>();
        }

    }
}
