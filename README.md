# RouteHandlerHttpModule

*Find out what ASP.NET MVC controller or Web Forms page handled a given route*

Have you ever worked on an application where you could hit a particular page,
but you didn't always know where to find the code that handled that page?

What if every response included a header showing what MVC controller handled
the request?

![Screenshot of X-Route-Handler header](https://cloud.githubusercontent.com/assets/979677/12868716/33ff7898-ccda-11e5-9abe-081ce62c50ba.PNG)

Or what Web Forms page handled the request?

![Screenshot of X-Route-Handler header](https://cloud.githubusercontent.com/assets/979677/12868717/3ee9153e-ccda-11e5-824f-0b8623be4c35.png)

That's the whole idea behind RouteHandlerHttpModule.

## Getting Started

Unlike many other HTTP Modules, RouteHandlerHttpModule is __not intended to be
installed as a dependency into your project__.  You probably don't want
visitors hitting your website to see class and file names from the code, right?
Instead, RouteHandlerHttpModule is __meant to be installed as a global HTTP
module on the developer's machine__.

Don't worry, the steps are easy:

1. [Downloaded the latest
   release](https://github.com/mkropat/RouteHandlerHttpModule/releases/latest)
   (you want `RouteHandlerHttpModule.zip`)
1. Extract the files somewhere
1. Open PowerShell as an Administrator
1. Run `.\Install-RouteHandlerModule.ps1` (from the directory where you
   extracted the files)

The installer script does three things:

1. Installs `RouteHandlerHttpModule.dll` into the GAC
1. Updates the IIS `applicationHost.config` to load RouteHandlerHttpModule
1. Updates (for the current user) the IIS Express `applicationHost.config` to
   load RouteHandlerHttpModule

You may need to restart IIS or IIS Express after you run the install script for
the change to take effect.

### Additional Instructions For Visual Studio 2015 + IIS Express

Starting in Visual Studio 2015, by default projects reference a per-solution
`applicationHost.config` (stored in the `.vs/config/` folder), instead of
referencing the user-wide `applicationHosting.config`.  To enable
RouteHandlerHttpModule in IIS Express, you have two options:

1. [Update the project file to point to the "global"
   `applicationHost.config`.](http://stackoverflow.com/questions/31574063/visual-studio-2015-change-template-for-new-applicationhost-config)
   The config setting needs to look like this:

  ```xml
  <UseGlobalApplicationHostFile>True</UseGlobalApplicationHostFile>
  ```

1. Update the `<modules></modules>` section in the project's
   `.vs/config/applicationHost.config` to add a reference to
   the RouteHandlerHttpModule assembly installed in the GAC

   - Search for "RouteHandlerHttpModule" in
     `Documents\IISExpress\config\applicationHost.config` to see an example of
     what the reference needs to look like

