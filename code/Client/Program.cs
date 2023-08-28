using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        private static Socket _socket;
        private static byte[] _recvBuffer = new byte[4096];   //读缓存

        static void Main(string[] args)
        {
            //获得服务器
            var serverInfo = GetServer().GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(serverInfo.Key))
            {
                Console.WriteLine("获得服务器失败!!!");
                Console.ReadLine();
                return;
            }

            //设定服务器IP地址  
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(new DnsEndPoint(serverInfo.Key, serverInfo.Value)); 
                Console.WriteLine("连接服务器成功!!!");

                BeginReceive();

                while (_socket != null)
                {
                    try
                    {
                        string input_msg = Console.ReadLine();
                        if (_socket != null && !string.IsNullOrEmpty(input_msg))
                        {
                            byte[] by = System.Text.Encoding.UTF8.GetBytes(input_msg);
                            int count = _socket.Send(by, 0, by.Length, SocketFlags.None);
                            Console.WriteLine("发送字节数：" + count);
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("网络错误：" + e.ToString());
                        Close();
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("连接服务器失败:" + ex.ToString());
            }
            Console.ReadLine();
        }
        private static void Close()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
            Console.WriteLine("客户端连接已经关闭");
        }
        private static void BeginReceive()
        {
            if (_socket == null) return;
            _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), _recvBuffer);
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        private static void OnReceive(IAsyncResult ar)
        {
            if (_socket == null) return;
            try
            {
                ar.AsyncWaitHandle.Close();
                byte[] buf = (byte[])ar.AsyncState;
                int len = _socket.EndReceive(ar);
                if (len > 0)
                {
                    string utf8string = System.Text.Encoding.UTF8.GetString(buf, 0, len);
                    Console.WriteLine(string.Format("收到服务器数据，长度:{0},内容:{1},时间:{2}", len, utf8string, MillisecondSince1970));

                    BeginReceive();
                }
                else
                {
                    Close();
                    return;
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10054) Console.WriteLine(e.ToString());
                Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Close();
            }
        }
        private static long MillisecondSince1970
        {
            get
            {
                System.TimeSpan duration = System.DateTime.Now - DateTime.Parse("1970-1-1");
                return (long)duration.TotalMilliseconds;
            }
        }
        private static async Task<KeyValuePair<string, ushort>> GetServer()
        {
            using (HttpClient client = new HttpClient())
            {
                string ip = System.Configuration.ConfigurationManager.AppSettings["nginx_ip"];
                ushort port = ushort.Parse(System.Configuration.ConfigurationManager.AppSettings["nginx_port"]);
                string url = $"http://{ip}:{port}/";
                // 构建请求 URL，将参数拼接到 URL 中
                url = $"{url}{"server"}?spid={"test"}&platform={"android"}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Server Response:" + content);
                        string pattern = @"ip=(\d+\.\d+\.\d+\.\d+),port=(\d+)";
                        Match match = Regex.Match(content, pattern);
                        if (match.Success)
                        {
                            string serverIp = match.Groups[1].Value;
                            string serverPort = match.Groups[2].Value;
                            return new KeyValuePair<string, ushort>(serverIp, ushort.Parse(serverPort));
                        }
                        else
                        {
                            Console.WriteLine("Response error:" + content);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return new KeyValuePair<string, ushort>();
        }
    }
}