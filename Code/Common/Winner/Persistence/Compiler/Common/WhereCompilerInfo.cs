using System.Text;
using System.Text.RegularExpressions;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Common
{
    public class WhereCompilerInfo:QueryCompilerInfo
    {
        /// <summary>
        /// 前一个匹配的内容
        /// </summary>
        public Match PreviousMatch { get; set; }
        /// <summary>
        /// 是否保存
        /// </summary>
        public bool IsSaveWhere { get; set; }
    
        /// <summary>
        /// 存储
        /// </summary>
        public SaveCompilerInfo SaveCompiler { get; set; }
        

        public WhereCompilerInfo(OrmObjectInfo obj, string exp, TableInfo table, StringBuilder builder, string feildBeforeTag, string feildAfterTag, bool isSaveWhere = false, SaveCompilerInfo saveCompiler=null)
            : base(obj, exp, table, builder, feildBeforeTag, feildAfterTag)
        {
            IsSaveWhere = isSaveWhere;
            SaveCompiler = saveCompiler;
        }

    
    }
}
