using Newtonsoft.Json;
using System.Collections.Generic;

namespace Anexia.Monitoring.Models
{
    /// <summary>
    /// Dto for the module info
    /// </summary>
    [JsonObject(Title = "modules")]
    public class ModuleInfo
    {
        /// <summary>
        /// Module name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        /// <summary>
        /// Installed version of the module
        /// </summary>
        [JsonProperty(PropertyName = "installed_version")]
        public string InstalledVersion { get; set; }
        /// <summary>
        /// Newest version of the module
        /// </summary>
        [JsonProperty(PropertyName = "newest_version")]
        public string NewestVersion { get; set; }
        /// <summary>
        /// List of Licenses
        /// </summary>
        [JsonProperty(PropertyName = "licence_urls")]
        public List<string> Licenses { get; set; }
    }
}
