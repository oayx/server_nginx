using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.Http;

namespace Server
{
    class Server
    {
        protected Socket _socket;
        protected List<Socket> _clientSockets = new List<Socket>();
        private ushort _port = 0;
        private string _nginxIp = "";
        private ushort _nginxPort = 0;
        private uint _clientCount = 0;
        private byte[] _dataBuffer = new byte[0xFFFF];

        static void Main(string[] args)
        {
            ushort port = ushort.Parse(System.Configuration.ConfigurationManager.AppSettings["port"]);
            Console.WriteLine("端口:" + port);

            Server s = new Server();
            s.Listen(port);

            while (true)
            {
                s.Update();
                Thread.Sleep(10);    //等待1秒钟
            }
        }

        public Server()
        {
            _nginxIp = System.Configuration.ConfigurationManager.AppSettings["nginx_ip"];
            _nginxPort = ushort.Parse(System.Configuration.ConfigurationManager.AppSettings["nginx_port"]);
        }

        public virtual bool Listen(ushort port)
        {
            //开启服务区  
            _port = port;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Bind(new IPEndPoint(IPAddress.Any, port));  //绑定IP地址：端口  
                _socket.Listen(10);    //设定最多10个排队连接请求  
            }
            catch (SocketException e)
            {
                Console.WriteLine("server setup failed:" + e.ToString());
                return false;
            }
            _socket.Blocking = true;
            _socket.NoDelay = true;
            _socket.SendBufferSize = 0xFFFF;
            _socket.ReceiveBufferSize = 0xFFFF;
            _socket.SendTimeout = 0xbb8;
            _socket.ReceiveTimeout = 0xbb8;
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Console.WriteLine("服务器成功开启!!!");

            //开始接受连接，异步。
            _socket.BeginAccept(new AsyncCallback(OnAcceptClientConnect), _socket);

            return true;
        }

        private void Update()
        {
            for (int i = _clientSockets.Count - 1; i >= 0; --i)
            {
                Socket socket = _clientSockets[i];
                if (socket == null || !socket.Connected)
                {
                    --_clientCount;
                    PostClientCount();

                    _clientSockets.RemoveAt(i);

                    Console.WriteLine("有客户端退出");
                    continue;
                }
                if (socket.Available > 0)
                {
                    int len = 0;
                    try
                    {
                        len = socket.Receive(_dataBuffer, SocketFlags.None);
                        if (len == 0)
                        {
                            --_clientCount;
                            PostClientCount();

                            _clientSockets.RemoveAt(i);
                            Console.WriteLine("有客户端退出");
                        }
                        else
                        {
                            string utf8string = System.Text.Encoding.UTF8.GetString(_dataBuffer, 0, len);
                            Console.Write(string.Format("\n收到客户端数据，长度:{0}, 内容：{1}", len, utf8string));
                            //收到后立刻把消息返回给客户端
                            socket.Send(_dataBuffer, 0, len, SocketFlags.None);
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("HandleReceive SocketException:" + e.Message);
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return;
                    }
                }
            }
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void OnAcceptClientConnect(IAsyncResult ar)
        {
            Console.WriteLine("有客户端进来");

            //初始化一个SOCKET，用于其它客户端的连接
            Socket server_socket = (Socket)ar.AsyncState;
            Socket client_socket = server_socket.EndAccept(ar);
            _clientSockets.Add(client_socket);

            ++_clientCount;
            PostClientCount();

            //等待新的客户端连接
            server_socket.BeginAccept(new AsyncCallback(OnAcceptClientConnect), server_socket);
        }

        /// <summary>
        /// 上报客户端数量
        /// </summary>
        private async void PostClientCount()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"http://{_nginxIp}:{_nginxPort}/";
                // 构建 POST 请求的表单数据
                var formData = new Dictionary<string, string>
                {
                    { "ip", "127.0.0.1" },
                    { "valid", "true" },
                    { "port", _port.ToString() },
                    { "count", _clientCount.ToString() }
                };

                try
                {
                    // 构建 HTTP 请求内容
                    var content = new FormUrlEncodedContent(formData);
                    HttpResponseMessage response = await client.PostAsync(url, content);


                    if (response.IsSuccessStatusCode)
                    {
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
        }
    }
}