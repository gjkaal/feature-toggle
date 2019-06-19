using FeatureToggle.Web.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureToggle.Web.Controllers
{
    public class NiceControllerBase : ControllerBase {
        private static readonly JsonConverter[] converters = new[] { new StringEnumConverter() };
        private static readonly DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };
        private readonly ILogger _logger;
        private const int JsonSerializationMaxDepth = 10;

        public NiceControllerBase(ILogger logger)
        {
            _logger = logger;
        }

        protected async Task<IActionResult> HandleRequest<TQ>(string methodName, Func<Task<TQ>> action)
        {
            var agent = Request.Headers["User-Agent"];
            try
            {
                using (_logger.BeginScope($"{methodName} Agent:\"{agent}\""))
                {
                    _logger.LogInformation($"Start handling request");

                    var authorization = Request.Headers.FirstOrDefault(q => q.Key == "Authorization").Value.FirstOrDefault();
                    if (string.IsNullOrEmpty(authorization))
                    {
                        _logger.LogWarning($"Authorization is missing");
                        return ResponseHelper.AuthorizationNotAccepted("Expect authorization");
                    }
                    var accessToken = authorization.Split(' ');
                    if (accessToken.Length != 2 || string.IsNullOrEmpty(accessToken[1]))
                    {
                        _logger.LogWarning($"Access token is invalid");
                        return ResponseHelper.AuthorizationNotAccepted("Access token is not valid");
                    }

                    // TODO : Authorize
                    //var currentUser = accessToken[1].GetPrincipalFromJwt(Convert.FromBase64String(_internalApiServerConfig.JwtSecret));
                    //if (!currentUser.Identity.IsAuthenticated)
                    //{
                    //    _logger.LogWarning($"ClaimsPrincipal is not authenticated");
                    //}
                    //else
                    //{
                    //    Thread.CurrentPrincipal = currentUser;
                    //}

                    var userName = "anonymous";
                    _logger.LogInformation($"Using Claimsprincipal for {userName}");
                    using (_logger.BeginScope(userName))
                    {
                        TQ result;
                        using (_logger.BeginScope(action))
                        {
                            _logger.LogInformation($"Invoking: {methodName}");
                            result = await action();
                            _logger.LogInformation($"Invoke {methodName} complete");
                            if (result == null)
                            {
                                _logger.LogWarning($"Empty result from: {methodName}");
                            }
                        }

                        return new ContentResult
                        {
                            StatusCode = StatusCodes.Status200OK,
                            ContentType = "application/json",
                            // TODO: Lowercase json controller
                            Content = JsonConvert.SerializeObject(result, SerializerSettings)
                        };
                    }
                }
            }
            catch (FeatureServices.Exceptions.FeatureToggleException ce)
            {
                _logger.LogError("HandleRequest failed", ce);
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    ContentType = "application/json",
                    Content = JsonConvert.SerializeObject(
                        new ClientErrorData
                        {
                            //Link = HelpUrl,
                            Title = ce.Message
                        }, SerializerSettings)
                };
            }
            catch (Exception e)
            {
                _logger.LogError("HandleRequest failed", e);
                var clientError = new ClientErrorData
                {
                    //Link = HelpUrl,
                    Title = e.Message
                };
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentType = "application/json",
                    Content = JsonConvert.SerializeObject(clientError, SerializerSettings)
                };
            }
        }

        private static JsonSerializerSettings SerializerSettings =>
             new JsonSerializerSettings
             {
                 ContractResolver = contractResolver,
                 Formatting = Formatting.Indented,
                 Converters = converters,
                 MaxDepth = JsonSerializationMaxDepth,
                 NullValueHandling = NullValueHandling.Ignore
             };
    }
}
