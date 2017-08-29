using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersionMonitorNetCore.Models;


namespace VersionMonitorNetCore.Services
{
    /// <summary>
    /// Provides monitoring methods
    /// </summary>
    internal class MonitoringService : IDisposable
    {
        /// <summary>
        /// Url for querying the nuget packages
        /// </summary>
        private const string NUGET_URL_PREFIX = "https://api-v2v3search-0.nuget.org/query?q=packageid:";
        /// <summary>
        /// Client for calling the nuget-API
        /// </summary>
        private HttpClient _client = new HttpClient();

        /// <summary>
        /// Checks if the services are running (database and optional custom services)
        /// </summary>
        /// <returns></returns>
        internal string GetServiceStates()
        {
            bool success = true;

            // check if database is running
            if (VersionMonitor.CheckDatabaseFunction != null && !VersionMonitor.CheckDatabaseFunction())
                success = false;

            // check if custom services are running
            if (VersionMonitor.CheckCustomServicesFunction != null)
            {
                foreach (var result in VersionMonitor.CheckCustomServicesFunction())
                {
                    if (!result.IsRunning)
                    {
                        success = false;
                        break;
                    }
                }
            }

            return (success ? "OK" : "NOK");
        }

        /// <summary>
        /// Gets the runtime and modules info
        /// </summary>
        /// <returns></returns>
        internal async Task<dynamic> GetModulesInfo()
        {
            return new
            {
                runtime = GetRuntime(),
                modules = await GetModules()
            };
        }

        #region Runtime helper

        /// <summary>
        /// get runtime info
        /// </summary>
        /// <returns></returns>
        private RuntimeInfo GetRuntime()
        {
            // get current framework and version
            var framework = PlatformServices.Default.Application.RuntimeFramework;

            // try to convert to semantic version
            string frameworkVersion = framework.Version.ToString();
            SemanticVersion semanticVersion;
            SemanticVersion.TryParse(frameworkVersion, out semanticVersion);

            if (semanticVersion != null)
                frameworkVersion = semanticVersion.ToString();

            return new RuntimeInfo
            {
                // fix value, this application is only used for .NET Core
                Platform = "dotnetcore",
                PlatformVersion = frameworkVersion,
                Framework = framework.Identifier,
                FrameworkInstalledVersion = frameworkVersion,
                FrameworkNewestVersion = frameworkVersion //todo: newest framework version
            };
        }

        /// <summary>
        /// get info about modules
        /// </summary>
        /// <returns></returns>
        private async Task<List<ModuleInfo>> GetModules()
        {
            List<ModuleInfo> modules = new List<ModuleInfo>();
            var entryAssembly = Assembly.GetEntryAssembly();
            string entryAssemblyName = entryAssembly.GetName().Name.ToLower();
            var libraries = DependencyContext.Load(entryAssembly).RuntimeLibraries.OrderBy(x => x.Name);

            foreach (var library in libraries)
            {
                // no need to display executing assembly
                if (library.Name == entryAssemblyName)
                    continue;

                // try to convert to semantic version
                string assemblyVersion = library.Version.ToString();
                SemanticVersion semanticVersion;
                SemanticVersion.TryParse(assemblyVersion, out semanticVersion);

                if (semanticVersion != null)
                    assemblyVersion = semanticVersion.ToString();

                modules.Add(new ModuleInfo()
                {
                    Name = library.Name,
                    InstalledVersion = assemblyVersion,
                    NewestVersion = await GetNewestModuleVersion(library.Name, assemblyVersion)
                });
            }

            return modules;
        }

        /// <summary>
        /// Query nuget with package name to get newest version
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="installedVersion"></param>
        /// <returns></returns>
        private async Task<string> GetNewestModuleVersion(string libraryName, string installedVersion)
        {
            var response = await _client.GetAsync(NUGET_URL_PREFIX + libraryName);
            // status code verification
            response.EnsureSuccessStatusCode();
            // read response content
            string stringResponse = await response.Content.ReadAsStringAsync();

            // convert json to dto
            var nugetJson = JsonConvert.DeserializeObject<NugetJson>(stringResponse);
            if (nugetJson != null && nugetJson.data.Count > 0)
            {
                var nugetVersions = nugetJson.data.First().Versions;
                // try to convert to semantic version
                SemanticVersion maxVersion = null;
                foreach (var nugetVersion in nugetVersions)
                {
                    // check if a newer version exists
                    SemanticVersion currentVersion;
                    if (SemanticVersion.TryParse(nugetVersion.version, out currentVersion))
                        maxVersion = (maxVersion == null || currentVersion > maxVersion) ? currentVersion : maxVersion;
                }
                if (maxVersion != null)
                    return maxVersion.ToString();
            }

            return installedVersion;
        }

        #endregion

        #region Token validation

        /// <summary>
        /// Check if access token has been provided in Initialize-method
        /// </summary>
        /// <returns></returns>
        public bool CheckTokenConfiguration()
        {
            return !String.IsNullOrWhiteSpace(VersionMonitor.AccessToken);
        }

        /// <summary>
        /// Check if the access token from the query matches the one from the configuration
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public bool ValidateToken(string accessToken)
        {
            return VersionMonitor.AccessToken == accessToken;
        }

        #endregion

        public void Dispose()
        {
            _client.Dispose();
            _client = null;
        }
    }
}
