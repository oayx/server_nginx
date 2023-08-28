using System;
using System.Configuration;

namespace YX
{
    /// <summary>
    /// 地址映射ip
    /// @author hannibal
    /// </summary>
    public class IPMapSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _property = new ConfigurationProperty(string.Empty, typeof(IPMapCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public IPMapCollection KeyValues
        {
            get
            {
                return (IPMapCollection)base[_property];
            }
        }
    }
    /// <summary>
    /// 自定义一个集合
    /// </summary>
    [ConfigurationCollection(typeof(IPMapConfigInfo))]
    public class IPMapCollection : ConfigurationElementCollection
    {
        // 基本上，所有的方法都只要简单地调用基类的实现就可以了。

        public IPMapCollection()
            : base(StringComparer.OrdinalIgnoreCase)    // 忽略大小写
        {
        }

        // 其实关键就是这个索引器。但它也是调用基类的实现，只是做下类型转就行了。
        new public IPMapConfigInfo this[string name]
        {
            get
            {
                return (IPMapConfigInfo)BaseGet(name);
            }
        }
        public IPMapConfigInfo this[int index]
        {
            get
            {
                return (IPMapConfigInfo)BaseGet(index);
            }
        }

        // 下面二个方法中抽象类中必须要实现的。
        protected override ConfigurationElement CreateNewElement()
        {
            return new IPMapConfigInfo();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IPMapConfigInfo)element).address;
        }

        // 说明：如果不需要在代码中修改集合，可以不实现Add, Clear, Remove
        public void Add(IPMapConfigInfo setting)
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
    public class IPMapConfigInfo : ConfigurationElement
    {
        [ConfigurationProperty("address", IsRequired = true)]
        public string address
        {
            get { return this["address"].ToString(); }
            set { this["address"] = value; }
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
    }
}