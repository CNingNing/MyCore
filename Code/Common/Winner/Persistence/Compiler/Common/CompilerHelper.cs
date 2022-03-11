using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Winner.Persistence.Declare;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Common
{
    public static class CompilerHelper
    {    /// <summary>
         /// 得到3DES
         /// </summary>
         /// <param name="input"></param>
         /// <param name="key"></param>
         /// <returns></returns>
         public static string Decrypt3Des(string input, string key)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var des = TripleDES.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            var desDecrypt = des.CreateDecryptor();
            string result = "";
            try
            {
                byte[] buffer = Convert.FromBase64String(input);
                result = Encoding.UTF8.GetString(desDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch (Exception)
            {

            }
            return result;
        }
        /// <summary>
        /// 得到3DES
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt3Des(string input, string key)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var des = TripleDES.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.Mode = CipherMode.ECB;
            var desEncrypt = des.CreateEncryptor();
            byte[] buffer = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(desEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSearchTableName(this string name, OrmObjectInfo obj, OrmPropertyInfo property)
        {
            var id = GetSearchTableNameIndex(name,obj,property);
            return string.Format("{0}_{1}{2}", obj.SetTableName, property.FieldName, id);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetSearchTableNameIndex(this string name, OrmObjectInfo obj, OrmPropertyInfo property)
        {
            var id = (int)(name.GenerateLongId() % property.SearchTableCount);
            return id;

        }
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
            var md5 = MD5.Create();
            byte[] bytValue = Encoding.UTF8.GetBytes(input);
            byte[] bytHash = md5.ComputeHash(bytValue);
            var sTemp = new StringBuilder();
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp.Append(bytHash[i].ToString("X").PadLeft(2, '0'));
            }
            return sTemp.ToString().ToLower();
        }

        #region 读取和设置属性值
        /// <summary>
        /// 得到属性值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetPropertyType(this Type type, string name)
        {
            if (type == null)
                return null;
            if (name.Contains("."))
            {
                var names = name.Split('.');
                foreach (var s in names)
                {
                    type = type.GetProperty(s)?.PropertyType;
                    if (type == null)
                        return null;
                }
                return type;
            }
            return type.GetProperties().FirstOrDefault(it => it.Name == name)?.PropertyType;
        }
        /// <summary>
        /// 得到属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object GetProperty(this object info, string name)
        {
            if (name.Contains("."))
            {
                return GetRelatePropertyValue(info, name);
            }
            return GetPropertyValue(info, name);
        }

        /// <summary>
        /// 填充属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetProperty(this object info, string name, object value)
        {
            if (name.Contains('.'))
            {
                SetRelatePropertyValue(info, name, value);
            }
            else
            {
                SetPropertyValue(info, name, value);
            }
        }
        /// <summary>
        /// 得到属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static object GetPropertyValue(object info, string name)
        {
            var property = info.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(it => it.Name.Equals(name));
            if (property == null) return null;
            return property.GetValue(info, null);
        }
        /// <summary>
        /// 得到关联属性对象
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static object GetRelatePropertyValue(object info, string name)
        {
            string[] str = name.Split('.');
            object obj = info;
            foreach (string t in str)
            {
                obj = GetPropertyValue(obj, t);
                if (obj == null)
                    return null;
            }
            return obj;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void SetPropertyValue(object info, string name, object value)
        {
            var property = info.GetType()
                               .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                               .FirstOrDefault(it => it.Name.Equals(name));
            if (property == null) return;
            value = value == DBNull.Value
                        ? property.GetValue(info, null)
                        : property.PropertyType.TryConvertValue(value);
            if (value == null) return;
            property.SetValue(info, value, null);
        }

        /// <summary>
        /// 设置关联属性
        /// </summary>
        /// <param name="info"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void SetRelatePropertyValue(object info, string name, object value)
        {
            string[] str = name.Split('.');
            object obj = info;
            for (int i = 0; i < str.Length; i++)
            {
                if (obj == null) break;
                if (i != str.Length - 1)
                    obj = GetAndFillProperty(obj, str[i]);
                else
                    SetPropertyValue(obj, str[i], value);
            }
        }
        /// <summary>
        /// 填充对象属性并返回属性值
        /// </summary>
        public static object GetAndFillProperty(object info, string name)
        {
            object obj = GetPropertyValue(info, name);
            if (obj == null)
            {
                var pi = info.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .FirstOrDefault(it => it.Name.Equals(name));
                obj = pi == null ? null : Activator.CreateInstance(pi.PropertyType);
                SetPropertyValue(info, name, obj);
            }
            return obj;
        }
        /// <summary>
        /// 试着将value转换为type类型的值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object TryConvertValue(this Type type,object value)
        {
            if (value == null) return null;
            if (type == typeof(object)) return value;
            try
            {
                if (type.IsEnum)
                {
                    var chars = type.GetCustomAttributes(typeof (CharEnumAttribute), true);
                    if (chars.Length >0)
                    {
                        var c = Convert.ChangeType(value, typeof(char));
                        var i = Convert.ChangeType(c, typeof(long));
                        if (i != null)
                            return Enum.Parse(type, i.ToString());
                    }
                    return Enum.Parse(type, value.ToString());
                }
                if (type == typeof (bool))
                {
                    if (value.ToString().Equals("1")) return true;
                    if (value.ToString().Equals("0")) return false;
                }
                return Convert.ChangeType(value, type);
            }
            catch
            {
                return value;
            }
        }
       

        #endregion

   


    }
}
