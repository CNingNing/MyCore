using Component.Extension;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Component.ThirdPartySdk
{

    public class QqSdk
    {

        private string _qqappid;
        private string _qqkey;
        private readonly IHttpClientFactory _clientFactory;
        public QqSdk(string token, string key,IHttpClientFactory httpClientFactory)
        {
            _qqappid = token;
            _qqkey = key;
            this._clientFactory = httpClientFactory;
        }
        public virtual string QqAuthorize(string redirecturl)
        {
            return string.Format(
                    "https://graph.qq.com/oauth2.0/authorize?response_type=code&client_id={0}&redirect_uri={1}&state=State",
                    _qqappid, WebUtility.UrlEncode(redirecturl));
        }

        public virtual async Task<string> GetToken(string code, string redirecturl)
        {
            var url = string.Format(
              "https://graph.qq.com/oauth2.0/token?client_id={0}&client_secret={1}&code={2}&grant_type=authorization_code&redirect_uri={3}",
              _qqappid, _qqkey, code, redirecturl);
            var client=_clientFactory.CreateClient();
            var response = await client.GetStringAsync(url);
            //var response =  WebRequestHelper.SendGetRequestAsync(url)?.Result;
            if (string.IsNullOrEmpty(response) || !response.Contains("access_token"))
            {
                return "";
            }
            var dis = response.Split('&').Where(it => it.Contains("access_token")).FirstOrDefault();
            var token = dis?.Split('=')[1] ?? "";
            return token;
        }

        public virtual async Task<string> GetAuthorityOpendIdAndUnionId(string token)
        {
            if (string.IsNullOrEmpty(token)) return "";
            var url = $"https://graph.qq.com/oauth2.0/me?access_token={token}&unionid=1";
            var client=_clientFactory.CreateClient(); 
            var response = await client.GetStringAsync(url);
            //var response =  WebRequestHelper.SendGetRequestAsync(url)?.Result;
            if (string.IsNullOrEmpty(response) || response.Contains("error") || !response.Contains("callback"))
                return "";
            Regex reg = new Regex(@"\(([^)]*)\)");
            Match m = reg.Match(response);
            return m.Result("$1");
        }


        public virtual async Task<string> GetUserInfo(string token, string openId)
        {
            var url = $"https://graph.qq.com/user/get_user_info?access_token={token}&openid={openId}&oauth_consumer_key={_qqappid}";
            var client = _clientFactory.CreateClient();
            var response = await client.GetStringAsync(url);
            //var response =  WebRequestHelper.SendGetRequestAsync(url)?.Result;
            if (string.IsNullOrWhiteSpace(response))
                return "";
            return response;
        }


    }
}

