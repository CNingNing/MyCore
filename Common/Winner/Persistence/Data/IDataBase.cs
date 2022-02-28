using Winner.Persistence.Compiler.Common;

namespace Winner.Persistence.Data
{
    public interface IDataBase
    {
       
        /// <summary>
        /// 得到编译器
        /// </summary>
        /// <param name="dataBase"></param>
        /// <returns></returns>
        ICompiler GetCompiler(OrmDataBaseInfo dataBase);
        /// <summary>
        /// 得到所有数据库
        /// </summary>
        /// <returns></returns>
        void AddDataBase(OrmDataBaseInfo dataBase);

        /// <summary>
        /// 得到数据库
        /// </summary>
        /// <returns></returns>
        OrmDataBaseInfo GetDataBase(string name);

    }
}
