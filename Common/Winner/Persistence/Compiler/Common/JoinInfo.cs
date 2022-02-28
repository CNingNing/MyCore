using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Common
{
    public class JoinInfo
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string AsName { get; set; }
        /// <summary>
        /// 连接字段名
        /// </summary>
        public string AsFieldName { get; set; }
        /// <summary>
        /// 查询对象
        /// </summary>
        public OrmObjectInfo Object { get; set; }
        /// <summary>
        /// 映射对象
        /// </summary>
        public OrmMapInfo Map { get; set; }
        /// <summary>
        /// 连接名称
        /// </summary>
        public string JoinName { get; set; }
        /// <summary>
        /// 连接字段名
        /// </summary>
        public string JoinFieldName { get; set; }
    }
}
