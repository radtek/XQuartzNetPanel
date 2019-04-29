# 水晶石英前端工程

这个项目包含 面板 的所有客户端代码，只要一些前端开发工具。

## dev 快速启动

- `npm install` (仅第一次)
- `npm run watch` - 开始监视和生成源更改的客户端
- 在单独的窗口中 - `npm run dev-server` - builds and runs dev-server
- navigate to [http://localhost:3000](http://localhost:3000)

## What is the dev-server?

它是一个很小的nodejs Web应用程序，用作CrystalQuartz的开发后端。
dev服务器使得在不重新编译.NET部分的情况下更容易编写和调试前端代码。

## 所有可用的“npm”脚本

- `npm run watch` - 开始监视源代码并在每次更改时重建
- `npm run build-debug` - 在*debug*模式下生成面板并将资产保存到'dist'目录
- `npm run build-release` - 在*release*模式下构建面板并将资产保存到'dist'目录
- `npm run build-demo` - 构建静态无服务器面板演示，在浏览器中模拟quartz.net调度程序。将assest保存到'project\root/src/artifacts/gh pages/demo`
- `npm run build-dev-server` - 创建源服务器
- `npm run run-dev-server` -在端口3000上运行dev server
- `npm run dev-server` - 生成然后运行dev server