using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections;
using Winner.Persistence.Compiler.Analysis;
using Winner.Persistence.Exceptions;
using Winner.Persistence.Relation;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Compiler.Common
{
    public class SaveCompiler : ISaveCompiler
    {
        #region 属性
        /// <summary>
        /// 条件实例
        /// </summary>
        public IWhereCompiler WhereCompiler { get; set; }=new WhereCompiler();


        private string _paramterFlag = "@";
        /// <summary>
        /// ModelBase的属性名称
        /// </summary>
        public virtual string ParamterFlag
        {
            get { return _paramterFlag; }
            set { _paramterFlag = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual string FeildBeforeTag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual string FeildAfterTag { get; set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参数
        /// </summary>
        public SaveCompiler()
        { 
        }

        /// <summary>
        /// 属性实例，条件实例，Orm实例，属性实例,主键实例
        /// </summary>
        /// <param name="where"></param>

        public SaveCompiler(IWhereCompiler where)
        {
            WhereCompiler = where;

        }
        #endregion

        #region 接口实现

        /// <summary>
        /// 转换对象
        /// </summary>
        /// <param name="saveCompiler"></param>
        public virtual void Save(SaveCompilerInfo saveCompiler)
        {
            if (saveCompiler == null || saveCompiler.SaveInfo.Entity == null)return;
            switch (saveCompiler.SaveInfo.Entity.SaveType)
            {
                case SaveType.Add: AddInfo(saveCompiler); break;
                case SaveType.Modify: ModifyInfo(saveCompiler); break;
                case SaveType.Remove: DeleteInfo(saveCompiler); break;
                case SaveType.Restore: RestoreInfo(saveCompiler); break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        protected virtual DbDataReader GetDataReader(SaveCompilerInfo saveCompiler,string where)
        {
            
            var field = saveCompiler.SaveInfo.Object.PrimaryProperty.FieldName;
            var tableName = saveCompiler.SaveInfo.Object.SetTableName;
            saveCompiler.Command.CommandText =
                $"select * from {FeildBeforeTag}{tableName}{FeildAfterTag} {where}";
            return saveCompiler.Command.ExecuteReader();
        }
        #endregion

        #region 对象插入
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual IDictionary<OrmPropertyInfo, IList<WordInfo>> GetSearchWords(SaveCompilerInfo saveCompiler)
        {
            if (saveCompiler.SaveInfo.Object.Properties.Count(it => (it.SearchType & (int)OrmSearchType.Search)>0) == 0)
                return null;
            var wordDirctory = new Dictionary<OrmPropertyInfo, IList<WordInfo>>();
            foreach (var property in saveCompiler.SaveInfo.Object.Properties.Where(it => (it.SearchType & (int)OrmSearchType.Search) > 0))
            {
                var value = saveCompiler.SaveInfo.Entity.GetProperty(property.PropertyName);
                var val = value == null ? null : value.ToString();
                var words = Creator.Get<IAnalyzer>().Resolve(property, val);
                if (words != null && words.Count > 0)
                {
                    wordDirctory.Add(property, words);
                }

            }
            return wordDirctory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="wordDirctory"></param>
        /// <returns></returns>
        protected virtual void InsertWords(SaveCompilerInfo saveCompiler, IDictionary<OrmPropertyInfo, IList<WordInfo>> wordDirctory)
        {
            if (wordDirctory==null)
                return;
            foreach (var dic in wordDirctory)
            {
                foreach (var word in dic.Value)
                {
                    var id = saveCompiler.SaveInfo.Entity.GetProperty(saveCompiler.SaveInfo.Object.PrimaryProperty
                        .PropertyName);
                    var wordTableName = word.Name.GetSearchTableName(saveCompiler.SaveInfo.Object, dic.Key);
                    saveCompiler.Command.CommandText = $"insert into {wordTableName} ({FeildBeforeTag}Id{FeildAfterTag},{FeildBeforeTag}Name{FeildAfterTag}) values ('{id}','{word.Name}');";
                    saveCompiler.Command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// 转换插入
        /// </summary>
        /// <param name="saveCompiler"></param>
        protected virtual void AddInfo(SaveCompilerInfo saveCompiler)
        {
            if (saveCompiler.SaveInfo.Entity == null) return;
            var wordDirctory = GetSearchWords(saveCompiler);
            FillKey(saveCompiler, false);
            IList<OrmPropertyInfo> maps = new List<OrmPropertyInfo>();
            saveCompiler.Command.CommandText = GetAddSql(saveCompiler, maps, wordDirctory);
            Execute(saveCompiler);
            InsertWords(saveCompiler,wordDirctory);
            AppendAddMap(saveCompiler, maps);
        }

        /// <summary>
        /// 添加Map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="maps"></param>
        protected virtual void AppendAddMap(SaveCompilerInfo saveCompiler, IList<OrmPropertyInfo> maps)
        {
            foreach (var p in maps)
            {
                ConvertAddMap(saveCompiler, p);
            }
        }

        /// <summary>
        /// 得到添加SQL语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="maps"></param>
        /// <param name="wordDictionary"></param>
        /// <returns></returns>
        protected virtual string GetAddSql(SaveCompilerInfo saveCompiler, IList<OrmPropertyInfo> maps, IDictionary<OrmPropertyInfo, IList<WordInfo>> wordDictionary)
        {
            var sbName = new StringBuilder();
            var sbValue = new StringBuilder();
            AppendAddSqlAndConvertMap(saveCompiler, maps, sbName, sbValue);
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.MarkAddTime))
            {
                var pname = $"_{Guid.NewGuid().ToString("N")}";
                sbName.AppendFormat("{0},", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.MarkAddTime}{FeildAfterTag}");
                sbValue.AppendFormat($"{ParamterFlag}{pname},");
                AddParamter(saveCompiler.Command, null, pname, DateTime.Now);
            }
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.MarkModifyTime))
            {
                var pname = $"_{Guid.NewGuid().ToString("N")}";
                sbName.AppendFormat("{0},", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.MarkModifyTime}{FeildAfterTag}");
                sbValue.AppendFormat($"{ParamterFlag}{pname},");
                AddParamter(saveCompiler.Command, null, pname, DateTime.Now);
            }
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.MarkRemoveTime))
            {
                var pname = $"_{Guid.NewGuid().ToString("N")}";
                sbName.AppendFormat("{0},", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.MarkRemoveTime}{FeildAfterTag}");
                sbValue.AppendFormat($"{ParamterFlag}{pname},");
                AddParamter(saveCompiler.Command, null, pname, DateTime.Now);
            }
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.MarkRestoreTime))
            {
                var pname = $"_{Guid.NewGuid().ToString("N")}";
                sbName.AppendFormat("{0},", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.MarkRestoreTime}{FeildAfterTag}");
                sbValue.AppendFormat($"{ParamterFlag}{pname},");
                AddParamter(saveCompiler.Command, null, pname, DateTime.Now);
            }
            if (saveCompiler.SaveInfo.Object.VersionProperty != null)
            {
                sbName.AppendFormat("{0},", GetFeildName(saveCompiler.SaveInfo.Object.VersionProperty.FieldName));
                sbValue.AppendFormat("0,");
            }
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.Mark))
            {
                sbName.AppendFormat("{0},",$"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.Mark}{FeildAfterTag}");
                sbValue.AppendFormat("1,");
            }

            if (wordDictionary!=null && wordDictionary.Count > 0)
            {
                foreach (var dic in wordDictionary)
                {
                    var indexs = new List<int>();
                    foreach (var word in dic.Value)
                    {
                        indexs.Add(word.Name.GetSearchTableNameIndex(saveCompiler.SaveInfo.Object,dic.Key));
                    }
                    sbName.AppendFormat("{0},", $"{FeildBeforeTag}Search_{dic.Key.FieldName}{FeildAfterTag}");
                    sbValue.AppendFormat($"'{string.Join(",", indexs.Distinct().ToArray())}',");
                }
            }
            sbName.Remove(sbName.Length - 1, 1);
            sbValue.Remove(sbValue.Length - 1, 1);
            var tableName = saveCompiler.SaveInfo.SetTableName;
            return string.Format("insert into {0} ({1}) values ({2})", $"{FeildBeforeTag}{tableName}{FeildAfterTag}", sbName, sbValue);
        }

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="maps"></param>
        /// <param name="sbName"></param>
        /// <param name="sbValue"></param>
        protected virtual void AppendAddSqlAndConvertMap(SaveCompilerInfo saveCompiler,IList<OrmPropertyInfo> maps, StringBuilder sbName, StringBuilder sbValue)
        {
            foreach (OrmPropertyInfo p in saveCompiler.SaveInfo.Object.Properties)
            {
                if (!p.AllowAdd || p.IsIdentityKey) continue;
                if (p.Map != null) 
                    maps.Add(p);
                else
                    AppendAddSql(saveCompiler, p, sbName, sbValue, $"_{Guid.NewGuid().ToString("N")}");
            }
        }

        /// <summary>
        /// 是否插入
        /// </summary>
        /// <param name="property"></param>
        /// <param name="saveCompiler"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        protected virtual bool IsAllowAdd(OrmPropertyInfo property, SaveCompilerInfo saveCompiler, object pValue)
        {
            if (!CheckSaveProperty(saveCompiler, property))
                return false;
            if (pValue == null) return false;
            if (property.IsIdentityKey)
                return false;
            if (property.UnAddValue != null && (property.HasUnAddValue && (property.UnAddValue.Equals(pValue))))
                return false;
          
            return true;
        }

        /// <summary>
        /// 添加插入语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="sbName"></param>
        /// <param name="sbValue"></param>
        /// <param name="pname"></param>
        protected virtual void AppendAddSql(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, StringBuilder sbName, StringBuilder sbValue, string pname)
        {
            object pValue = saveCompiler.SaveInfo.Entity.GetProperty(property.PropertyName);
            if (!IsAllowAdd(property, saveCompiler, pValue))
                return;
            if (!string.IsNullOrWhiteSpace(property.EncryptedKey))
            {
                pValue = CompilerHelper.Encrypt3Des(pValue?.ToString(), property.EncryptedKey);
            }
            sbName.AppendFormat("{0},", GetFeildName(property.FieldName));
            sbValue.AppendFormat("{0}{1},",ParamterFlag, pname);
            AddParamter(saveCompiler.Command, property, pname, pValue);
        }
        #endregion

        #region 对象更新

        protected virtual void UpdateSearchWord(SaveCompilerInfo saveCompiler,string where)
        {
            if (saveCompiler.SaveInfo.Object.Properties.Count(it => (it.SearchType & (int)OrmSearchType.Search) > 0) <= 0) return;
            var wordDirctory = new Dictionary<OrmPropertyInfo, IList<WordInfo>>();
            var removeSql = new List<string>();
            var updateSql = new List<string>();
            using (var reader = GetDataReader(saveCompiler, where))
            {
                while (reader.Read())
                {
                    var id = saveCompiler.SaveInfo.Entity.GetProperty(saveCompiler.SaveInfo.Object.PrimaryProperty
                        .PropertyName);
                    var sql=new StringBuilder();
                    foreach (var property in saveCompiler.SaveInfo.Object.Properties.Where(it => (it.SearchType & (int)OrmSearchType.Search) > 0))
                    {
                        if (saveCompiler.SaveInfo.Entity.Properties != null && saveCompiler.SaveInfo.Entity.Properties.Count(s => s == property.PropertyName) == 0)
                            continue;
                        var indexs = new List<int>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var modifyValue = saveCompiler.SaveInfo.Entity.GetProperty(property.PropertyName)?.ToString();
                            if (reader.GetName(i) != $"{property.FieldName}" || modifyValue == reader.GetName(i))
                                continue;
                            var words = Creator.Get<IAnalyzer>().Resolve(property, modifyValue);
                            if (words != null && words.Count > 0)
                            {
                                wordDirctory.Add(property, words);
                            }
                            removeSql.AddRange(DeleteSearchWord(saveCompiler, reader, property, id));
                            foreach (var word in words)
                            {
                                indexs.Add(word.Name.GetSearchTableNameIndex(saveCompiler.SaveInfo.Object, property));
                            }
                            break;
                        }
                        sql.Append($"{property.FieldName}='{string.Join(",", indexs)}',");
                    }
                    if (sql.Length > 0)
                    {
                        sql.Remove(sql.Length-1, 1);
                        sql.Insert(0, $"update {saveCompiler.SaveInfo.Object.SetTableName} set ");
                        sql.Append($" where Id='{id}'");
                        updateSql.Add(sql.ToString());
                    }

                }
            }
            foreach (var sql in updateSql)
            {
                saveCompiler.Command.CommandText = sql;
                saveCompiler.Command.ExecuteNonQuery();
            }
            foreach (var sql in removeSql)
            {
                saveCompiler.Command.CommandText = sql;
                saveCompiler.Command.ExecuteNonQuery();
            }
            InsertWords(saveCompiler, wordDirctory);
        }
        /// <summary>
        /// 转换更新
        /// </summary>
        /// <param name="saveCompiler"></param>
        protected virtual void ModifyInfo(SaveCompilerInfo saveCompiler)
        {
            if (saveCompiler.SaveInfo.Entity == null) return;
            var where = GetModifyWhereSql(saveCompiler);
            UpdateSearchWord(saveCompiler, where);
           
            if (saveCompiler.SaveInfo.Entity == null) return;
            IList<OrmPropertyInfo> maps = new List<OrmPropertyInfo>();
            saveCompiler.Command.CommandText = GetModifySql(saveCompiler, where, maps);
            Execute(saveCompiler);
            AppendModifyMap(saveCompiler, maps);
        }

        /// <summary>
        /// 添加Map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="maps"></param>
        protected virtual void AppendModifyMap(SaveCompilerInfo saveCompiler,IList<OrmPropertyInfo> maps)
        {
            foreach (var p in maps)
            {
                ConvertModifyMap(saveCompiler, p);
            }
        }

        /// <summary>
        /// 得到更新语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        /// <param name="maps"></param>
        /// <returns></returns>
        protected virtual string GetModifySql(SaveCompilerInfo saveCompiler,string where, IList<OrmPropertyInfo> maps)
        {
            var sql = new StringBuilder();
            var tableName = saveCompiler.SaveInfo.SetTableName;
            sql.AppendFormat("update {0} set ", $"{FeildBeforeTag}{tableName}{FeildAfterTag}");
            AppendSetSqlAndConvertMap(saveCompiler, sql, maps);
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.MarkModifyTime))
            {
                var name = $"_{Guid.NewGuid().ToString("N")}";
                sql.AppendFormat("{0}={1}{2},", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.MarkModifyTime}{FeildAfterTag}",ParamterFlag, name);
                AddParamter(saveCompiler.Command, null, name, DateTime.Now);
            }
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.Mark))
            {
                sql.AppendFormat("{0}=2,", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.Mark}{FeildAfterTag}");
            }
            if (saveCompiler.SaveInfo.Object.VersionProperty != null && saveCompiler.SaveInfo.IsSetVersion)
            {
                sql.AppendFormat("{0}={0}+1,",GetFeildName(saveCompiler.SaveInfo.Object.VersionProperty.FieldName));
            }
            sql.Remove(sql.Length - 1, 1);
            sql.Append(where);
            return sql.ToString();
        }

        /// <summary>
        /// 拼接Set语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="sql"></param>
        /// <param name="pname"></param>
        protected virtual void AppendModifySetSql(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, StringBuilder sql, string pname)
        {
            object pValue = saveCompiler.SaveInfo.Entity.GetProperty(property.PropertyName);
            if (!IsAllowModify(saveCompiler,property, saveCompiler.SaveInfo.Entity, pValue))
                return;
            if (!string.IsNullOrWhiteSpace(property.EncryptedKey))
            {
                pValue = CompilerHelper.Encrypt3Des(pValue?.ToString(), property.EncryptedKey);
            }
            sql.AppendFormat("{0}={1}{2},", GetFeildName(property.FieldName), ParamterFlag, pname);
            AddParamter(saveCompiler.Command, property, pname, pValue);
        }

        /// <summary>
        /// 更新条件
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual string GetModifyWhereSql(SaveCompilerInfo saveCompiler)
        {
            var where = new StringBuilder();
            if (!string.IsNullOrEmpty(saveCompiler.SaveInfo.Entity.WhereExp))
                AppendCustomerWhere(saveCompiler, where);
            else
                AppendPrimaryWhere(saveCompiler, where);
            AppendVesionWhere(saveCompiler, where);
            if(where.Length>0) where.Insert(0, " where (");
            AppendDefaultWhere(where, saveCompiler.SaveInfo.Object);
            return where.ToString();
        }
        /// <summary>
        /// 添加版本控制
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        protected virtual void AppendVesionWhere(SaveCompilerInfo saveCompiler,StringBuilder where)
        {
            if (saveCompiler.SaveInfo.IsSetVersion)
            {
                string pname = $"_{Guid.NewGuid().ToString("N")}";
                var pValue =
                    saveCompiler.SaveInfo.Entity.GetProperty(saveCompiler.SaveInfo.Object.VersionProperty.PropertyName);
                if (where.Length > 0)
                    where.Append(" and ");
                where.AppendFormat("{0}={1}{2}", GetFeildName(saveCompiler.SaveInfo.Object.VersionProperty.FieldName), ParamterFlag, pname);
                AddParamter(saveCompiler.Command, saveCompiler.SaveInfo.Object.VersionProperty, pname, pValue);
            }
        }
        /// <summary>
        /// 添加set语句和转换Map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="sql"></param>
        /// <param name="maps"></param>
        protected virtual void AppendSetSqlAndConvertMap(SaveCompilerInfo saveCompiler, StringBuilder sql, IList<OrmPropertyInfo> maps)
        {
            foreach (var p in saveCompiler.SaveInfo.Object.Properties)
            {
                if (!p.AllowModify) continue;
                if (p.Map != null)
                    maps.Add(p);
                else
                    AppendModifySetSql(saveCompiler, p, sql, $"_{Guid.NewGuid().ToString("N")}");
            }
        }

        /// <summary>
        /// 得到是否更新
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="information"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        protected virtual bool IsAllowModify(SaveCompilerInfo saveCompiler, OrmPropertyInfo property,  EntityInfo information, object pValue)
        {
            if (!CheckSaveProperty(saveCompiler, property))
                return false;
            if (property.IsPrimaryKey || property.IsCustom ||  information.Properties != null && !property.InProperties(information.Properties))
            {
                return false;
            }
            if (property.UnModifyValue != null && (property.HasUnModifyValue &&(property.UnModifyValue.Equals(pValue))))
            {
                return false;
            }
           
            return true;
        }


        #endregion

        #region 对象删除

        protected virtual object GetSearchId(SaveCompilerInfo saveCompiler, DbDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) != $"Search_{saveCompiler.SaveInfo.Object.PrimaryProperty.FieldName}")
                    return reader.GetValue(i);
            }
            return null;
        }

        protected virtual IList<string> DeleteSearchWord(SaveCompilerInfo saveCompiler,DbDataReader reader, OrmPropertyInfo property,object id)
        {
            var result = new List<string>();
            var field = saveCompiler.SaveInfo.Object.PrimaryProperty.FieldName;
            var tableName = saveCompiler.SaveInfo.Object.SetTableName;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) != $"Search_{property.FieldName}")
                    continue;
                var value = reader.GetString(i);
                if (string.IsNullOrWhiteSpace(value))
                    continue;
                var indexs = value.Split(',');
                foreach (var index in indexs)
                {
                    result.Add($"delete from {tableName}_{property.FieldName}{index} where {FeildBeforeTag}{field}{FeildAfterTag}='{id}'");
                }
                break;
            }
            return result;
        }

        protected virtual void DeleteSearchWord(SaveCompilerInfo saveCompiler,string where)
        {
            if (saveCompiler.SaveInfo.Object.Properties.Count(it => (it.SearchType & (int)OrmSearchType.Search) > 0) <= 0) return;
            var result=new List<string>();
            using (var reader = GetDataReader(saveCompiler, where))
            {
                while (reader.Read())
                {
                    var id = saveCompiler.SaveInfo.Entity.GetProperty(saveCompiler.SaveInfo.Object.PrimaryProperty
                        .PropertyName);
                    foreach (var property in saveCompiler.SaveInfo.Object.Properties.Where(it => (it.SearchType & (int)OrmSearchType.Search) > 0))
                    {
                        result.AddRange(DeleteSearchWord(saveCompiler, reader, property, id));
                    }
                }
            }
            foreach (var sql in result)
            {
                saveCompiler.Command.CommandText = sql;
                saveCompiler.Command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 转换删除
        /// </summary>
        /// <param name="saveCompiler"></param>
        protected virtual void DeleteInfo(SaveCompilerInfo saveCompiler)
        {
            if (saveCompiler.SaveInfo.Entity == null) return;
            var deleteSql = GetDeleteSql(saveCompiler);
            string where = GetDeleteWhereSql(saveCompiler);
            DeleteSearchWord(saveCompiler, where); 
            saveCompiler.Command.CommandText = $"{deleteSql} {where}";
            ConvertDeleteMap(saveCompiler, where);
            Execute(saveCompiler);
         
        }

        /// <summary>
        /// 得到删除语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        protected virtual string GetDeleteSql(SaveCompilerInfo saveCompiler)
        {
            var sql = new StringBuilder();
            var tableName = saveCompiler.SaveInfo.SetTableName;
            sql.Append(string.IsNullOrEmpty(saveCompiler.SaveInfo.Object.Mark)
                           ? string.Format("delete from {0} ", $"{FeildBeforeTag}{tableName}{FeildAfterTag}")
                           : GetDeleteMarkSql(saveCompiler));
            return sql.ToString();
        }

        /// <summary>
        /// 得到标记删除语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual string GetDeleteMarkSql(SaveCompilerInfo saveCompiler)
        {
            var sql=new StringBuilder();
            var tableName = saveCompiler.SaveInfo.SetTableName;
            sql.AppendFormat("update {0} set {1} ", $"{FeildBeforeTag}{tableName}{FeildAfterTag}", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.Mark}{FeildAfterTag}=0");
            foreach (var property in saveCompiler.SaveInfo.Object.Properties.Where(property => property.AllowRemove))
            {
                if(!CheckSaveProperty(saveCompiler, property) || property.Map==null)
                    continue;
                var pName = $"_{Guid.NewGuid().ToString("N")}";
                sql.AppendFormat(",{0}={1}{2} ", GetFeildName(property.FieldName), ParamterFlag, pName);
                AddParamter(saveCompiler.Command, property, pName, saveCompiler.SaveInfo.Entity.GetProperty(property.PropertyName));
            }
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.MarkRemoveTime))
            {
                var pName = $"_{Guid.NewGuid().ToString("N")}";
                sql.AppendFormat(",{0}={1}{2} ", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.MarkRemoveTime}{FeildAfterTag}", ParamterFlag, pName);
                AddParamter(saveCompiler.Command, null, pName, DateTime.Now);
            }
            return sql.ToString();
        }

        /// <summary>
        /// 得到删除的where语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual string GetDeleteWhereSql(SaveCompilerInfo saveCompiler)
        {
            var where = new StringBuilder();
            if (!string.IsNullOrEmpty(saveCompiler.SaveInfo.Entity.WhereExp))
                AppendCustomerWhere(saveCompiler, where);
            else
                AppendPrimaryWhere(saveCompiler, where);
            if (where.Length > 0) where.Insert(0, " where (");
            AppendDefaultWhere(where, saveCompiler.SaveInfo.Object);
            return where.ToString();
        }
         
    
        #endregion

        #region 对象还原

        /// <summary>
        /// 转换删除
        /// </summary>
        /// <param name="saveCompiler"></param>
        protected virtual void RestoreInfo(SaveCompilerInfo saveCompiler)
        {
            if (saveCompiler.SaveInfo.Entity == null)return;
            if (string.IsNullOrEmpty(saveCompiler.SaveInfo.Object.Mark)) return;
            string where = GetRestoreWhereSql(saveCompiler);
            saveCompiler.Command.CommandText = GetRestoreSql(saveCompiler, where);
            ConvertRestoreMap(saveCompiler, where);
            Execute(saveCompiler);
        }

        /// <summary>
        /// 得到还原语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        protected virtual string GetRestoreSql(SaveCompilerInfo saveCompiler, string where)
        {
            var sql = new StringBuilder();
            sql.Append(GetRestoreMarkSql(saveCompiler));
            if (!string.IsNullOrEmpty(where))
                sql.Append(where.ToString(CultureInfo.InvariantCulture));
            return sql.ToString();
        }

        /// <summary>
        /// 得到标记还原语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual string GetRestoreMarkSql(SaveCompilerInfo saveCompiler)
        {
            var sql = new StringBuilder();
            var tableName = saveCompiler.SaveInfo.SetTableName;
            sql.AppendFormat("update {0} set {1} ", $"{FeildBeforeTag}{tableName}{FeildAfterTag}",
                $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.Mark}{FeildAfterTag}=1");
            foreach (var property in saveCompiler.SaveInfo.Object.Properties.Where(property => property.AllowRestore))
            {
                if (!CheckSaveProperty(saveCompiler, property))
                    continue;
                var pname = $"_{Guid.NewGuid().ToString("N")}";
                sql.AppendFormat(",{0}={1}{2} ", GetFeildName(property.FieldName),ParamterFlag, pname);
                AddParamter(saveCompiler.Command, property, pname, saveCompiler.SaveInfo.Entity.GetProperty(property.PropertyName));
            }
            if (!string.IsNullOrWhiteSpace(saveCompiler.SaveInfo.Object.MarkRestoreTime))
            {
                var pname = $"_{Guid.NewGuid().ToString("N")}";
                sql.AppendFormat(",{0}={1}{2}", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.MarkRestoreTime}{FeildAfterTag}", ParamterFlag, pname);
                AddParamter(saveCompiler.Command, null, pname, DateTime.Now);
            }
            return sql.ToString();
        }

        /// <summary>
        /// 得到还原的where语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual string GetRestoreWhereSql(SaveCompilerInfo saveCompiler)
        {
            var where = new StringBuilder();
            if (!string.IsNullOrEmpty(saveCompiler.SaveInfo.Entity.WhereExp))
                AppendCustomerWhere(saveCompiler, where);
            else
                AppendPrimaryWhere(saveCompiler, where);
            if (where.Length > 0)
                where.Insert(0, " where (");
            AppendDefaultWhere(where, saveCompiler.SaveInfo.Object);
            return where.ToString();
        }


        #endregion

        #region 转换添加Map

        /// <summary>
        /// 转换Map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        protected virtual void ConvertAddMap(SaveCompilerInfo saveCompiler, OrmPropertyInfo property)
        {
            if (property.Map == null || !property.Map.IsAdd) return;
            var p = saveCompiler.SaveInfo.Entity.GetType().GetProperties().FirstOrDefault(it => it.Name.Equals(property.PropertyName));
            if (p == null) return;
            var pValue = p.GetValue(saveCompiler.SaveInfo.Entity, null);
            if (pValue != null && !saveCompiler.IsInMap(property))
            {
                saveCompiler.MapProperties.Add(property);
                SelectAddMap(saveCompiler, property, pValue);
            }
        }

        /// <summary>
        /// 选择添加Map的执行方式
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="pValue"></param>
        protected virtual void SelectAddMap(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, object pValue)
        {
            switch (property.Map.MapType)
            {
                case OrmMapType.OneToOne:
                    AddOneToOneMap(saveCompiler, property, pValue);
                    break;
                case OrmMapType.OneToMany:
                    AddOneToManyMap(saveCompiler, property, pValue);
                    break;
            }
        }

        /// <summary>
        /// 添加1对1关系map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="pValue"></param>
        protected virtual void AddOneToOneMap(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, object pValue)
        {
            var pk = saveCompiler.SaveInfo.Entity.GetType().GetProperty(property.Map.ObjectProperty.PropertyName).GetValue(saveCompiler.SaveInfo.Entity, null);
            FillForeignProperty(pValue,property, pk);
            AddMapInfo(saveCompiler, property, pValue);
        }

        /// <summary>
        /// 添加1对多关系map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="pValue"></param>
        protected virtual void AddOneToManyMap(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, object pValue)
        {
            System.Reflection.MethodInfo method = pValue.GetType().GetMethod("GetEnumerator");
            object pk = saveCompiler.SaveInfo.Entity.GetType().GetProperty(property.Map.ObjectProperty.PropertyName).GetValue(saveCompiler.SaveInfo.Entity, null);
            if (method != null)
            {
                FillOneToManyMap(saveCompiler, property, pValue, method, pk);
            }
        }

        /// <summary>
        /// 填充1对多关系map对象
        /// </summary>
        /// <param name="saveCompiler"/>
        /// <param name="property"></param>
        /// <param name="pValue"></param>
        /// <param name="method"></param>
        /// <param name="pk"></param>
        protected virtual void FillOneToManyMap(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, object pValue, System.Reflection.MethodInfo method, object pk)
        {
            var etor = (IEnumerator)method.Invoke(pValue, null);
            while (etor.MoveNext())
            {
                if (etor.Current == null)
                    continue;
                FillForeignProperty(etor.Current, property, pk);
                etor.Current.GetType().GetProperty(property.Map.MapObjectProperty.PropertyName).SetValue(etor.Current, pk, null);
                AddMapInfo(saveCompiler, property, etor.Current);
            }
        }

        /// <summary>
        /// 填充外键
        /// </summary>
        /// <param name="info"></param>
        /// <param name="property"></param>
        /// <param name="pk"></param>
        protected virtual void FillForeignProperty(object info, OrmPropertyInfo property, object pk)
        {
            var propertyType =
                info.GetType()
                    .GetProperties()
                    .FirstOrDefault(it => it.Name.Equals(property.Map.MapObjectProperty.PropertyName));
            if (propertyType == null) return;
            if (propertyType.PropertyType.IsValueType && !pk.Equals(0)
                || !propertyType.PropertyType.IsValueType && pk != null) return;
            propertyType.SetValue(info, pk, null);
        }

        /// <summary>
        /// 添加map信息
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="entity"></param>
        protected virtual void AddMapInfo(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, object entity)
        {
            if (((EntityInfo)entity).SaveType != SaveType.Modify || saveCompiler.ContentEntities.Contains(entity)) return;
            var mapSaveCompiler = new SaveCompilerInfo
                {
                    Command = saveCompiler.Command,
                    MapProperties = saveCompiler.MapProperties,
                    ContentEntities = saveCompiler.ContentEntities,
                    SaveInfo=new SaveInfo
                        {
                            Entity = (EntityInfo)entity,
                            Object = property.Map.GetMapObject()
                        }
             };
            AddInfo(mapSaveCompiler);
        }
       

        #endregion

        #region 转换更新Map

        /// <summary>
        /// 更新map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        protected virtual void ConvertModifyMap(SaveCompilerInfo saveCompiler, OrmPropertyInfo property)
        {
            if (property.Map == null || !property.Map.IsModify) return;
            var p = saveCompiler.SaveInfo.Entity.GetType().GetProperties().FirstOrDefault(it => it.Name.Equals(property.PropertyName));
            if (p == null) return;
            var pValue = p.GetValue(saveCompiler.SaveInfo.Entity, null);
            if (pValue != null && !saveCompiler.IsInMap(property))
            {
                saveCompiler.MapProperties.Add(property);
                SelectModifyMap(saveCompiler, pValue, property);
            }
        }

        /// <summary>
        /// 选择更新map方式
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="pValue"></param>
        /// <param name="property"></param>
        protected virtual void SelectModifyMap(SaveCompilerInfo saveCompiler, object pValue, OrmPropertyInfo property)
        {
            switch (property.Map.MapType)
            {
                case OrmMapType.OneToOne:
                    ModifyMapInfo(saveCompiler, property, pValue);
                    break;
                case OrmMapType.OneToMany:
                    ModifyOneToManyMap(saveCompiler, property, pValue);
                    break;
            }
        }

        /// <summary>
        /// 执行1对多map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="pValue"></param>
        protected virtual void ModifyOneToManyMap(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, object pValue)
        {
            System.Reflection.MethodInfo method = pValue.GetType().GetMethod("GetEnumerator");
            if (method != null)
            {
                var etor = (IEnumerator)method.Invoke(pValue, null);
                while (etor.MoveNext())
                {
                    ModifyMapInfo(saveCompiler, property, etor.Current);
                }
            }
        }

        /// <summary>
        /// 添加map信息
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        protected virtual void ModifyMapInfo(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, object entity)
        {
            var mapModel = ((EntityInfo) entity);
            if (((EntityInfo)entity).SaveType != SaveType.Modify || saveCompiler.ContentEntities.Contains(entity)) return;
            var mapSaveCompiler = new SaveCompilerInfo
            {
                Command = saveCompiler.Command,
                MapProperties = saveCompiler.MapProperties,
                ContentEntities = saveCompiler.ContentEntities,
                SaveInfo=new SaveInfo
                        {
                            TableIndex = saveCompiler.SaveInfo.TableIndex,
                            Entity = mapModel,
                            Object = property.Map.GetMapObject()
                        }
            };
            ModifyInfo(mapSaveCompiler);
        }

        #endregion

        #region 转换删除Map

        /// <summary>
        /// 删除map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        protected virtual void ConvertDeleteMap(SaveCompilerInfo saveCompiler, string where)
        {
            foreach (var p in saveCompiler.SaveInfo.Object.Properties)
            {
                SelectDeleteMap(saveCompiler, where, p);
            }
        }

        /// <summary>
        /// 选择删除Map方式
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        /// <param name="property"></param>
        protected virtual void SelectDeleteMap(SaveCompilerInfo saveCompiler, string where, OrmPropertyInfo property)
        {
            if (property.Map == null || !property.Map.IsRemove && !saveCompiler.IsInMap(property)) return;
            saveCompiler.MapProperties.Add(property);
            AppendDeleteMapSql(saveCompiler, property, @where);
            var mapSaveCompiler = new SaveCompilerInfo
            {
                Command = saveCompiler.Command,
                MapProperties = saveCompiler.MapProperties,
                SaveInfo = new SaveInfo
                {
                    Entity = saveCompiler.SaveInfo.Entity,
                    Object = property.Map.GetMapObject(),
                    TableIndex=saveCompiler.SaveInfo.TableIndex
                }
            };
            ConvertDeleteMap(mapSaveCompiler, GetDeleteMapWhere(saveCompiler, property, @where));
        }

        /// <summary>
        /// 得到map删除条件
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        protected virtual string GetDeleteMapWhere(SaveCompilerInfo saveCompiler, OrmPropertyInfo property,string where)
        {
            var sbwhere = new StringBuilder();
            sbwhere.AppendFormat(" where {0} in ( select ", GetFeildName(property.Map.MapObjectProperty.FieldName));
            sbwhere.AppendFormat("{0} from ", GetFeildName(property.Map.ObjectProperty.FieldName));
            sbwhere.AppendFormat("{0} {1} ", $"{FeildBeforeTag}{saveCompiler.SaveInfo.SetTableName}{FeildAfterTag}", where);
            AppendDefaultWhere(sbwhere,property.Map.GetMapObject());
            return sbwhere.ToString();
        }

        /// <summary>
        /// 添加map删除语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="where"></param>
        protected virtual void AppendDeleteMapSql(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, string where)
        {
            saveCompiler.Command.CommandText = string.Format("{0};{1}", GetDeleteMapSql(saveCompiler, property, @where), saveCompiler.Command.CommandText);
        }

        /// <summary>
        /// 得到删除map的语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        protected virtual string GetDeleteMapSql(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, string where)
        {
            var sql = new StringBuilder();
            var mapObject = property.Map.GetMapObject();
            var tableName =$"{FeildBeforeTag}{saveCompiler.SaveInfo.GetSetTableName(mapObject)}{FeildAfterTag}" ;
            if (string.IsNullOrWhiteSpace(mapObject.Mark))
            {
                sql.Append(string.Format("delete from {0}  ", tableName));
            }
            else
            {
                sql.Append($"update {tableName} set {FeildBeforeTag}{property.Map.GetMapObject().Mark}{FeildAfterTag}=0 ");
                if (!string.IsNullOrWhiteSpace(mapObject.MarkRemoveTime))
                {
                    var pname = $"_{Guid.NewGuid().ToString("N")}";
                    sql.AppendFormat(",{0}={1}{2} ", $"{FeildBeforeTag}{mapObject.MarkRemoveTime}{FeildAfterTag}", ParamterFlag, pname);
                    AddParamter(saveCompiler.Command, null, pname, DateTime.Now);
                }
            }
            sql.AppendFormat("where {0} in ( select ", GetFeildName(property.Map.MapObjectProperty.FieldName));
            sql.AppendFormat("{0} from ", GetFeildName(property.Map.ObjectProperty.FieldName));
            sql.AppendFormat("{0} {1}) ", $"{FeildBeforeTag}{saveCompiler.SaveInfo.SetTableName}{FeildAfterTag}" , where);
            return sql.ToString();
        }
        #endregion

        #region 转换还原Map

        /// <summary>
        /// 还原map
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        protected virtual void ConvertRestoreMap(SaveCompilerInfo saveCompiler, string where)
        {
            foreach (var p in saveCompiler.SaveInfo.Object.Properties)
            {
                if (p.Map != null)
                {
                    SelectRestoreMap(saveCompiler, where, p);
                }
            }
        }

        /// <summary>
        /// 选择还原Map方式
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        /// <param name="property"></param>
        protected virtual void SelectRestoreMap(SaveCompilerInfo saveCompiler, string where, OrmPropertyInfo property)
        {
            if (property.Map == null || !property.Map.IsRestore && !saveCompiler.IsInMap(property)) return;
            saveCompiler.MapProperties.Add(property);
            AppendRestoreMapSql(saveCompiler, property, @where);
            var mapSaveCompiler = new SaveCompilerInfo
            {
                Command = saveCompiler.Command,
                MapProperties = saveCompiler.MapProperties,
                  SaveInfo = new SaveInfo
                {
                    Entity = saveCompiler.SaveInfo.Entity,
                    Object = property.Map.GetMapObject()
                }
            };
            ConvertRestoreMap(mapSaveCompiler, GetRestoreMapWhere(saveCompiler, property, @where));
        }

        /// <summary>
        /// 得到map删除条件
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        protected virtual string GetRestoreMapWhere(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, string where)
        {
            var sbwhere = new StringBuilder();
            sbwhere.AppendFormat(" where {0} in ( select ", GetFeildName(property.Map.MapObjectProperty.FieldName));
            sbwhere.AppendFormat("{0} from ", GetFeildName(property.Map.ObjectProperty.FieldName));
            sbwhere.AppendFormat("{0} {1} ", $"{FeildBeforeTag}{saveCompiler.SaveInfo.SetTableName}{FeildAfterTag}" , where);
            AppendDefaultWhere(sbwhere, property.Map.GetMapObject());
            return sbwhere.ToString();
        }

        /// <summary>
        /// 添加map删除语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="where"></param>
        protected virtual void AppendRestoreMapSql(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, string where)
        {
            saveCompiler.Command.CommandText = string.Format("{0};{1}", GetRestoreMapSql(saveCompiler, property, @where), saveCompiler.Command.CommandText);
        }

        /// <summary>
        /// 得到删除map的语句
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        protected virtual string GetRestoreMapSql(SaveCompilerInfo saveCompiler, OrmPropertyInfo property, string where)
        {
            var obj = property.Map.GetMapObject();
            var sql = new StringBuilder();
            var mapObject = property.Map.GetMapObject();
            var tableName = $"{FeildBeforeTag}{saveCompiler.SaveInfo.GetSetTableName(mapObject)}{FeildAfterTag}"; 
            sql.AppendFormat("update {0} set {1}=1 ", tableName, $"{FeildBeforeTag}{obj.Mark}{FeildAfterTag}");
            if (!string.IsNullOrWhiteSpace(obj.MarkRestoreTime))
            {
                var pname = $"_{Guid.NewGuid().ToString("N")}";
                sql.AppendFormat(",{0}={1}{2}", $"{FeildBeforeTag}{obj.MarkRestoreTime}{FeildAfterTag}", ParamterFlag, pname);
                AddParamter(saveCompiler.Command, null, pname, DateTime.Now);
            }
            sql.AppendFormat("where {0} in ( select ", GetFeildName(property.Map.MapObjectProperty.FieldName));
            sql.AppendFormat("{0} from ", GetFeildName(property.Map.ObjectProperty.FieldName));
            sql.AppendFormat("{0} {1}) ", $"{FeildBeforeTag}{saveCompiler.SaveInfo.SetTableName}{FeildAfterTag}" , where);
            return sql.ToString();
        }
        #endregion

        #region 设置条件和参数

    
        /// <summary>
        /// 添加主键条件
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        protected virtual void AppendPrimaryWhere(SaveCompilerInfo saveCompiler, StringBuilder where)
        {
            object pvalue = saveCompiler.SaveInfo.Entity.GetProperty(saveCompiler.SaveInfo.Object.PrimaryProperty.PropertyName);
            if(pvalue==null) return;
            string pname = $"_{Guid.NewGuid().ToString("N")}";
            where.AppendFormat("{0}={1}{2}", $"{FeildBeforeTag}{saveCompiler.SaveInfo.Object.PrimaryProperty.PropertyName}{FeildAfterTag}", ParamterFlag, pname);
            AddParamter(saveCompiler.Command, saveCompiler.SaveInfo.Object.PrimaryProperty, pname, pvalue);
        }
        /// <summary>
        /// 设置默认查询
        /// </summary>
        /// <param name="where"></param>
        /// <param name="obj"></param>
        protected virtual void AppendDefaultWhere(StringBuilder where, OrmObjectInfo obj)
        {
            if (where.Length > 0) where.Append(")");
            if (string.IsNullOrEmpty(obj.Mark))//设置默认查询
                return;
            string key = where.Length > 0 ? "and" : "where";
            where.AppendFormat(" {0} {1}", key, $"{FeildBeforeTag}{obj.Mark}{FeildAfterTag}>0");
        }

        /// <summary>
        /// 拼接自定义条件
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="where"></param>
        protected virtual void AppendCustomerWhere(SaveCompilerInfo saveCompiler, StringBuilder where)
        {
            var whereCompile = new WhereCompilerInfo(saveCompiler.SaveInfo.Object,
                                                     saveCompiler.SaveInfo.Entity.WhereExp, new TableInfo { Joins = new Dictionary<string, JoinInfo>() },
                                                     new StringBuilder(),FeildBeforeTag,FeildAfterTag,true,saveCompiler){Query=new QueryInfo{Object=saveCompiler.SaveInfo.Object,Parameters = saveCompiler.SaveInfo.Entity.Parameters } };

            WhereCompiler.Translate(whereCompile);
            if (whereCompile.Query.SqlParameters != null)
            {
                foreach (var sqlParameter in whereCompile.Query.SqlParameters)
                {
                    AddParamter(saveCompiler.Command,sqlParameter.Key,sqlParameter.Value);
                }
            }
            where.Append(whereCompile.Builder);
            saveCompiler.IsSaveParameters = true;
        }

        #endregion

        #region 执行

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual int Execute(SaveCompilerInfo saveCompiler)
        {
            if (string.IsNullOrEmpty(saveCompiler.Command.CommandText)) return 0;
            SaveParameters(saveCompiler);
            var rev = saveCompiler.Command.ExecuteNonQuery();
            if (saveCompiler.SaveInfo.Entity.SaveType == SaveType.Modify && saveCompiler.SaveInfo.IsSetVersion && rev<=0)
            {
                throw new VersionException("Version Expired");
            }
            FillKey(saveCompiler, true);
            saveCompiler.Command.Parameters.Clear();
            saveCompiler.Command.CommandText = null;
            return rev;
        }
        /// <summary>
        /// 存储参数
        /// </summary>
        /// <param name="saveCompiler"></param>
        protected virtual void SaveParameters(SaveCompilerInfo saveCompiler)
        {
            if (saveCompiler.SaveInfo.Entity.Parameters == null || saveCompiler.IsSaveParameters) return;
            foreach (var parameter in saveCompiler.SaveInfo.Entity.Parameters)
            {
                AddParamter(saveCompiler.Command, parameter.Key, parameter.Value);
            }
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="property"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual void AddParamter(DbCommand command, OrmPropertyInfo property,string name,object value)
        {
            command.Parameters.Add(value);
        }
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual  void AddParamter(DbCommand command, string name, object value)
        {
            command.Parameters.Add(value);
        }

        /// <summary>
        /// 填充主键
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="isExecute"></param>
        protected virtual void FillKey(SaveCompilerInfo saveCompiler, bool isExecute)
        {
            if (!IsFillKey(saveCompiler, isExecute)) return;
            object pValue = GetKey(saveCompiler);
            saveCompiler.SaveInfo.Entity.SetProperty(saveCompiler.SaveInfo.Object.PrimaryProperty.PropertyName, pValue);
            if (saveCompiler.SaveInfo.Entity.Properties != null)
                saveCompiler.SaveInfo.Entity.SetProperty(saveCompiler.SaveInfo.Object.PrimaryProperty.PropertyName);
        }

        /// <summary>
        /// 得到主键
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <returns></returns>
        protected virtual object GetKey(SaveCompilerInfo saveCompiler)
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }

        /// <summary>
        /// 判断是否填充主键
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsFillKey(SaveCompilerInfo saveCompiler, bool isExecute)
        {
            if (saveCompiler.SaveInfo.Entity.SaveType != SaveType.Add) return false;
            if (!isExecute && !saveCompiler.SaveInfo.Object.PrimaryProperty.IsIdentityKey)
            {
                var propertyType = saveCompiler.SaveInfo.Entity.GetType().GetProperties()
                   .FirstOrDefault(it => it.Name.Equals(saveCompiler.SaveInfo.Object.PrimaryProperty.PropertyName));
                if (propertyType == null) return false;
                var propertyValue = propertyType.GetValue(saveCompiler.SaveInfo.Entity, null);
                if (propertyType.PropertyType.IsValueType && !propertyValue.Equals(0)
                    || !propertyType.PropertyType.IsValueType && 
                    propertyValue != null && !"".Equals(propertyValue)) return false;
                return true;

            }
            return isExecute && saveCompiler.SaveInfo.Object.PrimaryProperty.IsIdentityKey;
        }
        /// <summary>
        /// 得到字段名
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected virtual string GetFeildName(string fieldName)
        {
            return $"{FeildBeforeTag}{fieldName}{FeildAfterTag}";
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveCompiler"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual bool CheckSaveProperty(SaveCompilerInfo saveCompiler, OrmPropertyInfo property)
        {
            if (property.PropertyName == saveCompiler.SaveInfo.Object.VersionProperty?.PropertyName ||
                property.PropertyName == saveCompiler.SaveInfo.Object.Mark ||
                property.PropertyName == saveCompiler.SaveInfo.Object.MarkAddTime
                || property.PropertyName == saveCompiler.SaveInfo.Object.MarkModifyTime
                || property.PropertyName == saveCompiler.SaveInfo.Object.MarkRemoveTime
                || property.PropertyName == saveCompiler.SaveInfo.Object.MarkRestoreTime)
                return false;
            return true;
        }
    }
}
