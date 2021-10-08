using Microsoft.AspNetCore.Mvc.Filters;

namespace Anexia.Monitoring.Attribute
{
    /// <summary>
    ///     Attribute for adding CORS headers
    /// </summary>
    public class AllowCrossOriginAttribute : ActionFilterAttribute
    {
        /// <summary>
        ///     <inheritdoc/>
        ///     Adds needed headers for allowing cross origin requests.
        /// </summary>
        /// <param name="context">The current action executing context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Add Response Header-Elements
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");

            base.OnActionExecuting(context);
        }
    }
}