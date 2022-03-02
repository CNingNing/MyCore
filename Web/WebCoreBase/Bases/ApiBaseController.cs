using System;
using System.Threading.Tasks;

using Winner;
using Winner.Filter;
using Winner.Persistence;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebCore.Base
{

    public class ApiBaseController : ControllerBase
    {

        //private IdentityEntity _identity;
        ///// <summary>
        ///// 身份验证
        ///// </summary>
        //public virtual IdentityEntity Identity
        //{
        //    get
        //    {
        //        if (_identity == null && ViewBag.Identity != null)
        //            _identity = ViewBag.Identity;
        //        if (_identity==null)
        //            _identity= HttpContextHelper.Current().GetIdentity();
        //        return _identity;
        //    }

        //}
        /// <summary>
        /// 身份验证
        /// </summary>
        //public virtual ApiRequestEntity ApiRequest
        //{
        //    get
        //    {
        //        if (ViewBag.ApiRequest != null)
        //            return ViewBag.ApiRequest as ApiRequestEntity;
        //        return null;
        //    }
        //}

        /// <summary>
        /// 凭据验证数据
        /// </summary>
        //protected virtual VerificationEntity Verification
        //{
        //    get
        //    {
        //       return ViewBag.Verification;
        //    }
        //}

        /// <summary>
        /// 返回错误结构
        /// </summary>
        /// <param name="result"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        //protected virtual IActionResult ReturnResult(ApiResponseEntity result, JsonSerializerSettings settings = null)
        //{
        //    //if (!string.IsNullOrWhiteSpace(ApiRequest?.Sign))
        //    //{
        //    //    result.Language = ApiRequest.Language;
        //    //    result.Token = ApiRequest?.Token;
        //    //    result.TraceId = ApiRequest?.TraceId;
        //    //    result.Sign = result.GetSign();
        //    //}
        //    return this.Jsonp(result, settings);
        //}
        /// <summary>
        /// 返回结构
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected virtual IActionResult ReturnResult(string code, string message, object data = null, JsonSerializerSettings settings = null)
        {
            return ReturnResult(data != null, code, message, data, settings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected virtual IActionResult ReturnResult(bool status, string code, string message, object data = null, JsonSerializerSettings settings = null)
        {
           // var result = new ApiResponseEntity { Status = status, Code = code, Message = message, Data = data };
            return ReturnResult("","",null, settings);
        }

        /// <summary>
        /// 返回错误结构
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual IActionResult ReturnSuccessResult(string message, object data)
        {
            return ReturnResult("Success", message, data ?? "");
        }

        /// <summary>
        /// 返回错误结构
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual IActionResult ReturnFailureResult(string code, string message)
        {
            return ReturnResult(code, message);
        }
        /// <summary>
        /// 返回错误结构
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        //protected virtual IActionResult ReturnExceptionResult(Exception ex)
        //{
        //    var error = Creator.Get<IValidation>().GetErrorInfo(typeof(VoucherProtocolEntity).FullName, "99999");
        //    Task task = new Task(() => { AddErrorEntity(ex); });
        //    task.Start();
        //    return ReturnResult("Failure", error == null ? "Failure" : error.Message);
        //}
        /// <summary>
        /// 添加错误信息
        /// </summary>
        //protected virtual void AddErrorEntity(Exception ex)
        //{
        //    var info = new ErrorEntity
        //    {
        //        Address = Request.Url(),
        //        Device = Request.UserAgent(),
        //        Ip = HttpContextHelper.Current().Request.GetClientIp(),
        //        SaveType = SaveType.Add
        //    };
        //    info.SetEntity(ex);
        //    Ioc.Resolve<IApplicationService, ErrorInfo>().Save(info); 
        //}
       
    }
}
