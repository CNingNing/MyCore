namespace Winner.Persistence.Compiler.Common
{
    public interface IGroupbyCompiler
    {
        /// <summary>
        /// 解析Groupby
        /// </summary>
        /// <param name="queryCompiler"></param>
        void Translate(QueryCompilerInfo queryCompiler);
    }
}
