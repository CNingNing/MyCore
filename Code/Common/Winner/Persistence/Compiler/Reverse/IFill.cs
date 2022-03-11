using System.Data;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Reverse
{
    public interface IFill
    {
        /// <summary>
        /// 根据reader设置属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        T Reverse<T>(IDataReader reader, OrmObjectInfo obj);
        /// <summary>
        /// 根据reader设置属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        T Reverse<T>(DataTable table, OrmObjectInfo obj);
    }
}
