
## 主要功能


实现了类似Nginx的负载均衡，


## 功能介绍

1.Client：客户端程序，

2.Nginx：负载负载均衡的http服务器

3.Server：后端服务器

3.Robot：客户端压力测试程序


## 实现流程

1.客户端先发一个http请求给Nginx，收到请求后，筛选合适的内网服务器，把ip和端口返回给客户端
2.客户端拿到Nginx返回的ip和端口，连接后端服务器


## Nginx实现了以下算法
1.round_robin：轮询模式，按顺序分配给客户端

2.least_conn：最少连接模式，分配连接数最少的服务器给客户端(需要服务器主动上报连接数)

3.weight：根据权重随机分配

4.ip_hash：ip映射模式，根据客户端ip，查询映射表，找到对应的服务器(可以实现多节点，就近连接)


## App.config的配置说明
1.servers节点用于配置内网服务器信息，包括ip、端口、权重

2.ipmap用于配置区域地址和服务器的映射。目前区域地址是获得省一级


## 其他说明
1.根据ip获得所在的省份，用的是GeoLite2库，数据截止时间是(2023.8月)，这个网站可以获得最新的数据https://github.com/P3TERX/GeoLite.mmdb/releases


## 效果演示
<br><img src='image/1.png'><br>
