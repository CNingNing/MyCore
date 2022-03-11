using System.Collections.Generic;
using Winner.Persistence.Relation;

namespace Winner.Persistence.Compiler.Analysis
{

    public interface IAnalyzer
    {
        /// <summary>
        /// 分词信息
        /// </summary>
        /// <param name="property"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        IList<WordInfo> Resolve(OrmPropertyInfo property, string input);
        /// <summary>
        /// 分词信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IList<WordInfo> Resolve(string input);

    }
}
