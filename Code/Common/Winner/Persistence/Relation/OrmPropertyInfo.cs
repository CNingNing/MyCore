using System;
using System.Collections.Generic;
using System.Linq;

namespace Winner.Persistence.Relation
{
    [Serializable]
    public class OrmPropertyInfo
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }



        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 是否允许读取
        /// </summary>
        public bool AllowRead { get; set; }

        /// <summary>
        /// 是否允许录入
        /// </summary>
        public bool AllowAdd { get; set; }

        /// <summary>
        /// 是否允许更新
        /// </summary>
        public bool AllowModify { get; set; }

        /// <summary>
        /// 是否允许删除更新
        /// </summary>
        public bool AllowRemove { get; set; }

        /// <summary>
        /// 是否允许还原更新
        /// </summary>
        public bool AllowRestore { get; set; }

        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 是否自动生成主键
        /// </summary>
        public bool IsIdentityKey { get; set; }

        /// <summary>
        /// 是否为自定义字段
        /// </summary>
        public bool IsCustom { get; set; }

        /// <summary>
        /// 是否乐观锁
        /// </summary>
        public bool IsOptimisticLocker { get; set; }

        /// <summary>
        /// 是否资源文件
        /// </summary>
        public bool IsFileName { get; set; }

        /// <summary>
        /// 是否有设置
        /// </summary>
        public bool HasUnAddValue { get; set; }

        /// <summary>
        /// 添加为该值的时候，不操作
        /// </summary>
        public object UnAddValue { get; set; }

        /// <summary>
        /// 是否有设置
        /// </summary>
        public bool HasUnModifyValue { get; set; }

        /// <summary>
        /// 添加为该值的时候，不操作
        /// </summary>
        public object UnModifyValue { get; set; }

        /// <summary>
        /// 是否有设置
        /// </summary>
        public bool HasReadNullValue { get; set; }

        /// <summary>
        /// 当读取为null的默认值
        /// </summary>
        public object ReadNullValue { get; set; }
        /// <summary>
        /// 搜索类型
        /// </summary>
        public int SearchType { get; set; }
        /// <summary>
        /// 搜索类型
        /// </summary>
        public int SearchTableCount { get; set; }
        /// <summary>
        /// 映射关系
        /// </summary>
        public OrmMapInfo Map { get; set; }


        /// <summary>
        /// 对象
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EncryptedKey { get; set; }
        /// <summary>
        /// 属性是否在properties里
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool InProperties(IList<string> properties)
        {
            return properties.Contains(PropertyName);
        }

        public IDictionary<string, OrmObjectInfo> Orms { get; set; }
        /// <summary>
        /// 得到ORM
        /// </summary>
        /// <returns></returns>
        public virtual OrmObjectInfo GetObject()
        {
            return Orms[ObjectName];
        }

    }


}
