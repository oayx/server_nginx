using System;
using System.IO;
using System.Net;
using System.Threading;

namespace YX
{
    /// <summary>
    /// 处理http请求
    /// HttpListener提供一个简单的、可通过编程方式控制的 HTTP 协议侦听器。通过它可以很容易的提供一些Http服务，而无需启动IIS这类大型服务程序
    /// 例如：http://localhost:9090/index.html?name=125&psw=123456
    /// 
    /// URL配置规则(从MSDN复制过来的:https://docs.microsoft.com/zh-cn/dotnet/api/system.net.httplistener?redirectedfrom=MSDN&view=netframework-4.8)
    ///     1.URI 前缀字符串由方案（http 或 https）、主机、可选端口和可选路径组成。 http://www.contoso.com:8080/customerData/ 的完整前缀字符串的示例。 前缀必须以正斜杠（"/"）结尾。 前缀与请求的 URI 最匹配的 HttpListener 对象将响应请求。 多个 HttpListener 对象无法添加相同的前缀;如果 HttpListener 添加已在使用的前缀，则会引发 Win32Exception 异常。
    ///     2.指定端口后，可以使用 "*" 替换主机元素，以指示如果请求的 URI 与任何其他前缀都不匹配，则 HttpListener 接受发送到端口的请求。 例如，若要接收请求的 URI 未由任何 HttpListener处理时发送到端口8080的所有请求，前缀为http://*： 8080/。 同样，若要指定 HttpListener 接受发送到端口的所有请求，请将主机元素替换为 "+" 字符。 例如，https://+:8080。 "*" 和 "+" 字符可以出现在包含路径的前缀中。
    ///     3.从 Windows 10 上的 .NET Core 2.0 或 .NET Framework 4.6 开始，由 HttpListener 对象管理的 URI 前缀支持通配符子域。 若要指定通配符子域，请在 URI 前缀中将 "*" 字符用作主机名的一部分。 例如， http://*. foo.com/。 将此作为参数传递给 Add 方法。 这适用于 Windows 10 上的 .NET Core 2.0 或 .NET Framework 4.6;在早期版本中，这将生成 HttpListenerException
    /// @author hannibal
    /// </summary>
    public class HttpService
    {
        /// <summary>
        /// 线程是否关闭
        /// </summary>
        private bool _disposed = false;
        private HttpListener _httpListerner;
        /// <summary>
        /// request请求
        /// </summary>
        private HttpRequest _requests = new HttpRequest();

        public void Setup()
        {
            string port = Utils.GetConfigValue("port_for_server");
            Console.WriteLine("port:" + port);

            //打开端口
            this.AddFireWallPort(port.ToUShort());

            //TODO:为了防止无效请求，需要增加安全措施
            _httpListerner = new HttpListener();
            _httpListerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _httpListerner.Prefixes.Add("http://*:" + port + "/");
        }
        public void Destroy()
        {
            _disposed = true;
            _httpListerner.Close();
        }
        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            _httpListerner.Start();

            //启动新线程处理逻辑
            new Thread(new ThreadStart(OnRequestThread)).Start();
        }
        /// <summary>
        /// 防火墙打开端口
        /// </summary>
        /// <param name="port"></param>
        private void AddFireWallPort(int port)
        {
            string argsDll = String.Format(@"firewall set portopening TCP {0} ENABLE", port);
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("netsh", argsDll);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            System.Diagnostics.Process.Start(psi).WaitForExit();
        }
        /// <summary>
        /// 处理请求
        /// </summary>
        private void OnRequestThread()
        {
            while (!_disposed)
            {
                try
                {
                    HttpListenerContext httpListenerContext = _httpListerner.GetContext();
                    if (httpListenerContext.Request.HttpMethod == "GET")
                    {//get用于处理服务器列表请求
                        _requests.PushGet(httpListenerContext);
                    }
                    else if (httpListenerContext.Request.HttpMethod == "POST")
                    {//post处理内网服务器上报信息
                        _requests.PushPost(httpListenerContext);
                    }

                    //响应
                    //httpListenerContext.Response.StatusCode = 200;
                    //using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                    //{
                    //    writer.Write(((int)ResponseCode.Succeed).ToString());
                    //}
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);

                    try
                    {
                        //发送失败消息
                        HttpListenerContext requestContext = _httpListerner.GetContext();
                        requestContext.Response.StatusCode = 500;
                        using (StreamWriter writer = new StreamWriter(requestContext.Response.OutputStream))
                        {
                            writer.Write(((int)ResponseCode.Unknow).ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        _disposed = true;
                    }
                }
            }
            Thread.Sleep(10);
        }
    }
}