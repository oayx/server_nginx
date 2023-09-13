using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        private static string _ip = "";
        private static ushort _port = 0;

        private static long _maxDelay = 0;
        private static long _totalDelay = 0;
        private static long _sendCount = 0;

        private static HttpClient _client = new HttpClient();
        private static Stopwatch _stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            _ip = System.Configuration.ConfigurationManager.AppSettings["nginx_ip"];
            _port = ushort.Parse(System.Configuration.ConfigurationManager.AppSettings["nginx_port"]);

            Task.Run(() =>
            {
                while(true)
                {
                    string cmd = Console.ReadLine();
                    switch (cmd)
                    {
                        case "delay":
                            Console.WriteLine($"max delay:{_maxDelay}ms,avg delay:{_totalDelay / _sendCount}ms, count:{_sendCount}");
                            break;
                        case "reset":
                            _maxDelay = _totalDelay = _sendCount = 0;
                            break;
                    }
                    Thread.Sleep(1000);
                }
            });

            Task.Run(() =>
            {
                TestRequest();
            });

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        static async void TestRequest()
        {
            while (true)
            {
                _stopwatch.Restart();

                await SendGet();

                _sendCount++;
                long delay = _stopwatch.ElapsedMilliseconds;
                if (delay > _maxDelay)
                    _maxDelay = delay;
                _totalDelay += delay;

                //Thread.Sleep(1);
            }
        }

        static async Task SendGet()
        {
            string url = $"http://{_ip}:{_port}/";
            // 构建请求 URL，将参数拼接到 URL 中
            url = $"{url}{"server"}?spid={"test"}&platform={"android"}";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                _stopwatch.Stop();
                if (response.IsSuccessStatusCode)
                {
                    //string content = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("Server Response:" + content);
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
        static async void SendPost()
        {
            string url = $"http://{_ip}:{_port}/";
            // 构建请求 URL，将参数拼接到 URL 中
            url = $"{url}{"server"}?name={"test"}&age={12}";

            try
            {
                // 构建 POST 请求的表单数据
                var formData = new Dictionary<string, string>
                    {
                        { "ip", "127.0.0.1" },
                        { "port", "7001" },
                        { "valid", "true" },
                        { "count", "100" }
                    };

                // 构建 HTTP 请求内容
                var content = new FormUrlEncodedContent(formData);
                HttpResponseMessage response = await _client.PostAsync(url, content);
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
