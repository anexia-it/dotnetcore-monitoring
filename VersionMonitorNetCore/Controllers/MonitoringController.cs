using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VersionMonitorNetCore.Attribute;
using VersionMonitorNetCore.Services;

namespace VersionMonitorNetCore.Controllers
{
    /// <summary>
    /// APIs for version monitoring
    /// </summary>
    public class MonitoringController : Controller
    {
        // error message for missing authentication token
        private const string TOKEN_ERROR_MESSAGE = "Access token not configured";
        private readonly MonitoringService _service;

        public MonitoringController()
        {
            _service = new MonitoringService();
        }

        /// <summary>
        /// Get info about state of services (database etc.)
        /// </summary>
        /// <param name="access_token">the token to allow access to the monitoring routes - must be send as query-param with each api-call</param>
        /// <returns>plain text with state infos</returns>
        [HttpGet]
        [Produces("text/plain")]
        [AllowCrossOrigin]
        public dynamic GetServiceStates([FromQuery] string access_token)
        {
            var result = CheckAccessToken(access_token);
            if (result != null)
                return result;

            return new OkObjectResult(_service.GetServiceStates());
        }

        /// <summary>
        /// Get version-info about runtime and modules
        /// </summary>
        /// <param name="access_token">the token to allow access to the monitoring routes - must be send as query-param with each api-call</param>
        /// <returns>json object with runtime and modules infos</returns>
        [HttpGet]
        [Produces("application/json")]
        [AllowCrossOrigin]
        public async Task<dynamic> GetModulesInfo([FromQuery] string access_token)
        {
            var result = CheckAccessToken(access_token);
            if (result != null)
                return result;

            var info = await _service.GetModulesInfo();
            return new OkObjectResult(info);
        }


        /// <summary>
        /// Check the access token
        /// </summary>
        /// <param name="token">the token to allow access to the monitoring routes - must be send as query-param with each api-call</param>
        /// <returns></returns>
        private dynamic CheckAccessToken(string token)
        {
            if (!_service.CheckTokenConfiguration())
                return new UnauthorizedResult();

            if (!_service.ValidateToken(token))
                return new UnauthorizedResult();

            return null;
        }
    }
}