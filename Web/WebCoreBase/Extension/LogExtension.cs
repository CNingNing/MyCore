//using Microsoft.AspNetCore.Mvc;
//using Dependent;
//using Winner.Persistence;

//namespace WebCore.Extension
//{
//    static public class LogExtension
//    {
     
//        #region 登入日志扩展方法

//        /// <summary>
//        /// 添加登入日志
//        /// </summary>
//        /// <param name="controller"></param>
//        /// <param name="identity"></param>
//        /// <param name="type"></param>
//        /// <param name="ip"></param>
//        /// <param name="city"></param>
//        /// <param name="address"></param>
//        /// <param name="device"></param>
//        /// <param name="message"></param>
//        public static void AddLoginLog(this Controller controller, IdentityEntity identity,string type, string ip, string city, string address,string device,string message)
//        {
//            if (identity == null)
//                return;
//            var info = new LoginEntity
//                {
//                    Type=string.IsNullOrEmpty(type)?"Account":type,
//                    Ip = ip,
//                    Address = address,
//                    City=city,
//                    Device = device,
//                    Account = new AccountEntity { Id = identity.Id },
//                    Message= message,
//                    SaveType = SaveType.Add
//                };
//            Ioc.Resolve<IApplicationService,LoginEntity>().Save(info);
//        }

//        /// <summary>
//        /// 添加登入日志
//        /// </summary>
//        /// <param name="controller"></param>
//        /// <param name="identity"></param>
//        /// <param name="type"></param>
//        /// <param name="message"></param>
//        public static void AddLoginLog(this Controller controller, IdentityEntity identity, string type, string message)
//        {
//            if(identity==null)
//                return;
//            var info = new LoginEntity
//            {
//                Type = string.IsNullOrEmpty(type) ? "Account" : type,
//                Ip = HttpContextHelper.Current().Request.GetClientIp(),
//                Address = controller.Request.Url(),
//                Device = controller.Request.UserAgent(),
//                Account = new AccountEntity { Id = identity.Id },
//                Message = message,
//                SaveType = SaveType.Add
//            };
//            Ioc.Resolve<IApplicationService, LoginEntity>().Save(info);
//        }
//        #endregion

    
//    }

 
//}
