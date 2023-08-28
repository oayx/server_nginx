using MaxMind.GeoIP2;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace YX
{
    /// <summary>
    /// 内网服务器
    /// @author hannibal
    /// </summary>
    public static class InternalServer
    {
        /// <summary>
        /// 开启调试
        /// </summary>
        private static bool _enableDebug = false;

        /// <summary>
        /// 服务器筛选算法
        /// </summary>
        private static string _selectRule = "least_conn";
        /// <summary>
        /// 如果是轮训算法，表示当前轮训到的索引
        /// </summary>
        private static int _selectIndex = 0;
        /// <summary>
        /// ip定位用
        /// </summary>
        private static DatabaseReader _databaseReader;

        /// <summary>
        /// 配置的有效服务器列表
        /// </summary>
        private static List<ServerInfo> _servers = new List<ServerInfo>();
        /// <summary>
        /// 收集到的服务器信息,key(ip,port),value(客户端连接数)
        /// </summary>
        private static Dictionary<KeyValuePair<string, ushort>, uint> _serverInfos = new Dictionary<KeyValuePair<string, ushort>, uint>();

        /// <summary>
        /// 地址映射成ip
        /// </summary>
        private static Dictionary<string, KeyValuePair<string, ushort>> _idmaps = new Dictionary<string, KeyValuePair<string, ushort>>();

        static InternalServer()
        {
            LoadConfig();
        }
        /// <summary>
        /// 加载配置信息
        /// </summary>
        public static void LoadConfig()
        {
            _enableDebug = Utils.GetConfigValue("enable_debug") != "0";
            _selectRule = Utils.GetConfigValue("select_rule");
            Console.WriteLine("rule:" + _selectRule);

            {//
                if (_selectRule == "ip_hash")
                {
                    string databasePath = "../GeoLite2/GeoLite2-City.mmdb";
                    try
                    {
                        _databaseReader = new DatabaseReader(databasePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("load GeoLite2 error:" + ex.ToString());
                        _selectRule = "least_conn";
                    }
                }
                else if (_databaseReader != null)
                {
                    _databaseReader.Dispose();
                    _databaseReader = null;
                }
            }

            {//所有服务器
                _servers.Clear();
                var configs = (ServerSection)ConfigurationManager.GetSection("servers");
                if (configs != null)
                {
                    foreach (ServerConfigInfo info in configs.KeyValues)
                    {
                        var serverInfo = new ServerInfo() { Type = info.type, Name = info.name, IP = info.ip, Port = info.port, Weight = info.weight };
                        _servers.Add(serverInfo);
                    }
                }
            }

            {//地址映射
                _idmaps.Clear();
                var configs = (IPMapSection)ConfigurationManager.GetSection("ipmap");
                if (configs != null)
                {
                    foreach (IPMapConfigInfo info in configs.KeyValues)
                    {
                        _idmaps.Add(info.address.ToLower(), new KeyValuePair<string, ushort>(info.ip, info.port));
                    }
                }
            }
        }

        public static KeyValuePair<string, ushort> GetServer(Dictionary<string, string> args)
        {
            KeyValuePair<string, ushort> selectServer = new KeyValuePair<string, ushort>("", 0);

            if (_servers.Count == 0)
                return selectServer;

            switch (_selectRule) 
            {
                case "round_robin"://轮询
                    {
                        int count = _servers.Count;
                        while(count-- >= 0)
                        {
                            _selectIndex = _selectIndex % _servers.Count;
                            var info = _servers[_selectIndex++];
                            if (!info.Valid) continue;
                            selectServer = new KeyValuePair<string, ushort>(info.IP, info.Port);
                        }
                    }
                    break;
                case "least_conn"://最少连接数
                    {
                        //第一步查找最少连接
                        uint minValue = uint.MaxValue;
                        for(int i = 0; i < _servers.Count; ++i)
                        {
                            if (_servers[i].Valid && _servers[i].Count < minValue)
                                minValue = _servers[i].Count;
                        }

                        //最少连接可能有多个，存储到这，然后随机取一个
                        var minLists = new List<int>();
                        for (int i = 0; i < _servers.Count; ++i)
                        {
                            if (_servers[i].Valid && _servers[i].Count == minValue)
                                minLists.Add(i);
                        }

                        //随机取一个
                        int index = Utils.RandRange_List(minLists);
                        var info = _servers[index];
                        selectServer = new KeyValuePair<string, ushort>(info.IP, info.Port);
                    }
                    break;
                case "weight"://权重
                    {
                        var weightLists = new List<KeyValuePair<int, uint>>();
                        for (int i = 0; i < _servers.Count; ++i)
                        {
                            if (_servers[i].Valid)
                                weightLists.Add(new KeyValuePair<int, uint>(i, _servers[i].Weight));
                        }
                        var index = Utils.RandRange_Percent(weightLists);
                        var info = _servers[index];
                        selectServer = new KeyValuePair<string, ushort>(info.IP, info.Port);
                    }
                    break;
                case "ip_hash"://ip地址映射
                    {
                        bool isSelect = false;
                        if(args.TryGetValue("ip", out var ip))
                        {
                            if (ip != "127.0.0.1" && ip != "::1" && !ip.StartsWith("192.168"))
                            {//排除内网ip
                                if (_databaseReader.TryCity(ip, out var response))
                                {//获得省份，也可以改成City(城市)，还可以获得国家
                                    string cityName = response.MostSpecificSubdivision.Name.ToLower();
                                    if(_idmaps.TryGetValue(cityName, out var host))
                                    {//根据地址，获得ip
                                        selectServer = new KeyValuePair<string, ushort>(host.Key, host.Value);
                                        isSelect = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("address map to host error:" + cityName);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("GeoLite2 parse ip error:" + ip);
                                }
                            }
                        }
                        if(!isSelect)
                        {//随机一个
                            var lists = new List<int>();
                            for (int i = 0; i < _servers.Count; ++i)
                            {
                                if (_servers[i].Valid)
                                    lists.Add(i);
                            }

                            int index = Utils.RandRange_List(lists);
                            var info = _servers[index];
                            selectServer = new KeyValuePair<string, ushort>(info.IP, info.Port);
                        }
                    }
                    break;
            }

            //记录信息
            if(_enableDebug && !string.IsNullOrEmpty(selectServer.Key))
            {
                Debuger.Add(selectServer.Key, selectServer.Value);
            }

            return selectServer;
        }
        /// <summary>
        /// 收到服务器上报信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="count"></param>
        /// <param name="valid"></param>
        /// <returns></returns>
        public static bool ModifyServer(string ip, ushort port, uint count, bool valid)
        {
            bool isFind = false;
            foreach(var server in _servers)
            {
                if(server.IP == ip && server.Port == port)
                {
                    server.Count = count;
                    server.Valid = valid;

                    isFind = true;
                    break;
                }
            }
            return isFind;
        }
        public static void DebugServerState()
        {
            foreach (var info in _servers)
            {
                Console.WriteLine($"Name:{info.Name},Address:{info.IP},Port:{info.Port},Count:{info.Count}");
            }
        }
    }
}
