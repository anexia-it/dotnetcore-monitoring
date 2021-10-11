using System.Threading.Tasks;
using Anexia.Monitoring.Attribute;
using Anexia.Monitoring.Services;
using Microsoft.AspNetCore.Mvc;

namespace Anexia.Monitoring.Controllers
{
    /// <summary>
    ///     APIs for version monitoring
    /// </summary>
    public class MonitoringController : Controller
    {
        // error message for missing authentication token
        private const string TOKEN_ERROR_MESSAGE = "Access token not configured";
        private readonly MonitoringService _service;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MonitoringController"/> class.
        /// </summary>
        public MonitoringController()
        {
            _service = new MonitoringService();
        }

        /// <summary>
        ///     Get info about state of services (database etc.)
        /// </summary>
        /// <param name="access_token">
        ///     the token to allow access to the monitoring routes - must be send as query-param with each
        ///     api-call
        /// </param>
        /// <returns>plain text with state infos</returns>
        [HttpGet]
        [Produces("text/plain")]
        [AllowCrossOrigin]
        public dynamic GetServiceStates([FromQuery] string access_token)
        {
            var result = CheckAccessToken(access_token);
            return result != null ? result : new OkObjectResult(_service.GetServiceStates());
        }

        /// <summary>
        ///     Get version-info about runtime and modules
        /// </summary>
        /// <param name="access_token">
        ///     the token to allow access to the monitoring routes - must be send as query-param with each
        ///     api-call
        /// </param>
        /// <returns>json object with runtime and modules infos</returns>
        [HttpGet]
        [Produces("application/json")]
        [AllowCrossOrigin]
        public async Task<dynamic> GetModulesInfo([FromQuery] string access_token)
        {
            var result = CheckAccessToken(access_token);
            if (result != null)
            {
                return result;
            }

            var info = await _service.GetModulesInfo();
            return new OkObjectResult(info);
        }

        /// <summary>
        ///     Check the access token
        /// </summary>
        /// <param name="token">the token to allow access to the monitoring routes - must be send as query-param with each api-call.</param>
        /// <returns>null if authorized, <see cref="UnauthorizedResult"/> otherwise.</returns>
        private dynamic CheckAccessToken(string token)
        {
            if (!_service.CheckTokenConfiguration())
            {
                return new UnauthorizedResult();
            }

            if (!_service.ValidateToken(token))
            {
                return new UnauthorizedResult();
            }

            return null;
        }
    }
}