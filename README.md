## GitCandy
GitCandy© is a [Git](http://git-scm.com/documentation) distributed version control platform based on ASP.NET MVC application, which supports public and private repositories. You can create and collaborate your repository with your team anytime anywhere without any limit.

Visit a demo on [http://gitcandy.com](http://gitcandy.com).

Get source and fork me on [http://github.com/Aimeast/GitCandy](http://github.com/Aimeast/GitCandy).

---
### Prerequisites
* [IIS 7.0](http://www.iis.net/learn)
* [.NET Framework 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
* [ASP.NET MVC 5](http://www.asp.net/mvc/tutorials/mvc-5)
* [Git](http://git-for-windows.github.io/)
* [Sqlite](http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki) or [Sql Server](http://www.microsoft.com/en-us/sqlserver/get-sql-server/try-it.aspx)

---
### Installation
* Download last [release](http://github.com/Aimeast/GitCandy/releases) or build [dev](http://github.com/Aimeast/GitCandy/dev) branch by yourself
* Create a web site on IIS, copy binary and resource files to site path
* Copy `GitCandy\bin\[NativeBinaries & x86 & x64]` folders to destination if you are publishing the website
* Create a database by `/Sql/Create.[Sqlite | MsSql].sql`, copy database file to `App_Data` folder if any
* Update connection string in `Web.config` file
* Prepare two folders for storage `Repositories` and `Cache`
* Navigate to your site and login with default username `admin`, password `gitcandy`
* Go to `Settings` page for set folders path of `Repositories`, `Cache` and `git-core`
* You are recommended to set `<compilation debug="false" />` in `Web.config`

##### *note*
* The `Repositories` or `Cache` path looks like `x:\Repos` or `x:\Cache`
* The `git-core` path looks like `x:\PortableGit\libexec\git-core` or `x:\PortableGit\mingw64\libexec\git-core`

---
### Thanks for (alphabet)
* [ASP.NET MVC](http://aspnetwebstack.codeplex.com/) @ [Apache License 2.0](http://aspnetwebstack.codeplex.com/license)
* [Bootstrap](http://github.com/twbs/bootstrap) @ [MIT License](http://github.com/twbs/bootstrap/blob/master/LICENSE)
* [Bootstrap-switch](http://github.com/nostalgiaz/bootstrap-switch) @ [Apache License 2.0](http://github.com/nostalgiaz/bootstrap-switch/blob/master/LICENSE)
* [EntityFramework](http://entityframework.codeplex.com/) @ [Apache License 2.0](http://entityframework.codeplex.com/license)
* [Highlight.js](http://github.com/isagalaev/highlight.js) @ [New BSD License](http://github.com/isagalaev/highlight.js/blob/master/LICENSE)
* [jQuery](http://github.com/jquery/jquery) @ [MIT License](http://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt)
* [LibGit2Sharp](http://github.com/libgit2/libgit2sharp) @ [MIT License](http://github.com/libgit2/libgit2sharp/blob/master/LICENSE.md)
* [marked](http://github.com/chjj/marked) @ [MIT License](http://github.com/chjj/marked/blob/master/LICENSE)
* [Microsoft.Composition (MEF2)](http://mef.codeplex.com/) @ [Microsoft Public License](http://mef.codeplex.com/license)
* [Newtonsoft.Json](http://json.codeplex.com/) @ [MIT License](http://json.codeplex.com/license)
* [SharpZipLib](http://github.com/icsharpcode/SharpZipLib) @ [GPL License v2](http://github.com/icsharpcode/SharpZipLib/blob/master/doc/COPYING.txt)
* [ZeroClipboard](http://github.com/zeroclipboard/zeroclipboard) @ [MIT License](http://github.com/zeroclipboard/zeroclipboard/blob/master/LICENSE)

---
### License
The MIT license
