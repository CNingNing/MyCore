using System.Collections.Generic;
using Winner.Persistence.Translation;

namespace Winner.Persistence.Route
{
    public interface IDbRoute
    {
        /// <summary>
        /// 得到名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        DbRouteInfo GetDbRoute(string name);
        /// <summary>
        /// 得到读路由
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IList<QueryInfo> GetRouteQueries(QueryInfo query);

        /// <summary>
        /// 得到写路由
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        void SetRouteSaveInfo(SaveInfo save);
    }
}
