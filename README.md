# XQuartzNetPanel

@[TOC](目录)
## 简介
1. 远程管理Job (动态添加job,添加触发器,暂停触发器 等等)
2. 对 CrystalQuartz 进行中文翻译  (中文翻译程度 80%)
3. 修改 添加 job class 的 逻辑. 将 下拉框 改为  三个文本框 (dll类库名称,命名空间,类名)修改后可以随意 添加 job,  
  **使用方法:**
  分别复制 dll 文件 到 web  与 service 的根目录.  (web 添加时 需要,  service  执行时 需要)
  然后 点击添加job ,  并手动输入 相应的新的 job信息 和其他信息,即可 远程添加成功


借鉴两个 开源项目↓

1. **[CrystalQuartz](https://github.com/guryanovev/CrystalQuartz)**

2. **[QuartzNet](https://github.com/quartznet/quartznet)**    

![首页图片](https://github.com/xxxxue/XQuartzNetPanel/blob/master/images/index.jpg)
![详情界面图片](https://github.com/xxxxue/XQuartzNetPanel/blob/master/images/2.jpg)

![添加job class 图片](https://github.com/xxxxue/XQuartzNetPanel/blob/master/images/jobclass.jpg)