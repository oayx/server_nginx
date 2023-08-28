using System;
using System.Collections.Generic;

namespace YX
{
    public static class Debuger
    {
        /// <summary>
        /// 每个服务器选中的次数,key(ip+port),value(次数)
        /// </summary>
        private static Dictionary<string, int> _server2Count = new Dictionary<string, int>();
        /// <summary>
        /// 每秒最大请求数
        /// </summary>
        private static uint _maxRequestCount = 0;
        /// <summary>
        /// 当前请求数
        /// </summary>
        private static uint _lastRequestCount = 0;
        private static uint _currRequestCount = 0;

        public static void Update()
        {
            if (_currRequestCount > _maxRequestCount)
            {
                _maxRequestCount = _currRequestCount;
            }
            _lastRequestCount = _currRequestCount;
            _currRequestCount = 0;
        }

        public static void Add(string ip, ushort port)
        {
            _currRequestCount++;

            string key = $"{ip}:{port}";
            if (!_server2Count.TryGetValue(key, out var count))
            {
                count = 0;
                _server2Count.Add(key, 0);
            }
            _server2Count[key] = count + 1;
        }

        public static void DebugSelectCount()
        {
            foreach (var info in _server2Count)
            {
                Console.WriteLine($"Address:{info.Key},Count:{info.Value}");
            }
        }
        public static void DebugRequestCount()
        {
            Console.WriteLine($"MaxRequestCount:{_maxRequestCount},CurrRequestCount:{_lastRequestCount}");
        }
    }
}
