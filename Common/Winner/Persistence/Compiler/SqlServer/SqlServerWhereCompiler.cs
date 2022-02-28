using System.Collections.Generic;
using System.Text.RegularExpressions;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.SqlServer
{
    public class SqlServerWhereCompiler : WhereCompiler
    {
        /// <summary>
        /// 自定义分词
        /// </summary>
        protected override bool AppendLike(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties, string firstName, string lastName)
        {
            whereCompiler.Builder.AppendFormat("({0} like {1} +'%' or {0} like '%'+{1} +'%' or {0} like '%'+{1})",
                firstName, lastName);
            return true;
        }
        /// <summary>
        /// 自定义分词
        /// </summary>
        protected override bool AppendFullText(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties, string firstName, string lastName)
        {
            whereCompiler.Builder.AppendFormat("CONTAINS({0},{1})",
                firstName, lastName);
            return true;
        }
 
        

    }
}
