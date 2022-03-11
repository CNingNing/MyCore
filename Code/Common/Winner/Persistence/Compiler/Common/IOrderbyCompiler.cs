namespace Winner.Persistence.Compiler.Common
{
    public interface IOrderbyCompiler
    {
        /// <summary>
        /// 解析orderby
        /// </summary>
        /// <param name="queryCompiler"></param>
        void Translate(QueryCompilerInfo queryCompiler);
    }
}
