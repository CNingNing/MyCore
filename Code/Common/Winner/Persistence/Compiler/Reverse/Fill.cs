using System;
using System.Data;
using System.Collections;
using System.Linq;
using System.Xml;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Relation;
namespace Winner.Persistence.Compiler.Reverse
{
    public class Fill : IFill
    {
      
        #region 接口的实现

        /// <summary>
        /// 填充返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        public virtual T Reverse<T>(IDataReader reader, OrmObjectInfo obj)
        {
            var type = Type.GetType(obj.ObjectName);
            var rev = new ArrayList();
            while (reader.Read())
            {
                var value = GetOrmObjectElement(type, reader, obj);
                if (value != null) rev.Add(value);
            }
            return (T)((object)rev.ToArray(type));
        }
        /// <summary>
        /// 填充对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual T Reverse<T>(DataTable table, OrmObjectInfo obj)
        {
            var type = Type.GetType(obj.ObjectName);
            var rev = new ArrayList();
            foreach (DataRow row in table.Rows.AsParallel())
            {
                var value = GetOrmObjectElement(type, table, row, obj);
                if (value != null) rev.Add(value);
            }
            return (T)((object)rev.ToArray(type));
        }

        #endregion

        #region 填充返回结果

        #region  datareader
     

        /// <summary>
        /// 添加Orm对象
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        protected virtual object GetOrmObjectElement(Type elementType, IDataReader reader, OrmObjectInfo obj)
        {
            var entity = Activator.CreateInstance(elementType);//是否添加实例
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = GetPropertyName(reader.GetName(i));
                SetOrmObjectField(entity, name, reader[i], obj);
            }
            return entity;
        }

        /// <summary>
        /// 添加Orm对象
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="table"></param>
        /// <param name="row"></param>
        /// <param name="obj"></param>
        protected virtual object GetOrmObjectElement(Type elementType, DataTable table,DataRow row, OrmObjectInfo obj)
        {
            var entity = Activator.CreateInstance(elementType);//是否添加实例
            foreach (DataColumn column in table.Columns)
            {
                var name = GetPropertyName(column.ColumnName);
                SetOrmObjectField(entity, name, row[column.ColumnName], obj);
            }
          
            return entity;
        }




        #endregion

        /// <summary>
        /// 添加OrmObject的Filed
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="obj"></param>
        protected virtual void SetOrmObjectField(object entity, string propertyName, object value, OrmObjectInfo obj)
        {
            var chainPropertys = obj.GetChainProperties(propertyName);
            OrmPropertyInfo property = chainPropertys.Count > 0 ? chainPropertys[chainPropertys.Count - 1] : null;
            if (property == null)
            {
                entity.SetProperty(propertyName, value);
                return;
            }
            if (!property.AllowRead) return;
            if (value == DBNull.Value && property.HasReadNullValue)
                entity.SetProperty(propertyName, property.ReadNullValue);
            else if (property.Map != null && property.Map.MapType == OrmMapType.OneToMany)
                SetManyOrmProperty(entity, propertyName, value.ToString());
            else
            {
                if (!string.IsNullOrWhiteSpace(property.EncryptedKey))
                {
                    value = CompilerHelper.Decrypt3Des(value?.ToString(), property.EncryptedKey);
                }
                entity.SetProperty(propertyName, value);
            }
        }
        /// <summary>
        /// 得到属性名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual string GetPropertyName(string name)
        {
            var index = name.LastIndexOf('_');
            if (index == -1)
                return name;
            return name.Substring(0, index).Replace("_", ".");
        }

        /// <summary>
        /// 得到填充对象
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual void SetManyOrmProperty(object entity, string propertyName, string value)
        {
            if(string.IsNullOrEmpty(value))return;
            var subEntity = entity;
            var names = propertyName.Split('.');
            for (int i = 0; i < names.Length-1; i++)
            {
                subEntity = CompilerHelper.GetAndFillProperty(subEntity, names[i]);
            }
            var property =
                subEntity.GetType().GetProperties().FirstOrDefault(it => it.Name.Equals(names[names.Length-1]));
            if(property==null)return;
            var elements = new ArrayList();
            var elementType = GetPropertyElementType(property.PropertyType);
            var doc = new XmlDocument();
            doc.LoadXml(string.Format("<sysroot>{0}</sysroot>",value));
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                var element = Activator.CreateInstance(elementType);
                foreach (XmlNode nd in node.ChildNodes)
                {
                    var name = GetPropertyName(nd.Name);
                    if (nd.ChildNodes.Count == 1 && nd.FirstChild.NodeType == XmlNodeType.Text)
                        element.SetProperty(name, GetPropertyTypeByName(element, name).TryConvertValue(nd.InnerText));
                    else
                        SetManyOrmProperty(element, name, nd.InnerXml);
                }
                elements.Add(element);
            }
            subEntity.SetProperty(names[names.Length-1], elements.ToArray(elementType));
        }

        /// <summary>
        /// 得到属性类型
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual Type GetPropertyTypeByName(object entity, string name)
        {
            var names = name.Split('.');
            Type property = entity.GetType().GetProperties().First(it=>it.Name.Equals(names[0])).PropertyType;
            for (int i = 1; i < names.Length; i++)
            {
                property=property.GetProperties().First(it=>it.Name.Equals(names[i])).PropertyType;
            }
            return property;
        }

        protected virtual object GetDataReaderValue(Type type, object value)
        {
            if(value==DBNull.Value)
               return type.IsValueType ? Activator.CreateInstance(type) : null;
            return value;
        }

        
        #endregion

        #region 关联属性和集合属性操作方法

        /// <summary>
        /// 得到属性集合里的属性类型
        /// </summary>
        /// <returns></returns>
        protected virtual Type GetPropertyElementType(Type type)
        {
            if (type == null) return null;
            Type ty;
            if (type.IsGenericType)
                ty = type.GetGenericArguments()[0];
            else if (type.GetElementType() != null)
                ty = type.GetElementType();
            else
                ty = typeof(object);
            return ty;
        }

        #endregion


     
    }
}
