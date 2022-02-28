using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Declare;
using Winner.Persistence.Relation;
using OracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using OracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;
using OracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;

namespace Winner.Persistence.Compiler.Oracle
{
    public class OracleSaveCompiler : SaveCompiler
    {
        private string _paramterFlag = ":";
        /// <summary>
        /// ModelBase的属性名称
        /// </summary>
        public override string ParamterFlag
        {
            get { return _paramterFlag; }
            set { _paramterFlag = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override string FeildBeforeTag { get; set; } = "\"";
        /// <summary>
        /// 
        /// </summary>
        public override string FeildAfterTag { get; set; } = "\"";
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected override void AddParamter(DbCommand command, string name, object value)
        {
            if (string.IsNullOrEmpty(name)) return;
            name = string.Format(":{0}", name);
            if (value == null)
            {
                command.Parameters.Add(new OracleParameter(name, DBNull.Value));
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
                    command.Parameters.Add(new OracleParameter(name, builder.ToString()));
                }

            }
            else if (value.GetType().IsEnum)
            {
                var chars = value.GetType().GetCustomAttributes(typeof(CharEnumAttribute), true);
                command.Parameters.Add(chars.Length > 0
                                           ? new OracleParameter(name, Convert.ChangeType(value, typeof(char)))
                                           : new OracleParameter(name, value));
            }
            else
            {
                command.Parameters.Add(new OracleParameter(name, value));
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
            var pa = new OracleParameter(name, value);
            if (property != null)
            {
                if (property.Length != 0)
                    pa.Size = property.Length;
                if (!string.IsNullOrEmpty(property.FieldType))
                    pa.OracleDbType = (OracleDbType)Enum.Parse(typeof(OracleDbType), property.FieldType);
            }
            if (pa.DbType == DbType.Boolean)
            {
                pa.OracleDbType = OracleDbType.Int16;
                pa.Value =bool.Parse(pa.Value.ToString())? 1 : 0;
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
         

        /// <summary>
        /// 判断是否填充主键
        /// </summary>
        /// <returns></returns>
        protected override bool IsFillKey(SaveCompilerInfo saveCompile, bool isExecute)
        {
            if (!isExecute)
                return base.IsFillKey(saveCompile, true);
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveCompile"></param>
        /// <returns></returns>
        protected override object GetKey(SaveCompilerInfo saveCompile)
        {
            if (saveCompile.SaveInfo.Entity.SaveType == SaveType.Add &&
                saveCompile.SaveInfo.Object.PrimaryProperty.IsIdentityKey)
            {
                var con = saveCompile.Command.Connection as OracleConnection;
                var cmd = new OracleCommand
                {
                    Connection = con,
                    CommandText = $"select {saveCompile.SaveInfo.Object.SetTableName}_Sequence.nextval from dual"
                };
                cmd.BindByName = true;
                var reader = cmd.ExecuteReader();
                reader.Read();
                return reader[0];
            }
            return base.GetKey(saveCompile);
        }

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="saveCompile"></param>
        /// <param name="maps"></param>
        /// <param name="sbName"></param>
        /// <param name="sbValue"></param>
        protected override void AppendAddSqlAndConvertMap(SaveCompilerInfo saveCompile, IList<OrmPropertyInfo> maps, StringBuilder sbName, StringBuilder sbValue)
        {
            int i = 0;
            foreach (OrmPropertyInfo p in saveCompile.SaveInfo.Object.Properties)
            {
                if (!p.AllowAdd ) continue;
                if (p.Map != null)
                    maps.Add(p);
                else
                    AppendAddSql(saveCompile, p, sbName, sbValue, string.Format("Add_{0}", i++));
            }
        }

    }
}
