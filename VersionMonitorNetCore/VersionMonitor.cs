using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Reflection;
using Anexia.Monitoring.Models;

namespace Anexia.Monitoring
{
    /// <summary>
    /// Startpoint for initializing the version monitoring:
    /// 1) set access token (-> SetAccessToken function)
    /// 2) register the needed monitoring routes before adding the default MVC routes (RegisterServiceStateMonitor and/or RegisterModulesInfoMonitor function) 
    /// </summary>
    public static class VersionMonitor
    {
        /// <summary>
        /// the major version number of this assembly - needed for creating the monitoring-routes
        /// </summary>
        private static string _assemblyVersion = null;

        /// <summary>
        /// token for accessing the apis
        /// </summary>
        internal static string AccessToken { get; private set; }
        /// <summary>
        /// function to check if the database is running - moved logic to client to keep version monitor platform independent
        /// </summary>
        internal static Func<bool> CheckDatabaseFunction { get; set; }
        /// <summary>
        /// function to check if custom services are running
        /// </summary>
        internal static Func<List<ServiceState>> CheckCustomServicesFunction { get; set; }

        /// <summary>
        /// Set the access token for the monitoring APIs
        /// </summary>
        /// <param name="accessToken">the token to allow access to the monitoring routes - must be send as query-param with each api-call</param>
        public static void SetAccessToken(string accessToken)
        {
            AccessToken = accessToken;
        }

        /// <summary>
        /// Register route to call service state monitor - make sure this is called before adding mvc default routing
        /// route: "/anxapi/v[VERSIONMONITOR-VERSION]/up?access_token=[TOKEN]"
        /// </summary>
        /// <param name="app">IApplicationBuilder to map routes</param>
        /// <param name="checkDatabaseFunction">function to check if database is running</param>
        /// <param name="checkCustomServicesFunction">function to check if custom services are running</param>
        public static void RegisterServiceStateMonitor(IApplicationBuilder app, Func<bool> checkDatabaseFunction = null, Func<List<ServiceState>> checkCustomServicesFunction = null)
        {
            CheckCustomServicesFunction = checkCustomServicesFunction;
            CheckDatabaseFunction = checkDatabaseFunction;

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "anxservice", //route name
                     ("anxapi/v" + GetVersionNumber() + "/up"), // template
                     new { controller = "Monitoring", action = "GetServiceStates" }, // defaults
                     null, // constraints
                     new[] { "VersionMonitorNetCore.Controllers" }); //datatoken
            });
        }

        /// <summary>
        /// Register route to call runtime & modules monitor - make sure this is called before adding mvc default routing
        /// route: "/anxapi/v[VERSIONMONITOR-VERSION]/modules?access_token=[TOKEN]"
        /// </summary>
        /// <param name="app">IApplicationBuilder to map routes</param>
        public static void RegisterModulesInfoMonitor(IApplicationBuilder app)
        {
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                     "anxruntime", //route name
                     ("anxapi/v" + GetVersionNumber() + "/modules"), // template
                     new { controller = "Monitoring", action = "GetModulesInfo" }, // defaults
                     null, // constraints
                     new[] { "VersionMonitorNetCore.Controllers" }); //datatoken
            });
        }

        private static string GetVersionNumber()
        {
            if (String.IsNullOrWhiteSpace(_assemblyVersion))
            {
                var version = typeof(Anexia.Monitoring.VersionMonitor).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                if (String.IsNullOrWhiteSpace(version))
                    throw new Exception("Failed to get version number for VersionMonitorNetCore assembly!");

                // only major version number is needed
                _assemblyVersion = version.Substring(0, version.IndexOf("."));
            }

            return _assemblyVersion;
        }
    }
}
