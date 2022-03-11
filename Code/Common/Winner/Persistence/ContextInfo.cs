using System.Collections.Generic;
using System.Linq;
using Winner.Persistence.Translation;

namespace Winner.Persistence
{
    public class ContextInfo
    {
        public object Locker{ get; set; }=new object();
        IDictionary<object, object> _entities = new Dictionary<object, object>();
        /// <summary>
        /// 实体
        /// </summary>
        public IDictionary<object, object> Entities
        {
            get { return _entities; }
            set { _entities = value; }
        }
        IDictionary<object, SaveInfo> _storages = new Dictionary<object, SaveInfo>();
        /// <summary>
        /// 存储实体
        /// </summary>
        public IDictionary<object, SaveInfo> Storages
        {
            get { return _storages; }
            set { _storages = value; }
        }

        private IList<IUnitofwork>  _unitofworks=new List<IUnitofwork>();
        /// <summary>
        /// 事务
        /// </summary>
        public IList<IUnitofwork> Unitofworks
        {
            get { return _unitofworks; } 
            set { _unitofworks = value; }
        }
        /// <summary>
        /// 是否存在实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool HasEntity(object key)
        {
            return Entities.ContainsKey(key);
        }
        /// <summary>
        /// 是否存在实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool HasStorage(object entity)
        {
            return Storages.ContainsKey(entity);
        }
        /// <summary>
        /// 是否存在实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object GetEntity(object key)
        {
            if (Entities.ContainsKey(key))
                return Entities[key];
            return null;
        }

    
        /// <summary>
        /// 得到存储
        /// </summary>
        /// <returns></returns>
        public virtual IList<SaveInfo> GetSaves()
        {
            return Storages.Where(storage => storage.Value.Entity.SaveType != SaveType.None)
                .Select(storage => storage.Value).ToList();
        }
    }



 
    
}