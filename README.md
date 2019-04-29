# XQuartzNetPanel

## 为什么会出现这个项目
1. 为了方便大家去学习 QuartzNet 与 CrystalQuartz


## 准备
1. Node.js
2. .NET Framework **4.7.2** 和 **4.5.2** 和 **4.5** 和 **4**  
3. (非必须)ASP NET Core 2.2 

## 简介
1. 持久化到数据库(本项目里用的是SQLServer) **[表结构请点击这里](https://github.com/quartznet/quartznet/tree/master/database/tables)**;
2. 远程管理Job (动态添加job,添加触发器,暂停触发器 等等)
3. 对 CrystalQuartz 进行中文翻译  (中文翻译程度 80%)
4. 修改 添加 job class 的 逻辑. 将 下拉框 改为  三个文本框 (dll类库名称,命名空间,类名)修改后可以随意 添加 job,  
  **使用方法:**   
  分别复制 dll 文件 到 web  与 service 的根目录.    (web 添加时 需要,  service  执行时 需要)  
  然后 点击添加job ,  并手动输入 相应的新的 job信息 和其他信息,即可 远程添加成功
5. 调整一些CSS 样式
6. 代码增加很多注释

## 如何运行

![运行过程图片](https://github.com/xxxxue/XQuartzNetPanel/blob/master/images/run.jpg)

## 结构图

<details>
<summary>展开查看</summary>
<pre><code>
XQuartzNetPanel
│
├── CrystalQuartz : CrystalQuartz源码
│    │
│    ├── Core 
│    │     │
│    │     ├── CrystalQuartz.Core.Quartz2 : 2.x 版本的操作
│    │     └── CrystalQuartz.Core.Quartz3 : 3.x 版本的操作
│    │
│    ├── CrystalQuartz.Application  :CrystalQuartz的核心启动类库
│    │
│    └── CrystalQuartz.Application.Client   : Node.js  页面
│
├── QuartzService  
│    │
│    ├──Job  : job 类库文件夹
│    │
│    └──QuartzService  : 跑job 的服务  (使用 Quartz 2.6.2.0 版本)
│         └──quartz.config : QuartzNet 配置文件 (记得改成自己的数据库)
│
│
└── WebPanel : 管理页面 文件夹
    │
    └──XQuartz.Web
         ├── Helper
         │     └── FakeProvider.cs : Web项目 连接  Service 的 地方 (记得改为自己的tcp连接)
         └──  Web.config : 有很多的配置.都是中文注释.
</code></pre>
</details>

## 借鉴两个 开源项目↓

1. **[CrystalQuartz](https://github.com/guryanovev/CrystalQuartz)**

2. **[QuartzNet](https://github.com/quartznet/quartznet)**    

## 示例图片
![首页图片](https://github.com/xxxxue/XQuartzNetPanel/blob/master/images/index.jpg)
![详情界面图片](https://github.com/xxxxue/XQuartzNetPanel/blob/master/images/2.jpg)

![添加job class 图片](https://github.com/xxxxue/XQuartzNetPanel/blob/master/images/jobclass.jpg)