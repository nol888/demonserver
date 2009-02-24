namespace dAmnSharp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    internal class dALogin
    {
        public static string GetAuthtoken(string username, string password)
        {
            HttpWebResponse response = null;
            string str = "";
            string str2 = "http://www.deviantart.com/";
            Stream requestStream = null;
            Uri requestUri = new Uri("http://www.deviantart.com/users/login");
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Referer = str2;
            request.UserAgent = "dAmnSharp 1.0";
            StringBuilder builder = new StringBuilder();
            builder.Append("ref=");
            builder.Append(HttpUtility.UrlEncode(str2));
            builder.Append("&username=");
            builder.Append(HttpUtility.UrlEncode(username));
            builder.Append("&password=");
            builder.Append(HttpUtility.UrlEncode(password));
            builder.Append("&action=Login&reusetoken=1");
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            request.ContentLength = bytes.Length;
            try
            {
                try
                {
                    requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                catch
                {
                    return "";
                }
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
            }
            try
            {
                try
                {
                    request.CookieContainer = new CookieContainer();
                    response = (HttpWebResponse) request.GetResponse();
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                    string pattern = string.Concat(new object[] { "s:9:", '"', "authtoken", '"', ";s:32:", '"', "(.*?)", '"', ";" });
                    string input = HttpUtility.UrlDecode(response.Cookies["userinfo"].Value);
                    if (Regex.IsMatch(input, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase))
                    {
                        str = Regex.Replace(Regex.Match(input, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase).Value, pattern, "$1", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    }
                }
                catch
                {
                    return "";
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return str;
        }
    }
}

