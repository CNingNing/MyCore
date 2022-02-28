using System.Data.Common;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Common
{
    public interface IQueryCompiler
    {
        /// <summary>
        /// 解析查询
        /// </summary>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        void Translate(DbCommand command, OrmObjectInfo obj,QueryInfo query);

        /// <summary>
        /// 解析查询
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        void TranslateQuery(OrmObjectInfo obj, QueryInfo query);
        /// <summary>
        /// 解析查询
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="query"></param>
        /// <param name="queryCompiler"></param>
        void TranslateQuery(OrmObjectInfo obj, QueryInfo query, QueryCompilerInfo queryCompiler);
    }
}
