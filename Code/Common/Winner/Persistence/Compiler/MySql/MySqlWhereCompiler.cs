using System.Collections.Generic;
using System.Text.RegularExpressions;
using Winner.Persistence.Compiler.Common;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.MySql
{
    public class MySqlWhereCompiler : WhereCompiler
    {        /// <summary>
        /// 
        /// </summary>
        public override string FeildBeforeTag { get; set; } = "`";
        /// <summary>
        /// 
        /// </summary>
        public override string FeildAfterTag { get; set; } = "`";
        /// <summary>
        /// 自定义分词
        /// </summary>
        protected override bool AppendLike(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties, string firstName, string lastName)
        {
            whereCompiler.Builder.AppendFormat("({0} like concat({1},'%') or {0} like concat('%',{1},'%') or {0} like concat('%',{1}))",
                firstName, lastName);
            return true;
        }
        /// <summary>
        /// 自定义分词
        /// </summary>
        protected override bool AppendFullText(WhereCompilerInfo whereCompiler, Match match, List<OrmPropertyInfo> chainProperties, string firstName, string lastName)
        {
            whereCompiler.Builder.AppendFormat("MATCH ({0}) AGAINST ({1} IN BOOLEAN MODE)", firstName, lastName);
            return true;
        }
 
        /// <summary>
        /// 匹配开始
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected override void ReplaceStartsWith(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            ReplaceStringMothed(whereCompiler, match, "{0} like concat({1},'%')");
        }

        /// <summary>
        /// 匹配结束
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <param name="match"></param>
        /// <param name="key"></param>
        protected override void ReplaceEndsWith(WhereCompilerInfo whereCompiler, Match match, string key)
        {
            ReplaceStringMothed(whereCompiler, match, "{0} like concat('%',{1})");

        }

    }
}
