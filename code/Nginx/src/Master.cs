using System;

namespace YX
{
    /// <summary>
    /// 管理器
    /// @author hannibal
    /// </summary>
    public static class Master
    {
        private static bool _isStop = false;
        private static CmdHandle _cmdHandle = new CmdHandle();
        private static HttpService _httpServer = new HttpService();

        public static void Setup()
        {
            Console.WriteLine("start time(" + DateTime.Now.ToString() + ")");
            Console.Title = "Http Server";

            _cmdHandle.Setup();
            _httpServer.Setup();
        }
        public static void Destroy()
        {
            _isStop = true;
            _httpServer.Destroy();
            _cmdHandle.Destroy();
            Console.WriteLine("服务器关闭");
        }
        public static void Update()
        {
            if (_isStop)
                return;

            Debuger.Update();
        }

        public static void Start()
        {
            _httpServer.Start();

            while (!_isStop)
            {
                Update();
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
