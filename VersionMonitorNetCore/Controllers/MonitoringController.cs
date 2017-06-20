using Microsoft.AspNetCore.Mvc;
using VersionMonitorNetCore.Services;

namespace VersionMonitorNetCore.Controllers
{
    /// <summary>
    /// APIs for version monitoring
    /// </summary>
    public class MonitoringController : Controller
    {
        private const string TOKEN_ERROR_MESSAGE = "Access token not configured";
        private readonly MonitoringService _service;

        public MonitoringController()
        {
            _service = new MonitoringService();
        }

        /// <summary>
        /// Get info about state of services (database etc.)
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns>plain text with state infos</returns>
        [HttpGet]
        [Produces("text/plain")]
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
        /// <param name="access_token"></param>
        /// <returns>json object with runtime and modules infos</returns>
        [HttpGet]
        [Produces("application/json")]
        public dynamic GetModulesInfo([FromQuery] string access_token)
        {
            var result = CheckAccessToken(access_token);
            if (result != null)
                return result;

            var info = _service.GetModulesInfo();
            return new OkObjectResult(info);
        }


        /// <summary>
        /// Check the access token
        /// </summary>
        /// <param name="token"></param>
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