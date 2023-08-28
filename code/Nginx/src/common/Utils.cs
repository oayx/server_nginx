using System;
using System.Collections.Generic;

namespace YX
{
    /// <summary>
    /// 公用方法
    /// @author hannibal
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 配置表
        /// </summary>
        public static string GetConfigValue(string key, string default_value = "")
        {
            try
            {
                var value = System.Configuration.ConfigurationManager.AppSettings[key];
                if (value == null)
                    value = default_value;
                return value;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetConfigValue error:" + e.ToString());
                return default_value;
            }
        }
        public static short ToShort(this string str)
        {
            short v = 0;
            if (short.TryParse(str, out v))
                return v;
            Console.WriteLine("ToShort类型转化失败:" + str);
            return 0;
        }
        public static ushort ToUShort(this string str)
        {
            ushort v = 0;
            if (ushort.TryParse(str, out v))
                return v;
            Console.WriteLine("ToUShort类型转化失败:" + str);
            return 0;
        }
        public static int ToInt(this string str)
        {
            int v = 0;
            if (int.TryParse(str, out v))
                return v;
            Console.WriteLine("ToInt类型转化失败:" + str);
            return 0;
        }
        public static uint ToUInt(this string str)
        {
            uint v = 0;
            if (uint.TryParse(str, out v))
                return v;
            Console.WriteLine("ToUInt类型转化失败:" + str);
            return 0;
        }

        static private Random ro = new Random();
        /// <summary>
        /// 产生随机数
        /// </summary>
        /// <returns>结果：x>=0 && x<1</returns>
        public static float Range()
        {
            return ((float)ro.Next(0, int.MaxValue)) / ((float)int.MaxValue);
        }
        /// <summary>
        /// 产生随机数
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns>x>=param1 && x<param2</returns>
        public static int RandRange(int param1, int param2)
        {
            return ro.Next(param1, param2);
        }
        /**
         * 从数组中产生随机数[-1,1,2]
         * 结果：-1/1/2中的一个
         */
        public static T RandRange_Array<T>(T[] arr)
        {
            T loc = arr[RandRange(0, arr.Length)];
            return loc;
        }
        public static T RandRange_List<T>(List<T> arr)
        {
            T loc = arr[RandRange(0, arr.Count)];
            return loc;
        }
        /// <summary>
        /// 根据概率随机
        /// </summary>
        /// <param name="list">概率分布</param>
        /// <returns>索引</returns>
        public static int RandRange_Percent(List<uint> list)
        {
            if (list == null || list.Count == 0) return -1;

            uint totalPercent = 0;
            list.ForEach(delegate (uint v)
            {
                if (v == 0)
                    Console.WriteLine("权重设置有误，不能为0");
                totalPercent += v; 
            });
            //随机一个概率值，用这个概率值去判断落在哪个区间；之所以从0开始，是后面的区间判断使用的是>=&&<方式
            int randomValue = RandRange(0, (int)totalPercent);

            uint curTotal = 0;
            //遍历需要随机的道具列表，看处于哪个区间
            for (int i = 0; i < list.Count; ++i)
            {
                uint curRandom = list[i];
                if (randomValue >= curTotal && randomValue < curTotal + curRandom)
                {
                    return i;
                }
                else
                {
                    curTotal += curRandom;
                }
            }
            return 0;
        }
        /// <summary>
        /// 根据概率随机
        /// </summary>
        /// <param name="list">键-索引，值-权重</param>
        /// <returns>键</returns>
        public static T RandRange_Percent<T>(List<KeyValuePair<T, uint>> list)
        {
            if (list == null || list.Count == 0)
                throw new System.Exception("无效列表");

            uint totalPercent = 0;
            list.ForEach((KeyValuePair<T, uint> v) =>
            {
                if (v.Value == 0) Console.WriteLine("权重设置有误，不能为0");
                totalPercent += v.Value;
            });
            //随机一个概率值，用这个概率值去判断落在哪个区间；之所以从0开始，是后面的区间判断使用的是>=&&<方式
            int randomValue = RandRange(0, (int)totalPercent);

            uint curTotal = 0;
            //遍历需要随机的道具列表，看处于哪个区间
            for (int i = 0; i < list.Count; ++i)
            {
                uint curRandom = list[i].Value;
                if (randomValue >= curTotal && randomValue < curTotal + curRandom)
                {
                    return list[i].Key;
                }
                else
                {
                    curTotal += curRandom;
                }
            }
            return list[0].Key;
        }
    }
}
