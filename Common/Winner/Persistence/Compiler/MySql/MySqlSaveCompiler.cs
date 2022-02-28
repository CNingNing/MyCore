using System;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Text;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Declare;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.MySql
{
    public class MySqlSaveCompiler : SaveCompiler
    {
        /// <summary>
        /// 
        /// </summary>
        public override string FeildBeforeTag { get; set; } = "`";
        /// <summary>
        /// 
        /// </summary>
        public override string FeildAfterTag { get; set; } = "`";
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void AddParamter(DbCommand command, string name, object value)
        {
            if (string.IsNullOrEmpty(name)) return;
            name = string.Format("@{0}", name);
            if (value == null)
            {
                command.Parameters.Add(new MySqlParameter(name, DBNull.Value));
            }
            else if (value is Array)
            {
                var array = value as Array;
                if (array.Length > 0)
                {
                    var builder = new StringBuilder();
                    for (int i = 0; i < array.Length; i++)
                    {
                        builder.Append(string.Format("{0},", array.GetValue(i)));
                    }
                    command.Parameters.Add(new MySqlParameter(name, builder.ToString()));
                }

            }
            else if (value.GetType().IsEnum)
            {
                var chars = value.GetType().GetCustomAttributes(typeof(CharEnumAttribute), true);
                command.Parameters.Add(chars.Length > 0
                                           ? new MySqlParameter(name, Convert.ChangeType(value, typeof(char)))
                                           : new MySqlParameter(name, value));
            }
            else
            {
                command.Parameters.Add(new MySqlParameter(name, value));
            }

        }
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="property"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void AddParamter(DbCommand command, OrmPropertyInfo property, string name, object value)
        {
            var pa = new MySqlParameter(string.Format("@{0}", name), value);
            if (property != null)
            {
                if (property.Length != 0)
                    pa.Size = property.Length;
                if (!string.IsNullOrEmpty(property.FieldType))
                    pa.MySqlDbType = (MySqlDbType)Enum.Parse(typeof(MySqlDbType), property.FieldType);
            }
            ResetValue(pa);
            command.Parameters.Add(pa);
        }
        /// <summary>
        /// 重新设置值
        /// </summary>
        /// <param name="parameter"></param>
        protected virtual void ResetValue(DbParameter parameter)
        {
            if (parameter.Value == null)
            {
                parameter.Value = DBNull.Value;
                return;
            }
            var type = parameter.Value.GetType();
            if (!type.IsEnum) return;
            if (parameter.DbType != DbType.Int16 && parameter.DbType != DbType.Int32
                && parameter.DbType != DbType.Int64 && parameter.DbType != DbType.Byte
                && parameter.DbType != DbType.UInt16 && parameter.DbType != DbType.UInt32
                && parameter.DbType != DbType.UInt64)
                parameter.Value = Convert.ChangeType(parameter.Value, typeof(char));
        }
 

        protected override object GetKey(SaveCompilerInfo saveCompile)
        {
            if (saveCompile.SaveInfo.Entity.SaveType == SaveType.Add && saveCompile.SaveInfo.Object.PrimaryProperty.IsIdentityKey)
                return ((MySqlCommand)saveCompile.Command).LastInsertedId;
            return base.GetKey(saveCompile);
        }

 

    }
}
