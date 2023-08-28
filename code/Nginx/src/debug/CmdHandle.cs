using System;
using System.Threading.Tasks;

namespace YX
{
    /// <summary>
    /// 处理命令
    /// @author hannibal
    /// </summary>
    public class CmdHandle
    {
        private bool _isQuit = false;
        public void Setup()
        {
            Console.WriteLine("cmd");
            Console.WriteLine("clr:     clearup");
            Console.WriteLine("quit:    close server");
            Console.WriteLine("reload:  reload config");
            Console.WriteLine("server:  print server state");
            Console.WriteLine("count:   print server select count");
            Console.WriteLine("request: print request count per seconds");
            Console.WriteLine("");

            Task.Run(() =>
            {
                while(!_isQuit)
                {
                    string cmd = Console.ReadLine();
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        switch (cmd)
                        {
                            case "clr":
                                Console.Clear();
                                break;
                            case "quit":
                                Master.Destroy();
                                break;
                            case "reload":
                                InternalServer.LoadConfig();
                                break;
                            case "server":
                                InternalServer.DebugServerState();
                                break;
                            case "count":
                                Debuger.DebugSelectCount();
                                break;
                            case "request":
                                Debuger.DebugRequestCount();
                                break;
                        }
                    }
                }
            });
        }

        public void Destroy()
        {
            _isQuit = true;
        }
    }
}
