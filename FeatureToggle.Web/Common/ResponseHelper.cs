using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Threading.Tasks;

namespace FeatureToggle.Web.Common
{
    public static class ResponseHelper
    {

        private static readonly StringEnumConverter enumConverter = new StringEnumConverter();

        public static ContentResult AuthorizationNotAccepted(string message)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(
                   new ClientErrorData
                   {
                       Title = message
                   }, enumConverter)
            };
        }

        public static ContentResult TokenNotAcceptedResponse(IUrlHelper url)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status406NotAcceptable,
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(
                    new ClientErrorData
                    {
                        Link = url.Action("SasToken", "Help"),
                        Title = "Token not accepted"
                    }, enumConverter)
            };
        }

        public static ContentResult SigningKeyNotValidResponse(IUrlHelper url, string signingKeyName)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(
                                    new ClientErrorData
                                    {
                                        Link = url.Action("SasToken", "Help"),
                                        Title = $"Signing key is not valid:{signingKeyName}"
                                    }, enumConverter)
            };
        }

        public static ContentResult TokenInvalidResponse(IUrlHelper url)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(
                                    new ClientErrorData
                                    {
                                        Link = url.Action("SasToken", "Help"),
                                        Title = "Invalid token"
                                    }, enumConverter)
            };
        }

        public static ContentResult InternalServerError(string HelpUrl, Exception e)
        {
            var clientError = new ClientErrorData
            {
                Link = HelpUrl,
                Title = e.Message
            };

            return new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(clientError, enumConverter)
            };
        }

        public static async Task<ContentResult> OkResult<TResponseType>(Func<Task<TResponseType>> action)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(await action(), enumConverter)
            };
        }

    }
}
