namespace Winner.Persistence.Compiler.Common
{
    public interface IWhereCompiler
    {
        /// <summary>
        /// 解析条件
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <returns></returns>
        void Translate(WhereCompilerInfo whereCompiler);

    }
}
