using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace MultiCommentViewer.TexTra
{
    class OauthParams
    {
        public string Key { get; set; }
        //public string Secret { get; set; }
        public string Token { get; set; }
        public string ConsumerSecret { get; set; }
        //public string TokenSecret { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
    }
    public class Response
    {
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public string Translated { get; set; }
    }
    public class TexTraTranslator
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        private static readonly Random NonceRandom = new Random();
        public async Task<Response> Traslate(string text, string name, string key, string secret)
        {
            var url = "https://mt-auto-minhon-mlt.ucri.jgn-x.jp/api/mt/generalN_en_ja/";
            var data = new Dictionary<string, string>
            {
                {"text", text },
                {"name", name },
                {"key", key },
                {"type", "json" },
            };
            var oauthHeader = GetOauth1Header(new OauthParams
            {
                ConsumerSecret = secret,
                Key = key,
                Method = "POST",
                Url = url,
            }, data);


            var content = new FormUrlEncodedContent(data);

            HttpResponseMessage res;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", oauthHeader);
                res = await client.PostAsync(url, content);
            }
            var s = await res.Content.ReadAsStringAsync();
            return Parse(s);
        }
        public Response Parse(string res)
        {
            dynamic d = JsonConvert.DeserializeObject(res);
            var code = (int)d.resultset.code;
            var c = d.resultset.result.information["text-t"];
            if (code == 0)
            {
                return new Response
                {
                    IsError = false,
                    Translated = d.resultset.result.information["text-t"],
                };
            }
            else
            {
                return new Response
                {
                    IsError = true,
                    ErrorMessage = code.ToString(),
                };
            }
        }
        private string GetOauth1Header(OauthParams options, Dictionary<string, string> data)
        {
            var @params = GetOauth1Params(options, data);
            StringBuilder sb = new StringBuilder();
            foreach (var kv in @params)
            {
                var k = kv.Key;
                var v = kv.Value;
                sb.AppendFormat("{0}=\"{1}\",", k, UrlEncode(v));
            }
            return sb.ToString();
        }
        private Dictionary<string, string> GetOauth1Params(OauthParams options, Dictionary<string, string> data)
        {
            var key = options.Key;
            var token = options.Token;
            var secret = options.ConsumerSecret;
            var method = options.Method;
            var url = options.Url;

            var parameter = new Dictionary<string, string>();
            parameter.Add("oauth_consumer_key", key);
            parameter.Add("oauth_signature_method", "HMAC-SHA1");
            parameter.Add("oauth_timestamp", Convert.ToInt64((DateTime.UtcNow - UnixEpoch).TotalSeconds).ToString());
            //epoch秒
            parameter.Add("oauth_nonce", NonceRandom.Next(123400, 9999999).ToString());
            parameter.Add("oauth_version", "1.0");
            if (!string.IsNullOrEmpty(token))
                parameter.Add("oauth_token", token);
            foreach (var kv in data)
            {
                var k = kv.Key;
                var v = kv.Value;
                parameter.Add(k, v);
            }
            parameter.Add("oauth_signature", CreateSignature(secret, method, new Uri(url), parameter));
            return parameter;
        }
        private string CreateSignature(string consumerSecret, string method, Uri uri, IDictionary<string, string> oauthParams)
        {
            return CreateSignature(consumerSecret, "", method, uri, oauthParams);
        }
        private string CreateSignature(string consumerSecret, string tokenSecret, string method, Uri uri, IDictionary<string, string> oauthParams)
        {
            //パラメタをソート済みディクショナリに詰替（OAuthの仕様）
            SortedDictionary<string, string> sorted = new SortedDictionary<string, string>(oauthParams);
            //URLエンコード済みのクエリ形式文字列に変換
            string paramString = CreateQueryString(sorted);
            //アクセス先URLの整形
            string url = string.Format("{0}://{1}{2}", uri.Scheme, uri.Host, uri.AbsolutePath);
            //署名のベース文字列生成（&区切り）。クエリ形式文字列は再エンコードする
            string signatureBase = string.Format("{0}&{1}&{2}", method, UrlEncode(url), UrlEncode(paramString));
            //署名鍵の文字列をコンシューマー秘密鍵とアクセストークン秘密鍵から生成（&区切り。アクセストークン秘密鍵なくても&残すこと）
            string key = UrlEncode(consumerSecret) + "&";
            if (!string.IsNullOrEmpty(tokenSecret))
                key += UrlEncode(tokenSecret);
            //鍵生成＆署名生成
            System.Security.Cryptography.HMACSHA1 hmac = new System.Security.Cryptography.HMACSHA1(Encoding.ASCII.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureBase));
            return Convert.ToBase64String(hash);
        }
        ///<summary>
        ///クエリコレクションをkey=value形式の文字列に構成して戻す
        ///</summary>
        ///<param name="param">クエリ、またはポストデータとなるkey-valueコレクション</param>
        protected string CreateQueryString(IDictionary<string, string> param)
        {
            if (param == null || param.Count == 0)
                return string.Empty;

            StringBuilder query = new StringBuilder();
            foreach (string key in param.Keys)
            {
                query.AppendFormat("{0}={1}&", UrlEncode(key), UrlEncode(param[key]));
            }
            return query.ToString(0, query.Length - 1);
        }
        protected string UrlEncode(string stringToEncode)
        {
            const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            StringBuilder sb = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(stringToEncode);

            foreach (byte b in bytes)
            {
                if (UnreservedChars.IndexOf((char)b) != -1)
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.AppendFormat("%{0:X2}", b);
                }
            }
            return sb.ToString();
        }
    }
}
