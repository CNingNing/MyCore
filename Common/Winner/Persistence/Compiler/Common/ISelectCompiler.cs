namespace Winner.Persistence.Compiler.Common
{
    public interface ISelectCompiler
    {
        /// <summary>
        /// 解析查询
        /// </summary>
        /// <param name="queryCompiler"></param>
        /// <returns></returns>
        void Translate(QueryCompilerInfo queryCompiler);
    }
}
