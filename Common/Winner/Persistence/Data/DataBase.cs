using System.Collections.Generic;
using System.Linq;
using Winner.Persistence.Compiler.Common;

namespace Winner.Persistence.Data
{
    public class DataBase : IDataBase
    {
        #region 属性
   
        private IDictionary<string, OrmDataBaseInfo> _dataBases = new Dictionary<string, OrmDataBaseInfo>();
        public IDictionary<string, OrmDataBaseInfo> DataBases
        {
            get { return _dataBases; }
            set { _dataBases = value; }
        }
        #endregion

        #region 接口的实现
  

        /// <summary>
        /// 得到编译器
        /// </summary>
        /// <param name="dataBase"></param>
        /// <returns></returns>
        public virtual ICompiler GetCompiler(OrmDataBaseInfo dataBase)
        {
            if (string.IsNullOrEmpty(dataBase.CompilerName))
                return Creator.Get<ICompiler>();
            return Creator.Get<ICompiler>(dataBase.CompilerName);
        }


        /// <summary>
        /// 添加数据库
        /// </summary>
        /// <param name="dataBase"></param>
        public virtual void AddDataBase(OrmDataBaseInfo dataBase)
        {
           DataBases.Add(dataBase.Name,dataBase);
        }

   
        /// <summary>
        /// 根据名称得到数据库
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual OrmDataBaseInfo GetDataBase(string name)
        {
            if (string.IsNullOrEmpty(name)) return DataBases.Values.FirstOrDefault(it => it.IsDefault);
            return DataBases.ContainsKey(name) ? DataBases[name] : null; 
        }

  

        #endregion
    }
}
