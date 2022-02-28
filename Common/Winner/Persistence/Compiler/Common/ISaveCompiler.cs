namespace Winner.Persistence.Compiler.Common
{
    public interface ISaveCompiler
    {
        /// <summary>
        /// 转换对象为sql语句
        /// </summary>
        /// <param name="saveCompile"></param>
        void Save(SaveCompilerInfo saveCompile);
    }
}
