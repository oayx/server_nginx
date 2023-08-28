namespace YX
{
    public struct RequestInfo
    {
        public string key;
        public string value;
    }
    /// <summary>
    /// 返回码
    /// </summary>
    public enum ResponseCode
    {
        Unknow = -1,//服务器内部错误
        Succeed,    //成功
        Failed,     //失败
    }
    /// <summary>
    /// 内网服务器信息
    /// </summary>
    public class ServerInfo
    {
        public ushort Type;
        public string Name;
        public string IP;
        public ushort Port;
        /// <summary>
        /// 权重
        /// </summary>
        public ushort Weight = 0;
        /// <summary>
        /// 当前连接数
        /// </summary>
        public uint Count = 0;
        /// <summary>
        /// 是否有效:考虑关服等情况
        /// </summary>
        public bool Valid = true;
    }
}
