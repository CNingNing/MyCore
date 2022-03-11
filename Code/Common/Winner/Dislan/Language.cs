using System.Collections.Generic;
using System.Linq;

namespace Winner.Dislan
{
    public class Language : ILanguage
    {
        #region 属性

        private IDictionary<string, IDictionary<string,LanguageInfo>> _langs = new Dictionary<string, IDictionary<string,LanguageInfo>>();
        /// <summary>
        /// 名称集合
        /// </summary>
        protected IDictionary<string, IDictionary<string, LanguageInfo>> Langs 
        { 
            get { return _langs; }
            set { _langs = value; }
        }

        #endregion
        #region 接口的实现

        /// <summary>
        /// 得到名称
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual string GetLang(string key, string name)
        {
            if (Langs.ContainsKey(key) && Langs[key]!=null && Langs[key].ContainsKey(name) && Langs[key][name]!=null)
            {
                return Langs[key][name].Message;
            }
            return null;
        }
        /// <summary>
        /// 添加名称
        /// </summary>
        /// <param name="key"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        public virtual bool AddLangs(string key, IList<LanguageInfo> infos)
        {
            if (!Langs.ContainsKey(key))
                Langs.Add(key, new Dictionary<string, LanguageInfo>());
            var dis = Langs[key];
            foreach (var info in infos)
            {
                if(!dis.ContainsKey(info.Name))
                    dis.Add(info.Name,info);
            }
            return true;
        }

      
     
        /// <summary>
        /// 移除名称
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool RemoveLang(string key)
        {
            if (!Langs.ContainsKey(key))
                return false;
            Langs.Remove(key);
            return true;
        }

        public virtual IList<LanguageInfo> GetLangs(string key)
        {
            if (!Langs.ContainsKey(key))
                return null;
            return Langs[key].Values.ToList();
        }

 

        #endregion 
    }
}
