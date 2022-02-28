namespace Winner.Persistence.Compiler.Common
{
    public interface IHavingCompiler
    {
        /// <summary>
        /// 解析Having
        /// </summary>
        /// <param name="whereCompiler"></param>
        /// <returns></returns>
        void Translate(WhereCompilerInfo whereCompiler);
    }
}
