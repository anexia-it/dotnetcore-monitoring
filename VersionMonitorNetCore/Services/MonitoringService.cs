using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Anexia.Monitoring.Models;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace Anexia.Monitoring.Services
{
    /// <summary>
    ///     Provides monitoring methods
    /// </summary>
    internal class MonitoringService : IDisposable
    {
        /// <summary>
        ///     Url for querying the nuget packages
        /// </summary>
        private const string NUGET_URL_PREFIX = "https://api-v2v3search-0.nuget.org/query?q=packageid:";
        private const string NETCORE_RELEASES_INDEX = "https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json";

        /// <summary>
        ///     Client for calling the nuget-API
        /// </summary>
        private HttpClient _client = new HttpClient();

        /// <inheritdoc />
        public void Dispose()
        {
            _client.Dispose();
            _client = null;
        }

        /// <summary>
        ///     Checks if the services are running (database and optional custom services)
        /// </summary>
        /// <returns>'OK' if database and all services are running, 'NOK' otherwise</returns>
        internal string GetServiceStates()
        {
            // check if database is running
            var success = !(VersionMonitor.CheckDatabaseFunction != null && !VersionMonitor.CheckDatabaseFunction());

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

            return success ? "OK" : "NOK";
        }

        /// <summary>
        ///     Gets the runtime and modules info
        /// </summary>
        /// <returns>Task containing struct with current runtime and modules.</returns>
        internal async Task<dynamic> GetModulesInfo()
        {
            return new
            {
                runtime = await GetRuntime(),
                modules = await GetModules()
            };
        }

        #region Runtime helper

        /// <summary>
        ///     Gets runtime info
        /// </summary>
        /// <returns>The current runtime's info.</returns>
        private async Task<RuntimeInfo> GetRuntime()
        {
            // get current framework and version
            var framework = PlatformServices.Default.Application.RuntimeFramework;

            // try to convert to semantic version
            var frameworkVersion = framework.Version.ToString();
            SemanticVersion.TryParse(frameworkVersion, out var semanticVersion);

            if (semanticVersion != null)
            {
                frameworkVersion = semanticVersion.ToString();
            }

            var json = await GetWebContent<ReleaseListJson>(NETCORE_RELEASES_INDEX);
            ReleasesJson latestVersion = null;
            if (json?.Releases?.Any() == true)
            {
                latestVersion = json.Releases.OrderByDescending(x => x.MainVersion).FirstOrDefault(x => x.SupportPhase == "current");
                if (latestVersion == null)
                {
                    latestVersion = json.Releases.OrderByDescending(x => x.MainVersion).FirstOrDefault(x => x.SupportPhase == "lts");
                }
            }

            return new RuntimeInfo
            {
                // fix value, this application is only used for .NET Core
                Platform = "dotnetcore",
                PlatformVersion = frameworkVersion,
                Framework = framework.Identifier,
                FrameworkInstalledVersion = frameworkVersion,
                FrameworkNewestVersion = latestVersion?.MainVersion
            };
        }

        /// <summary>
        ///     Gets info about modules
        /// </summary>
        /// <returns>Task containing list of installed libraries/modules' info.</returns>
        private async Task<List<ModuleInfo>> GetModules()
        {
            var modules = new List<ModuleInfo>();
            var entryAssembly = Assembly.GetEntryAssembly();
            var entryAssemblyName = entryAssembly.GetName().Name.ToLower();
            var libraries = DependencyContext.Load(entryAssembly).RuntimeLibraries.OrderBy(x => x.Name);

            foreach (var library in libraries)
            {
                // no need to display executing assembly
                if (library.Name == entryAssemblyName)
                {
                    continue;
                }

                // try to convert to semantic version
                var assemblyVersion = library.Version;
                SemanticVersion.TryParse(assemblyVersion, out var semanticVersion);

                if (semanticVersion != null)
                {
                    assemblyVersion = semanticVersion.ToString();
                }

                modules.Add(new ModuleInfo
                {
                    Name = library.Name,
                    InstalledVersion = assemblyVersion,
                    NewestVersion = await GetNewestModuleVersion(library.Name, assemblyVersion),

                    // get License
                    Licenses = await GetLicense(library.Name)
                });
            }

            return modules;
        }

        /// <summary>
        ///     Queries nuget with package name to get newest version
        /// </summary>
        /// <param name="libraryName">The name of the library/module.</param>
        /// <param name="installedVersion">The installed version.</param>
        /// <returns>Task containing the most up-to-date version of the library</returns>
        private async Task<string> GetNewestModuleVersion(string libraryName, string installedVersion)
        {
            var nugetJson = await GetWebContent<NugetJson>(NUGET_URL_PREFIX + libraryName);
            if (nugetJson != null && nugetJson.Data.Count > 0)
            {
                var nugetVersions = nugetJson.Data.First().Versions;

                // try to convert to semantic version
                SemanticVersion maxVersion = null;
                foreach (var nugetVersion in nugetVersions)
                {
                    // check if a newer version exists
                    if (SemanticVersion.TryParse(nugetVersion.Version, out var currentVersion))
                    {
                        maxVersion = maxVersion == null || currentVersion > maxVersion ? currentVersion : maxVersion;
                    }
                }

                if (maxVersion != null)
                {
                    return maxVersion.ToString();
                }
            }

            return installedVersion;
        }

        /// <summary>
        ///     Queries nuget with package name to get license
        /// </summary>
        /// <param name="libraryName">The name of the library/module.</param>
        /// <returns>Task containing the library's license.</returns>
        private async Task<List<string>> GetLicense(string libraryName)
        {
            var nugetJson = await GetWebContent<NugetJson>(NUGET_URL_PREFIX + libraryName);
            return nugetJson != null ? nugetJson.Data.Any() ? new List<string> { nugetJson.Data[0].LicenseUrl } : new List<string>() : new List<string>();
        }

        /// <summary>
        ///     Queries a webpage and returns it's content converted to the given format
        /// </summary>
        /// <param name="url">The url to call.</param>
        /// <returns>Task containing the webpage's deserialized content.</returns>
        private async Task<T> GetWebContent<T>(string url)
        {
            var response = await _client.GetAsync(url);

            // status code verification
            response.EnsureSuccessStatusCode();

            // read response content
            var stringResponse = await response.Content.ReadAsStringAsync();

            // convert json to dto
            return JsonConvert.DeserializeObject<T>(stringResponse);
        }

        #endregion

        #region Token validation

        /// <summary>
        ///     Checks if access token has been provided in Initialize-method
        /// </summary>
        /// <returns>true if access token is configured, false otherwise</returns>
        public bool CheckTokenConfiguration()
        {
            return !string.IsNullOrWhiteSpace(VersionMonitor.AccessToken);
        }

        /// <summary>
        ///     Checks if the access token from the query matches the one from the configuration
        /// </summary>
        /// <param name="accessToken">Access token to check against.</param>
        /// <returns>true if token matches with configured one, false otherwise.</returns>
        public bool ValidateToken(string accessToken)
        {
            return VersionMonitor.AccessToken == accessToken;
        }

        #endregion
    }
}