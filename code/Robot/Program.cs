using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace client
{
    class Program
    {
        private static string _ip = "";
        private static ushort _port = 0;

        static void Main(string[] args)
        {
            _ip = System.Configuration.ConfigurationManager.AppSettings["nginx_ip"];
            _port = ushort.Parse(System.Configuration.ConfigurationManager.AppSettings["nginx_port"]);

            while (true)
            {
                SendGet();
                //string str = Console.ReadLine();
                //if (str == "quit")
                //    break;
                Thread.Sleep(10);
            }
            Console.ReadKey();
        }

        static async void SendGet()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"http://{_ip}:{_port}/";
                // 构建请求 URL，将参数拼接到 URL 中
                url = $"{url}{"server"}?spid={"test"}&platform={"android"}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
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
        }
        static async void SendPost()
        {
            using (HttpClient client = new HttpClient())
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
