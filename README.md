# Anexia Monitoring

Package to monitor dependency and framework versions for .NET Core Frameworks. It can be also used to check if the website is alive and working correctly.

## Installation and configuration

Install the package via NuGet: "VersionMonitorNetCore"

Set Access Token and register monitoring routes before adding the default routes in Startup.cs:

		...        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Anexia.Monitoring.VersionMonitor.SetAccessToken("custom_access_token");
            Anexia.Monitoring.VersionMonitor.RegisterServiceStateMonitor(app, () => true);
            Anexia.Monitoring.VersionMonitor.RegisterModulesInfoMonitor(app);
            ...
        }
		...

You can configure blacklist-modules (by regular expressions) wich will be excluded in result-list.
By default there are three blacklist-regex-configurations done:
- ^[App_Web]
- ^[CompiledRazorTemplates]
- ^[System.]

You can override the default-blacklist by

		...        
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ...
            Anexia.Monitoring.VersionMonitor.SetBlackList(new List<string>(){ "your_regex" });
            ...
        }
		...

Also you can extend the existing blacklist by

		...        
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ...
            Anexia.Monitoring.VersionMonitor.SetAdditionalBlackList(new List<string>(){ "your_regex" });
            ...
        }
		...

## Usage

The package registers some custom REST endpoints which can be used for monitoring. Make sure that the `custom_access_token` is defined, since this is used for authorization.

#### Version monitoring of core and composer packages

Returns all a list with dependency and framework version information.

**URL:** `/anxapi/v1/modules?access_token=custom_access_token`

Response headers:

	Status Code: 200 OK
	Access-Control-Allow-Credentials: true
	Access-Control-Allow-Methods: GET, OPTIONS
	Access-Control-Allow-Origin: *
	Content-Type: application/json; charset=utf-8

Response body:

	"runtime":{
		"platform":"dotnetcore",
		"platform_version":"1.1",
		"framework":".NETCoreApp",
		"framework_installed_version":"1.1",
		"framework_newest_version":"1.1"
	},
	"modules":[{
			"name": "Libuv",
			"installed_version": "1.9.1",
			"newest_version": "1.10.0",
			"licence_urls": ["https://raw.githubusercontent.com/aspnet/libuv-build/775a18ca77368a0f4ca753e82b2369f24707fb3e/build/License.txt"]
		},
		{
			"name": "Microsoft.ApplicationInsights",
			"installed_version": "2.2.0",
			"newest_version": "2.8.1",
			"licence_urls": ["https://go.microsoft.com/fwlink/?LinkID=510709"]
		},
		...
	]}

#### Live monitoring

This endpoint can be used to verify if the application is alive and working correctly.

**URL:** `/anxapi/v1/up?access_token=custom_access_token`

Response headers:

	Status Code: 200 OK
	Access-Control-Allow-Credentials: true
	Access-Control-Allow-Methods: GET, OPTIONS
	Access-Control-Allow-Origin: *
	Content-Type: text/plain; charset=utf-8

Response body:

	OK

## List of developers

* Susanne Meier <SMeier@anexia-it.com>
* Joachim Eckerl <JEckerl@anexia-it.com>

## Project related external resources

* [.NET Core Documentation](https://docs.microsoft.com/en-us/dotnet/core/index)
