using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace YX
{
    /// <summary>
    /// http请求处理
    /// @author hannibal
    /// </summary>
    public class HttpRequest
    {
        #region 解析
        /// <summary>
        /// 解析get请求
        /// </summary>
        public void PushGet(HttpListenerContext httpContext)
        {
            if (httpContext.Request.Url.AbsolutePath.StartsWith("/server"))
            {//识别是请求获得服务器列表
                string clientIP = httpContext.Request.RemoteEndPoint.Address.ToString();
                //Console.WriteLine(clientIP);

                //解析参数
                var args = new Dictionary<string, string>();
                args.Add("ip", clientIP);
                var queryString = httpContext.Request.QueryString;
                for (int i = 0; i < queryString.Count; ++i)
                {
                    args.Add(queryString.GetKey(i), queryString.Get(i));
                }

                var server = InternalServer.GetServer(args);
                string response = "";
                if(string.IsNullOrEmpty(server.Key))
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
                else
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    response = $"ip={server.Key},port={server.Value}";
                }
                using (StreamWriter writer = new StreamWriter(httpContext.Response.OutputStream))
                {
                    writer.Write(response);
                }
            }
        }
        /// <summary>
        /// 解析post请求
        /// </summary>
        public void PushPost(HttpListenerContext httpContext)
        {
            using (var reader = new StreamReader(httpContext.Request.InputStream, System.Text.Encoding.UTF8))
            {
                string content = reader.ReadToEnd();
                if (string.IsNullOrEmpty(content))
                    return;

                string[] arrs = content.Split('&');
                if (arrs == null || arrs.Length == 0) 
                    return;

                string ip = ""; ushort port = 0;uint count = 0; bool valid = false;
                for (int i = 0; i < arrs.Length; ++i)
                {
                    string[] arr = arrs[i].Split('=');
                    if (arr == null || arr.Length != 2) continue;
                    switch(arr[0])
                    {
                        case "ip":ip = arr[1]; break;
                        case "port": port = arr[1].ToUShort(); break;
                        case "count": count = arr[1].ToUInt(); break;
                        case "valid": valid = ((arr[1].Equals("false", StringComparison.OrdinalIgnoreCase) || arr[1] == "0") ? false : true); break;
                        default:Console.WriteLine("error post param:" + arr[0]);break;
                    }
                }

                bool result = InternalServer.ModifyServer(ip, port, count, valid);
                if(result)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                using (StreamWriter writer = new StreamWriter(httpContext.Response.OutputStream))
                {
                    writer.Write("");
                }
            }
        }
        #endregion
    }
}
