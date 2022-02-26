//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.NetworkInformation;
//using System.Net.Sockets;
//using System.Reflection;
//using System.Threading.Tasks;
//using System.Xml;
//using Beeant.Application.Services;
//using Beeant.Application.Services.Authority;
//using Beeant.Application.Services.Sys;
//using Beeant.Domain.Entities.Api;
//using Beeant.Domain.Entities.Authority;
//using Beeant.Domain.Entities.Sys;
//using WebCore.FilterAttribute;
//using Configuration;
//using Dependent;
//using Winner.Persistence;
//using Winner.Persistence.Linq;

//namespace WebCore
//{

//    public class StartupHelper
//    {

//        public static string SubSystemUrl { get; set; }

//        #region 初始化
//        static public object Locker = new object();
//        public static bool IsLock;
//        public static string[] Initialize(string[] args)
//        {
//            if (IsLock)
//                return args;
//            lock (Locker)
//            {
//                if (IsLock)
//                    return args;
//                IsLock = true;
//                var appName = Assembly.GetEntryAssembly().FullName.Split(',')[0];
//                ConfigurationManager.Initialize(appName);
//                args= GetArgs(appName,args);
//                return ReplaceUrls(args);
//            }
//        }

//        protected static string[] ReplaceUrls(string[] args)
//        {
//            if (args == null || args.Length == 0)
//                return args;
//            var result = new List<string>();
//            foreach (var arg in args)
//            {
//                if (arg.Contains("http://lan"))
//                {
//                    var ip = GetLanIp();
//                    var val = arg.Replace("http://lan", $"http://{ip}");
//                    result.Add(val);
//                }
//                else
//                {
//                    result.Add(arg);
//                }
//            }

//            return result.ToArray();
//        }
//        protected static string GetLanIp()
//        {
//            var hostIps = NetworkInterface
//                .GetAllNetworkInterfaces()
//                .Where(network => network.OperationalStatus == OperationalStatus.Up)
//                .Select(network => network.GetIPProperties())
//                .OrderByDescending(properties => properties.GatewayAddresses.Count)
//                .SelectMany(properties => properties.UnicastAddresses)
//                .Where(address => !IPAddress.IsLoopback(address.Address) && address.Address.AddressFamily == AddressFamily.InterNetwork)
//                .Select(it => it.Address.ToString())
//                .ToArray();
//            return hostIps?.FirstOrDefault();
//        }
//        /// <summary>
//        /// 加载配置
//        /// </summary>
//        /// <param name="appName"></param>
//        /// <param name="args"></param>
//        private static string[] GetArgs(string appName,string[] args)
//        {
//            if (args != null && args.Length > 0 || ConfigurationManager.GetSetting<bool>("IsDebug"))
//                return args;
//            var url = $"{appName.Replace("Beeant.", "").Replace(".", "")}Url";
//            var doc = new XmlDocument();
//            var fileName = Path.Combine(ConfigurationManager.ConfigRootPath, "Config/Url.config");
//            doc.Load(fileName);
//            XmlNodeList xmlNodes = doc.SelectNodes("configuration/Settings/App");
//            if (xmlNodes == null || xmlNodes.Count == 0)
//                return args;
//            foreach (XmlNode xmlNode in xmlNodes)
//            {
//                if (xmlNode.Attributes == null ||
//                    xmlNode.Attributes["Name"] != null && !string.IsNullOrEmpty(xmlNode.Attributes["Name"].Value) &&
//                    xmlNode.Attributes["Name"].Value != appName)
//                    continue;
//                foreach (XmlNode node in xmlNode.ChildNodes)
//                {
//                    if (node.Attributes == null || node.Attributes["key"] == null || node.Attributes["value"] == null)
//                        continue;
//                    if(node.Attributes["key"].Value== url && node.Attributes["cmd"]!=null && !string.IsNullOrWhiteSpace(node.Attributes["cmd"].Value))
//                    {
//                        var result = node.Attributes["cmd"].Value.Split(' ').Select(it => it.Trim()).ToArray();
//                        return result;
//                    }
//                }
//            }
//            return args;
//        }
    

//        #endregion

//        #region 获取API服务
//        /// <summary>
//        /// 加载语言包
//        /// </summary>
//        public static void RegisterApiService(string name)
//        {
//            var task = new Task(() =>
//            {
//                lock (Locker)
//                {
//                    var dataProtocols = GetProtocols();
//                    IList<ProtocolEntity> entities = new List<ProtocolEntity>();
//                    var types = Assembly.Load(name).GetTypes();
//                    if (types != null)
//                    {
//                        foreach (var type in types)
//                        {
//                            if (!type.FullName.EndsWith("Controller"))
//                                continue;
//                            var methods = type.GetMethods();
//                            if (methods == null)
//                                continue;
//                            foreach (var method in methods)
//                            {
//                                if (!method.IsPublic || method.CustomAttributes == null || method.CustomAttributes.Count() == 0)
//                                    continue;
//                                var apiResgisterAttribute = method.GetCustomAttribute<ApiRegisterAttribute>();
//                                if (apiResgisterAttribute == null)
//                                    continue;
//                                if (dataProtocols != null && dataProtocols.ContainsKey(apiResgisterAttribute.Name))
//                                    continue;
//                                if (entities.Count(it => it.Name == apiResgisterAttribute.Name) > 0)
//                                    continue;
//                                AddApi(entities, apiResgisterAttribute);
//                            }
//                        }
//                    }
//                    if (entities.Count > 0)
//                        Ioc.Resolve<IApplicationService, ProtocolEntity>().Save(entities);
//                }
              
//            });
//            task.Start();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="entities"></param>
//        /// <param name="apiResgisterAttribute"></param>
//        private static void AddApi(IList<ProtocolEntity> entities,  ApiRegisterAttribute apiResgisterAttribute)
//        {
//            var entity = entities.FirstOrDefault(it => it.Name == apiResgisterAttribute.Name);
//            if (entity == null)
//            {
//                entity = new ProtocolEntity
//                {
//                    Tag=apiResgisterAttribute.Tag,
//                    Name = apiResgisterAttribute.Name,
//                    Nickname = apiResgisterAttribute.Nickname,
//                    Detail = apiResgisterAttribute.Detail,
//                    Type = apiResgisterAttribute.Type,
//                    IsLog = apiResgisterAttribute.IsLog,
//                    SaveType = SaveType.Add
//                };
//                entities.Add(entity);
//            }
//            else if (entity.Nickname != apiResgisterAttribute.Nickname)
//            {
//                entity.SaveType = SaveType.Modify;
//                entity.SetProperty(it => it.Nickname);
//            }

//        }
//        /// <summary>
//        /// 得到协议
//        /// </summary>
//        /// <returns></returns>
//        private static IDictionary<string,ProtocolEntity> GetProtocols()
//        {
//            var query=new QueryInfo();
//            query.Query<ProtocolEntity>().Select(it => it.Name);
//            var infos = Ioc.Resolve<IApplicationService, ProtocolEntity>().GetEntities<ProtocolEntity>(query);
//            return infos == null ? null : infos.ToDictionary(it => it.Name, s => s);
//        }
//        #endregion

//        #region 获取Authority资源

//        public static void RegisterAuthorityResource(string systemName, string name, bool isRemove = false)
//        {
//            RegisterAuthorityResource(systemName, name, name, isRemove);
//        }

//        /// <summary>
//        /// 加载语言包
//        /// </summary>
//        public static void RegisterAuthorityResource(string systemName, string name, string dllName,
//            bool isRemove = false)
//        {
//            RegisterSubSystemUrl(name);
//            var task = new Task(() =>
//            {
//                lock (Locker)
//                {
//                    var subSystem = GetOrSaveSubsystem(systemName, name);
//                    if (subSystem == null)
//                        return;
//                    IList<MenuEntity> menus = new List<MenuEntity>(GetMenus(subSystem.Id));
//                    var types = Assembly.Load(dllName).GetTypes();
//                    if (types != null)
//                    {
//                        foreach (var type in types)
//                        {
//                            if (!type.FullName.EndsWith("Controller"))
//                                continue;
//                            if (!type.IsPublic || type.CustomAttributes == null || type.CustomAttributes.Count() == 0)
//                                continue;
//                            var authorityResources = type.GetCustomAttributes<AuthorityRegisterAttribute>();
//                            if (authorityResources == null)
//                                continue;
//                            foreach (var authorityRegisterAttribute in authorityResources)
//                            {
//                                if (!string.IsNullOrWhiteSpace(authorityRegisterAttribute.SubstemUrl) &&
//                                    authorityRegisterAttribute.SubstemUrl != SubSystemUrl)
//                                    continue;
//                                AddResource(subSystem, menus, authorityRegisterAttribute);
//                            }
//                        }
//                    }

//                    if (isRemove)
//                    {
//                        foreach (var menu in menus)
//                        {
//                            if (menu.Version == 0 && menu.SaveType!= SaveType.None)
//                                menu.SaveType = SaveType.Remove;
//                            menu.Version = 0;
//                            if (menu.Abilities == null)
//                                continue;
//                            foreach (var ability in menu.Abilities)
//                            {
//                                if (ability.Version == 0)
//                                    ability.SaveType = SaveType.Remove;
//                                ability.Version = 0;
//                                if (ability.Resources == null)
//                                    continue;
//                                foreach (var resource in ability.Resources)
//                                {
//                                    if (resource.Version == 0)
//                                        resource.SaveType = SaveType.Remove;
//                                    resource.Version = 0;
//                                    if (resource.SaveType != SaveType.None && resource.SaveType != SaveType.Modify)
//                                    {
//                                        Ioc.Resolve<IEventEngineApplicationService>()
//                                            .Trigger("ClearAuthorityResourceCache", new Dictionary<string, string>
//                                            {
//                                                {"url", resource.Url}
//                                            });
//                                    }
//                                }
//                            }
//                        }
//                    }
//                    var rev = Ioc.Resolve<IApplicationService, MenuEntity>().Save(menus);
//                }
//            });
//            task.Start();
//        }

//        /// <summary>
//        /// 加载语言包
//        /// </summary>
//        public static void RegisterSubSystemUrl(string name)
//        {
//            if (!string.IsNullOrWhiteSpace(SubSystemUrl))
//                return;
//            SubSystemUrl = $"{name.Replace("Beeant.", "").Replace(".", "")}Url";
         
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="subSystem"></param>
//        /// <param name="menus"></param>
//        /// <param name="authorityRegisterAttribute"></param>
//        private static void AddResource(SubsystemEntity subSystem,IList<MenuEntity> menus, AuthorityRegisterAttribute authorityRegisterAttribute)
//        {
          
//            var menu = menus.FirstOrDefault(it => it.Url?.ToLower() == authorityRegisterAttribute.MenuUrl?.ToLower()?.Trim());
//            if (menu == null)
//            {
//                menu = new MenuEntity { Subsystem = subSystem,Parent=new MenuEntity(), Name = authorityRegisterAttribute.MenuName?.Trim(),IsShow=true, Url = authorityRegisterAttribute.MenuUrl?.Trim(), Sequence = authorityRegisterAttribute.MenuSequence, SaveType = SaveType.Add };
//                menus.Add(menu);
//            }
//            else if (menu.SaveType!=SaveType.Add && (menu.Name != authorityRegisterAttribute.MenuName || menu.Sequence!= authorityRegisterAttribute.MenuSequence))
//            {
//                menu.SaveType = SaveType.Modify;
//                if(authorityRegisterAttribute.MenuSequence>0)
//                {
//                    menu.Sequence = authorityRegisterAttribute.MenuSequence;
//                    menu.SetProperty(it => it.Sequence);
//                }
//                menu.Name = authorityRegisterAttribute.MenuName?.Trim();
//                menu.SetProperty(it => it.Name);
//            }
//            if (string.IsNullOrWhiteSpace(authorityRegisterAttribute.ResourceUrl))
//                return;
//            menu.Version = 1;
//            menu.Abilities = menu.Abilities ?? new List<AbilityEntity>();
//            authorityRegisterAttribute.AbilityName = string.IsNullOrWhiteSpace(authorityRegisterAttribute.AbilityName)? authorityRegisterAttribute.MenuName: authorityRegisterAttribute.AbilityName;
//            var ability = menu.Abilities?.FirstOrDefault(s => s.Name == authorityRegisterAttribute.AbilityName?.Trim() || (!string.IsNullOrWhiteSpace(s.Remark) && s.Remark== authorityRegisterAttribute.AbilityMark));
//            if (ability == null)
//            {
//                ability = new AbilityEntity { Menu = menu, Name = authorityRegisterAttribute.AbilityName?.Trim(), IsVerify=true, Remark = authorityRegisterAttribute.AbilityMark, SaveType = SaveType.Add };
//                menu.Abilities.Add(ability);
//            }
//            else if (ability.Name != authorityRegisterAttribute.AbilityName?.Trim() && ability.SaveType==SaveType.None)
//            {
//                ability.SaveType = SaveType.Modify;
//                ability.Name = authorityRegisterAttribute.AbilityName;
//                ability.SetProperty(it => it.Name);
//            }
//            ability.Version = 1;
//            authorityRegisterAttribute.ResourceName = string.IsNullOrWhiteSpace(authorityRegisterAttribute.ResourceName) ? authorityRegisterAttribute.AbilityName?.Trim() : authorityRegisterAttribute.ResourceName?.Trim();
//            ability.Resources = ability.Resources ?? new List<ResourceEntity>();
//            var resource = ability.Resources?.FirstOrDefault(s => s.Url?.ToLower() == authorityRegisterAttribute.ResourceUrl?.ToLower()?.Trim() && (s.Controls??"")== (authorityRegisterAttribute.ResourceControls??""));
//            if (resource == null)
//            {
//                resource = new ResourceEntity { Ability = ability,Url=authorityRegisterAttribute.ResourceUrl?.Trim(),
//                    Controls= authorityRegisterAttribute.ResourceControls,
//                    Name = authorityRegisterAttribute.ResourceName?.Trim(),
//                    Remark = "",IsLog=authorityRegisterAttribute.IsLog, SaveType = SaveType.Add };
//                ability.Resources.Add(resource);
//            }
//            else if (resource.Name != authorityRegisterAttribute.ResourceName?.Trim())
//            {
//                resource.SaveType = SaveType.Modify;
//                resource.Name = authorityRegisterAttribute.AbilityName?.Trim();
//                resource.SetProperty(it => it.Name);
//            }
//            resource.Version = 1;
//        }

//        /// <summary>
//        /// 得到系统
//        /// </summary>
//        /// <param name="systemName"></param>
//        /// <param name="name"></param>
//        /// <returns></returns>
//        private static SubsystemEntity GetOrSaveSubsystem(string systemName,string name)
//        {
//            var url = $"{name.Replace("Beeant.", "").Replace(".", "")}Url";
//            var query=new QueryInfo();
//            query.Query<SubsystemEntity>().Where(it => it.Url == url);
//            var info = Ioc.Resolve<IApplicationService, SubsystemEntity>().GetEntities<SubsystemEntity>(query)
//                ?.FirstOrDefault();
//            if (info == null)
//            {
//                info=new SubsystemEntity();
//                info.Url = url;
//                info.Name = systemName;
//                info.SaveType = SaveType.Add;
//                Ioc.Resolve<IApplicationService, SubsystemEntity>().Save(info);
//            }
//            else
//            {
//                info.SetProperty(it => it.Name);
//                info.Name = systemName;
//                info.SaveType = SaveType.Modify;
//                Ioc.Resolve<IApplicationService, SubsystemEntity>().Save(info);
//            }
//            return info;
//        }

//        /// <summary>
//        /// 得到协议
//        /// </summary>
//        /// <returns></returns>
//        private static IList<MenuEntity> GetMenus(long subsystemId)
//        {
//            var query = new QueryInfo();
//            query.Query<MenuEntity>().Where(it => it.Subsystem.Id== subsystemId)
//                .Select(it=>new object[]{it,it.Abilities.Select(s=>new object[]{s.Id,s.Name,s.Remark,s.Resources.Select(n=>new object[]{n.Id,n.Name,n.Url,n.Controls})})});
//            var infos = Ioc.Resolve<IApplicationService, MenuEntity>().GetEntities<MenuEntity>(query);
//            return infos;
//        }
//        #endregion

//        #region 注册event
//        /// <summary>
//        /// 加载语言包
//        /// </summary>
//        public static void RegisterEvent(string name, bool isRemove=true)
//        {
//            var url = $"{name.Replace("Beeant.", "").Replace(".", "")}Url";
//            var task = new Task(() =>
//            {
//                var query = new QueryInfo();
//                query.Query<EventEntity>().Where(it => it.Url.StartsWith(url)).Select(it => it);
//                var datas = Ioc.Resolve<IApplicationService>().GetEntities<EventEntity>(query);
//                IList<EventEntity> infos = new List<EventEntity>();
//                var types = Assembly.Load(name).GetTypes();
//                if (types != null)
//                {
//                    foreach (var type in types)
//                    {
//                        if (!type.FullName.EndsWith("Controller"))
//                            continue;
//                        if (!type.IsPublic || type.CustomAttributes == null || type.CustomAttributes.Count() == 0)
//                            continue;
//                        var eventRegisters= type.GetCustomAttributes<EventRegisterAttribute>();
//                        if (eventRegisters == null)
//                            continue;
//                        foreach (var eventRegister in eventRegisters)
//                        {
//                            var data = datas?.FirstOrDefault(it => it.Name == eventRegister.Name);
//                            if (data == null)
//                            {
//                                infos.Add(new EventEntity { Name = name, Url =eventRegister.Url, SaveType = SaveType.Add });
//                            }
//                            else if (data.Url != eventRegister.Url)
//                            {
//                                data.SaveType = SaveType.Modify;
//                                data.SetProperty(it => it.Url);
//                                infos.Add(data);
//                            }
//                            else
//                            {
//                                infos.Add(new EventEntity { Name = name, Url = eventRegister.Url});
//                            }
//                        }

//                    }
//                }
//                if (isRemove && datas!=null)
//                {
//                    foreach (var data in datas)
//                    {
//                        var info = infos?.FirstOrDefault(it => it.Name == data.Name);
//                        if (info == null)
//                        {
//                            data.SaveType = SaveType.Remove;
//                            infos.Add(data);
//                        }
//                    }
//                }
//                if (infos.Count > 0)
//                    Ioc.Resolve<IApplicationService>().Save(infos);
//            });
//            task.Start();
//            RegisterSubSystemUrl(name);
//        }
//        #endregion

//    }

//}
