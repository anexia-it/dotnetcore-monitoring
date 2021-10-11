using System.Collections.Generic;
using Newtonsoft.Json;

namespace Anexia.Monitoring.Models
{
    /// <summary>
    ///     Dto for the json object returned by netcore release page
    /// </summary>
    internal class ReleaseListJson
    {
        /// <summary>
        ///     Gets or sets the data of the releases
        /// </summary>
        [JsonProperty(PropertyName = "releases-index")]
        public List<ReleasesJson> Releases { get; set; }
    }
}