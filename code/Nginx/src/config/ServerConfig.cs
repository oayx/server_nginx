using System;
using System.Configuration;

namespace YX
{
    /// <summary>
    /// 服务器列表
    /// @author hannibal
    /// </summary>
    public class ServerSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _property = new ConfigurationProperty(string.Empty, typeof(ServerCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ServerCollection KeyValues
        {
            get
            {
                return (ServerCollection)base[_property];
            }
        }
    }
    /// <summary>
    /// 自定义一个集合
    /// </summary>
    [ConfigurationCollection(typeof(ServerConfigInfo))]
    public class ServerCollection : ConfigurationElementCollection
    {
        // 基本上，所有的方法都只要简单地调用基类的实现就可以了。

        public ServerCollection()
            : base(StringComparer.OrdinalIgnoreCase)    // 忽略大小写
        {
        }

        // 其实关键就是这个索引器。但它也是调用基类的实现，只是做下类型转就行了。
        new public ServerConfigInfo this[string name]
        {
            get
            {
                return (ServerConfigInfo)BaseGet(name);
            }
        }
        public ServerConfigInfo this[int index]
        {
            get
            {
                return (ServerConfigInfo)BaseGet(index);
            }
        }

        // 下面二个方法中抽象类中必须要实现的。
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerConfigInfo();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerConfigInfo)element).name;
        }

        // 说明：如果不需要在代码中修改集合，可以不实现Add, Clear, Remove
        public void Add(ServerConfigInfo setting)
        {
            BaseAdd(setting);
        }
        public void Clear()
        {
            BaseClear();
        }
        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
    /// <summary>
    /// 集合中的每个元素
    /// </summary>
    public class ServerConfigInfo : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = false)]
        public ushort type
        {
            get { return ushort.Parse(this["type"].ToString()); }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = false)]
        public string name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("ip", IsRequired = true)]
        public string ip
        {
            get { return this["ip"].ToString(); }
            set { this["ip"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public ushort port
        {
            get { return ushort.Parse(this["port"].ToString()); }
            set { this["port"] = value; }
        }
        [ConfigurationProperty("weight", IsRequired = false)]
        public ushort weight
        {
            get { return ushort.Parse(this["weight"].ToString()); }
            set { this["weight"] = value; }
        }
    }
}