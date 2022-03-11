using System.Collections.Generic;

namespace Winner.Persistence.Works
{
    public interface ITransaction
    {
     
        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="unitOfWorks"></param>
        /// <returns></returns>
        bool Commit(IList<IUnitofwork> unitOfWorks);

    }
}
