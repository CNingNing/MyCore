using System;
using System.Net;


namespace Component.SDK
{


	public class QqSdk
    {

        private string _appid;
        private string _key;
        public QqSdk(string token,string key)
        {
            _appid = token;
            _key = key;
        }
        public virtual string QqAuthorize(string redirecturl)
        {
             return string.Format(
                     "https://graph.qq.com/oauth2.0/authorize?response_type=code&client_id={0}&redirect_uri={1}&state=State",
                     _appid, WebUtility.UrlEncode(redirecturl));
        }
    }
}

