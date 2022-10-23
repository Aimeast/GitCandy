## GitCandy
GitCandy© 是一个基于 ASP.NET MVC 的 [Git](http://git-scm.com/documentation) 版本控制服务端，支持公有和私有代码库，可不受限制的创建代码代码库，随时随地的与团队进行协作。

演示网站不可用

源代码： [http://github.com/Aimeast/GitCandy](http://github.com/Aimeast/GitCandy)

---
### 系统要求
* [IIS 7.0](http://www.iis.net/learn)
* [.NET Framework 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
* [ASP.NET MVC 5](http://www.asp.net/mvc/tutorials/mvc-5)
* [Git](http://git-for-windows.github.io/)
* [Sqlite](http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki) 或 [Sql Server](http://www.microsoft.com/en-us/sqlserver/get-sql-server/try-it.aspx)

---
### 安装
* 下载最新[发布](http://github.com/Aimeast/GitCandy/releases)的版本或自己编译最新的[dev](http://github.com/Aimeast/GitCandy/dev)分支源码
* 在IIS创建一个站点，并把二进制文件和资源文件复制到站点目录
* 如果用了 Visual Studio 的发布功能，还要复制`GitCandy\bin\[NativeBinaries & x86 & x64]`文件夹到站点目录
* 用`/Sql/Create.[Sqlite | MsSql].sql`脚本创建一个数据库。如果创建的是Sqlite数据库，还需把数据库文件复制到`App_Data`文件夹
* 更新`Web.config`文件的数据库连接串
* 准备两个文件夹分别用来存储`代码库`和`缓存`
* 打开新建的站点，默认登录用户名是`admin`，密码是`gitcandy`
* 转到`设置`页面，分别设置`代码库`，`缓存`和`git-core`的路径
* 推荐在`Web.config`设置`<compilation debug="false" />`

##### *注*
* `代码库`和`缓存`路径示例：`x:\Repos`，`x:\Cache`
* `git-core`路径示例：`x:\PortableGit\libexec\git-core`，`x:\PortableGit\mingw64\libexec\git-core`

---
### 更新日志
跳转到[日志页](http://github.com/Aimeast/GitCandy/blob/dev/CHANGES.md)

---
### 鸣谢 (按字母序)
* [ASP.NET MVC](http://aspnetwebstack.codeplex.com/) @ [Apache License 2.0](http://aspnetwebstack.codeplex.com/license)
* [Bootstrap](http://github.com/twbs/bootstrap) @ [MIT License](http://github.com/twbs/bootstrap/blob/master/LICENSE)
* [Bootstrap-switch](http://github.com/nostalgiaz/bootstrap-switch) @ [Apache License 2.0](http://github.com/nostalgiaz/bootstrap-switch/blob/master/LICENSE)
* [EntityFramework](http://entityframework.codeplex.com/) @ [Apache License 2.0](http://entityframework.codeplex.com/license)
* [FxSsh](http://github.com/Aimeast/FxSsh) @ [MIT license](http://github.com/Aimeast/FxSsh/blob/master/LICENSE.md)
* [Highlight.js](http://github.com/isagalaev/highlight.js) @ [New BSD License](http://github.com/isagalaev/highlight.js/blob/master/LICENSE)
* [jQuery](http://github.com/jquery/jquery) @ [MIT License](http://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt)
* [LibGit2Sharp](http://github.com/libgit2/libgit2sharp) @ [MIT License](http://github.com/libgit2/libgit2sharp/blob/master/LICENSE.md)
* [marked](http://github.com/chjj/marked) @ [MIT License](http://github.com/chjj/marked/blob/master/LICENSE)
* [Microsoft.Composition (MEF2)](http://mef.codeplex.com/) @ [Microsoft Public License](http://mef.codeplex.com/license)
* [Newtonsoft.Json](http://json.codeplex.com/) @ [MIT License](http://json.codeplex.com/license)
* [SharpZipLib](http://github.com/icsharpcode/SharpZipLib) @ [GPL License v2](http://github.com/icsharpcode/SharpZipLib/blob/master/doc/COPYING.txt)

---
### 协议
MIT 协议
