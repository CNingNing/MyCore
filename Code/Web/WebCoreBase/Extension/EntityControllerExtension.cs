//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.AspNetCore.Mvc;
//using Beeant.Application.Services;
//using Beeant.Domain.Entities;
//using Dependent;
//using WebCore.Bases;
//using Component.Extension;
//using Winner.Persistence;
//using Winner.Persistence.Compiler.Common;
//using Winner.Persistence.Linq;
//using Winner.Persistence.Relation;

//namespace WebCore.Extension
//{
//    static public class EntityControllerExtension
//    {
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="name"></param>
//        /// <returns></returns>
//        public static IApplicationService ResolveApplication<T>(this Controller controller, string name = null)
//        {
//            return Ioc.Resolve<IApplicationService, T>(name);

//        }

//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="name"></param>
//        /// <returns></returns>
//        public static T ResolveIoc<T>(this Controller controller, string name=null)
//        {
//            return Ioc.Resolve<T>(name);

//        }
//        #region 非权限查询
//        public static IList<T> GetEntities<T>(this Controller controller, QueryInfo query) 
//        {
//            return Ioc.Resolve<IApplicationService, T>().GetEntities<T>(query);
//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        public static T GetEntity<T>(this Controller controller, long id) where T : BaseEntity
//        {
//            return Ioc.Resolve<IApplicationService, T>().GetEntity<T>(id);

//        }

//        public static IList<T> GetEntities<T>(this ControllerBase controller, QueryInfo query)
//        {
//            return Ioc.Resolve<IApplicationService, T>().GetEntities<T>(query);
//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        public static T GetEntity<T>(this ControllerBase controller, long id) where T : BaseEntity
//        {
//            return Ioc.Resolve<IApplicationService, T>().GetEntity<T>(id);

//        }

//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="info"></param>
//        /// <returns></returns>
//        public static bool SaveEntity<T>(this Controller controller, T info) where T : BaseEntity
//        {
//            return Ioc.Resolve<IApplicationService, T>().Save(info);

//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="info"></param>
//        /// <returns></returns>
//        public static IList<IUnitofwork> HandleEntity<T>(this Controller controller, T info) where T : BaseEntity
//        {
//            return Ioc.Resolve<IApplicationService, T>().Handle(new List<T> { info });

//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="info"></param>
//        /// <returns></returns>
//        public static IList<IUnitofwork> HandleEntities<T>(this Controller controller, IList<T> infos) where T : BaseEntity
//        {
//            return Ioc.Resolve<IApplicationService, T>().Handle(infos);

//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="info"></param>
//        /// <returns></returns>
//        public static bool CommitUnitofworks(this Controller controller, IList<IUnitofwork> unitofworks)
//        {
//            return Ioc.Resolve<IApplicationService>().Commit(unitofworks);

//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="info"></param>
//        /// <returns></returns>
//        public static bool InspectEntity<T>(this Controller controller, T info) where T : BaseEntity
//        {
//            return Ioc.Resolve<IApplicationService, T>().Inspect(info);

//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="infos"></param>
//        /// <returns></returns>
//        public static bool SaveEntities<T>(this Controller controller, IList<T> infos) where T : BaseEntity
//        {
//            return Ioc.Resolve<IApplicationService, T>().Save(infos);

//        }
//        #endregion

//        #region 权限查询
//        /// <summary>
//        /// 得到实体集合
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="query"></param>
//        /// <returns></returns>
//        public static IList<T> GetEntitiesByIdentity<T>(this BaseController controller, QueryInfo query) where T : BaseEntity
//        {
//            return GetEntitiesByIdentity<T>(controller, query, "Account.Id");
//        }

//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        public static T GetEntityByIdentity<T>(this BaseController controller, long id) where T : BaseEntity
//        {
//            var query = new QueryInfo();
//            query.Query<T>().Where(it=>it.Id==id);
//            var infos= GetEntitiesByIdentity<T>(controller,query);
//            if (infos == null) return null;
//            return infos.FirstOrDefault();
//        }
//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        public static T GetEntityByIdentity<T>(this BaseController controller) where T : BaseEntity
//        {
//            var query = new QueryInfo();
//            query.Query<T>();
//            var infos = GetEntitiesByIdentity<T>(controller, query);
//            if (infos == null) return null;
//            return infos.FirstOrDefault();
//        }
//        /// <summary>
//        /// 得到实体集合
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="query"></param>
//        /// <param name="propertyName"></param>
//        /// <returns></returns>
//        public static IList<T> GetEntitiesByIdentity<T>(this BaseController controller, QueryInfo query,string propertyName) where T : BaseEntity
//        {
//            var exp = string.Format("{0}==@AccountId", propertyName);
//            query.WhereExp = !string.IsNullOrEmpty(query.WhereExp)
//                                    ? string.Format("{0} && {1}", query.WhereExp, exp)
//                                    : exp;
//            query.SetParameter("AccountId",controller.Identity==null?0: controller.Identity.Id);
//            return Ioc.Resolve<IApplicationService, T>().GetEntities<T>(query);
//        }

//        /// <summary>
//        /// 得到实体
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="id"></param>
//        /// <param name="propertyName"></param>
//        /// <returns></returns>
//        public static T GetEntityByIdentity<T>(this BaseController controller, long id, string propertyName) where T : BaseEntity
//        {
//            var query = new QueryInfo();
//            query.Query<T>().Where(it => it.Id == id);
//            var infos = GetEntitiesByIdentity<T>(controller, query, propertyName);
//            if (infos == null) return null;
//            return infos.FirstOrDefault();
//        }

//        #endregion

//        #region 页面显示和保存
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="entity"></param>
//        /// <returns></returns>
//        public static IList<IDictionary<string, object>> GetForm<T>(this Controller controller, T entity) 
//        {
//            var result = new List<IDictionary<string, object>>();
//            GetForm(result, null, entity);
//            return result;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="result"></param>
//        /// <param name="propertyName"></param>
//        /// <param name="entity"></param>
//        /// <returns></returns>
//        public static void GetForm(IList<IDictionary<string, object>> result, string propertyName,object entity)
//        {
//            if (entity == null)
//                return;
          
//            var properties = entity.GetType().GetProperties();
//            foreach (var property in properties)
//            {
         
//                if(property.Name== "Properties" || property.Name == "WhereExp" || property.Name == "HandleResult" || property.Name == "Errors"
//                    || property.Name=="Mark" || property.Name == "SaveType" || property.Name == "DeleteTime" || property.Name == "Setting" || property.Name == "SettingDictionary"
//                   || property.Name == "Variables" || property.Name == "VariablesDictionary" || property.Name == "Version" || property.Name== "SaveSequence")
//                    continue;
//                if (property.PropertyType.GetInterface(typeof(IEnumerable).FullName)!=null && property.PropertyType!=typeof(string))
//                {
//                    var values = entity.GetProperty(property.Name) as IEnumerable;
//                    if (values == null)
//                        continue;
//                    var i = 0;
//                    foreach (var value in values)
//                    {
//                        if (value == null || value.GetType().IsValueType || value.GetType().ToString()== "System.String")
//                            continue;
//                        GetForm(result,$"{propertyName}{property.Name}[{i}].", value);
//                        i++;
//                    }
//                    continue;
//                }
//                var val = entity.GetProperty(property.Name);
//                if (val == null || val is DateTime && (val.Convert<DateTime>()==DateTime.MinValue.ToUniversalTime() || val.Convert<DateTime>() == DateTime.Now.GetDefault()))
//                    continue;
//                if (val is BaseEntity)
//                {
//                    GetForm(result, $"{propertyName}{property.Name}.", val);
//                }
//                else if (val is DateTime)
//                {
//                    result.Add(new Dictionary<string, object> { { "name", $"{propertyName}{property.Name}" }, { "value", val.Convert<DateTime>().ToString("yyyy-MM-dd") } });
//                }
//                else if (val.GetType().IsEnum)
//                {
//                    result.Add(new Dictionary<string, object> { { "name", $"{propertyName}{property.Name}" }, { "value", val.ToString() } });
//                }
//                else
//                {
//                    result.Add(new Dictionary<string, object> { { "name", $"{propertyName}{property.Name}" }, { "value", val } });
//                }
//           }
       
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="controller"></param>
//        /// <param name="entity"></param>
//        /// <returns></returns>
//        public static void FillForm<T>(this Controller controller, T entity) where T : BaseEntity
//        {
//            var paramterNames = controller.Request.AllKeys();
//            FillForm(entity, paramterNames, null,null);
       
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="entity"></param>
//        /// <param name="parent"></param>
//        /// <param name="paramterNames"></param>
//        /// <param name="requestName"></param>
//        /// <returns></returns>
//        public static void FillForm(BaseEntity entity, ICollection<string> paramterNames, BaseEntity parent, string requestName) 
//        {
//            var ormObject = Winner.Creator.Get<IOrm>().GetOrm(entity.GetType().FullName);
//            if(ormObject==null)
//                return;
//            entity.SaveType = entity.Id == 0 ? SaveType.Add : SaveType.Modify;
//            foreach (var property in ormObject.Properties)
//            {
//                if (property.PropertyName == "Id")
//                    continue;
             
//                if (property.Map!=null && property.Map.MapType== OrmMapType.OneToOne)
//                {
//                    var value = entity.GetProperty(property.PropertyName);
//                    if (value == null)
//                        continue;
//                    var item = value as BaseEntity;
//                    if (item == null)
//                        continue;
//                    var name = string.IsNullOrWhiteSpace(requestName) ? property.PropertyName : $"{requestName}{property.PropertyName}";
//                    if (paramterNames.Count(it => it != $"{name}.Id" && it.StartsWith($"{name}.")) > 0)
//                    {
//                        FillForm(item, paramterNames, null, $"{property.PropertyName}.");
//                    }
//                }
//                else if (property.Map != null && property.Map.MapType == OrmMapType.OneToMany)
//                {
//                    var value = entity.GetProperty(property.PropertyName);
//                    if (value == null)
//                        continue;
//                    var values = value as IEnumerable;
//                    if (values == null)
//                        continue;
//                    var i = 0;
//                    foreach (var val in values)
//                    {
//                        if (val == null)
//                            continue;
//                        var item = val as BaseEntity;
//                        if (item == null)
//                            continue;
//                        FillForm(item, paramterNames, entity, $"{property.PropertyName}[{i}].");
//                        i++;
//                    }
//                }
//                else 
//                {
//                    if (entity.SaveType == SaveType.Add && parent != null && parent.GetType() == property.PropertyType)
//                    {
//                        entity.SetProperty(property.PropertyName, parent);
//                    }
//                    if (entity.SaveType == SaveType.Modify)
//                    {
//                        var name = string.IsNullOrWhiteSpace(requestName) ? property.PropertyName : $"{requestName}{property.PropertyName}";
//                        if (paramterNames.Contains(name) && property.PropertyName != "Id" && ormObject.GetPropertyInfo(property.PropertyName) != null)
//                            entity.SetProperty(property.PropertyName);
//                    }
//                }
//            }
//            if (entity.SaveType == SaveType.Modify && entity.Properties == null)
//                entity.SaveType = SaveType.None;
//            //var properties = entity.GetType().GetProperties();
//            //foreach (var property in properties)
//            //{
                
//            //    if (entity.SaveType == SaveType.Add && parent != null && parent.GetType() == property.PropertyType)
//            //    {
//            //        entity.SetProperty(property.Name, parent);
//            //    }
//            //    if (entity.SaveType == SaveType.Modify)
//            //    {
//            //        var name = string.IsNullOrWhiteSpace(requestName) ? property.Name : $"{requestName}{property.Name}";
//            //       if (paramterNames.Contains(name) && property.Name!="Id" && ormObject.GetPropertyInfo(property.Name)!=null)
//            //            entity.SetProperty(property.Name);
//            //    }
//            //    if (property.PropertyType!=null)
//            //    {
//            //        var values = entity.GetProperty(property.Name) as IEnumerable;
//            //        if (values == null)
//            //            continue;
//            //        var i = 0;
//            //        foreach (var value in values)
//            //        {
//            //            if (value == null)
//            //                continue;
//            //            var item = value as BaseEntity;
//            //            if (item == null)
//            //                continue;
//            //            FillForm(item, paramterNames, entity,$"{property.Name}[{i}].");
//            //            i++;
//            //        }
//            //    }
//            //    else if(property.PropertyType.IsClass && property.PropertyType.FullName.StartsWith("Beeant.Domain.Entities"))
//            //    {
//            //        var value = entity.GetProperty(property.Name) as BaseEntity;
//            //        if (value == null)
//            //            continue;
//            //        var name = string.IsNullOrWhiteSpace(requestName) ? property.Name : $"{requestName}{property.Name}";
//            //        if(paramterNames.Count(it=>it!=$"{name}.Id" && it.StartsWith($"{name}."))>0)
//            //        {
//            //            FillForm(value, paramterNames, null, $"{property.Name}.");
//            //        }
//            //        if (entity.SaveType == SaveType.Modify)
//            //        {
//            //            var id = value.GetProperty("Id").Convert<long>();
//            //            if (id > 0)
//            //            {
//            //                entity.SetProperty($"{name}.Id");
//            //            }
//            //        }

//            //    }
            
//            //}
//            //if (entity.SaveType == SaveType.Modify && entity.Properties == null)
//            //    entity.SaveType = SaveType.None;
//        }

  
//        #endregion
//    }
//}
