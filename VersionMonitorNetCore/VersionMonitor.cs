using System;
using System.Collections.Generic;
using System.Reflection;
using Anexia.Monitoring.Models;
using Microsoft.AspNetCore.Builder;

namespace Anexia.Monitoring
{
    /// <summary>
    ///     Start point for initializing the version monitoring:
    ///     1) set access token (-> SetAccessToken function)
    ///     2) register the needed monitoring routes before adding the default MVC routes (RegisterServiceStateMonitor and/or
    ///     RegisterModulesInfoMonitor function)
    /// </summary>
    public static class VersionMonitor
    {
        /// <summary>
        ///     the major version number of this assembly - needed for creating the monitoring-routes
        /// </summary>
        private static string _assemblyVersion;

        /// <summary>
        ///     Gets token for accessing the apis
        /// </summary>
        internal static string AccessToken { get; private set; }

        /// <summary>
        ///     Gets or sets function to check if the database is running - moved logic to client to keep version monitor platform independent
        /// </summary>
        internal static Func<bool> CheckDatabaseFunction { get; set; }

        /// <summary>
        ///     Gets or sets function to check if custom services are running
        /// </summary>
        internal static Func<List<ServiceState>> CheckCustomServicesFunction { get; set; }

        /// <summary>
        ///     Set the access token for the monitoring APIs
        /// </summary>
        /// <param name="accessToken">
        ///     The token to allow access to the monitoring routes - must be send as query-param with each
        ///     api-call
        /// </param>
        public static void SetAccessToken(string accessToken)
        {
            AccessToken = accessToken;
        }

        /// <summary>
        /// Set the blacklistfor the monitoring APIs
        /// </summary>
        /// <param name="blackList">the list of module-prefixes not allowed to load in list</param>
        public static void SetBlackList(List<string> blackList)
        {
            BlackList = blackList ?? new List<string>();
        }

        /// <summary>
        /// Set the additionalBlackList the monitoring APIs
        /// </summary>
        /// <param name="additionalBlackList">the list of module-prefixes on Top not allowed to load in list</param>
        public static void SetAdditionalBlackList(List<string> additionalBlackList)
        {
            AdditionalBlackList = additionalBlackList ?? new List<string>();
        }

        /// <summary>
        /// blacklist for modules starts with not should been loaded
        /// </summary>
        internal static List<string> AdditionalBlackList { get; private set; } =
            new List<string>();

        /// <summary>
        /// blacklist for modules starts with not should been loaded
        /// </summary>
        internal static List<string> BlackList { get; private set; } = 
            new List<string>(){
                "^[App_Web]",
                "^[CompiledRazorTemplates]",
                "^[System.]"
            };

        /// <summary>
        ///     Register route to call service state monitor - make sure this is called before adding mvc default routing
        ///     route: "/anxapi/v[VERSIONMONITOR-VERSION]/up?access_token=[TOKEN]"
        /// </summary>
        /// <param name="app">IApplicationBuilder to map routes</param>
        /// <param name="checkDatabaseFunction">function to check if database is running.</param>
        /// <param name="checkCustomServicesFunction">function to check if custom services are running.</param>
        public static void RegisterServiceStateMonitor(IApplicationBuilder app, Func<bool> checkDatabaseFunction = null, Func<List<ServiceState>> checkCustomServicesFunction = null)
        {
            CheckCustomServicesFunction = checkCustomServicesFunction;
            CheckDatabaseFunction = checkDatabaseFunction;

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "anxservice",
                   pattern: $"anxapi/v{GetVersionNumber()}/up",
                   defaults: new { controller = "Monitoring", action = "GetServiceStates" },
                   dataTokens: new[] { "VersionMonitorNetCore.Controllers" }
                  );
            });
        }

        /// <summary>
        ///     Register route to call runtime & modules monitor - make sure this is called before adding mvc default routing
        ///     route: "/anxapi/v[VERSIONMONITOR-VERSION]/modules?access_token=[TOKEN]"
        /// </summary>
        /// <param name="app">IApplicationBuilder to map routes.</param>
        public static void RegisterModulesInfoMonitor(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                         name: "anxruntime",
                         pattern: $"anxapi/v{GetVersionNumber()}/modules",
                         defaults: new { controller = "Monitoring", action = "GetModulesInfo" },
                         dataTokens: new[] { "VersionMonitorNetCore.Controllers" }
                        );
                });
        }

        /// <summary>
        ///     Get version number via reflection
        /// </summary>
        private static string GetVersionNumber()
        {
            if (!string.IsNullOrWhiteSpace(_assemblyVersion))
            {
                return _assemblyVersion;
            }

            var version = typeof(VersionMonitor).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            if (string.IsNullOrWhiteSpace(version))
            {
                throw new Exception("Failed to get version number for VersionMonitorNetCore assembly!");
            }

            // only major version number is needed
            _assemblyVersion = version.Substring(0, version.IndexOf("."));

            return _assemblyVersion;
        }
    }
}