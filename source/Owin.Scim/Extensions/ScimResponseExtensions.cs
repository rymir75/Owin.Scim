using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Owin.Scim.Extensions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    using Model;
    
    /// <summary>
    /// Defines extension methods for <see cref="IScimResponse{T}"/>.
    /// </summary>
    public static class ScimResponseExtensions
    {
        public static IScimResponse<T2> Bind<T, T2>(this IScimResponse<T> scimResponse, Func<T, IScimResponse<T2>> bindingFunction)
        {
            if (scimResponse.IsLeft)
            {
                return CreateGenericErrorResponse<T, T2>(scimResponse, scimResponse.GetLeft());
            }

            return bindingFunction.Invoke(scimResponse.GetRight());
        }

        public static Task<IScimResponse<TRight2>> BindAsync<TRight, TRight2>(
            this IScimResponse<TRight> scimResponse,
            Func<TRight, Task<IScimResponse<TRight2>>> bindFunc)
        {
            if (scimResponse.IsLeft)
            {
                var tcs = new TaskCompletionSource<IScimResponse<TRight2>>();
                tcs.SetResult(new ScimErrorResponse<TRight2>(scimResponse.GetLeft()));

                return tcs.Task;
            }

            return bindFunc(scimResponse.GetRight());
        }

        public static IScimResponse<TRight> Let<TRight>(this IScimResponse<TRight> scimResponse, Action<TRight> action)
        {
            if (scimResponse.IsRight)
            {
                action.Invoke(scimResponse.GetRight());
            }

            return scimResponse;
        }

        internal static IScimResponse<T2> CreateGenericErrorResponse<T, T2>(IScimResponse<T> originalResponse, ScimError error)
        {
            if (IsBuiltInErrorResponse(originalResponse))
            {
                return new ScimErrorResponse<T2>(error);
            }

            try
            {
                return Activator.CreateInstance(
                    originalResponse.GetType()
                                    .GetGenericTypeDefinition()
                                    .MakeGenericType(typeof(T2)),
                    error) as IScimResponse<T2>;
            }
            catch (TargetInvocationException)
            {
                // No supportable constructor found! Return default.
                return new ScimErrorResponse<T2>(error);
            }
        }

        private static bool IsBuiltInDataResponse<T>(IScimResponse<T> originalResponse)
        {
            var typeInfo = originalResponse.GetType().GetTypeInfo();
            return (originalResponse is ScimDataResponse<T>) ||
                   (typeInfo.IsGenericType && typeof(ScimDataResponse<>).GetTypeInfo().IsAssignableFrom(typeInfo.GetGenericTypeDefinition().GetTypeInfo()));
        }

        private static bool IsBuiltInErrorResponse<T>(IScimResponse<T> originalResponse)
        {
            var typeInfo = originalResponse.GetType().GetTypeInfo();
            return (originalResponse is ScimErrorResponse<T>) ||
                   (typeInfo.IsGenericType && typeof(ScimErrorResponse<>).GetTypeInfo().IsAssignableFrom(typeInfo.GetGenericTypeDefinition().GetTypeInfo()));
        }

        /// <summary>
        /// Returns a new <see cref="HttpResponseMessage"/> with the <see cref="HttpResponseMessage.Content"/> set to <paramref name="scimResponse"/>. If 
        /// <paramref name="scimResponse"/> contains an error, it will attempt to parse the <see cref="ScimError.Status"/> as an <see cref="HttpStatusCode"/> 
        /// and assign it to the response message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scimResponse">The content contained in the HTTP response.</param>
        /// <param name="httpRequestMessage">The active <see cref="HttpRequestMessage"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> to set if <paramref name="scimResponse"/> has no errors.</param>
        /// <returns>HttpResponseMessage instance.</returns>
        public static HttpResponseMessage ToHttpResponseMessage<T>(
            this IScimResponse<T> scimResponse,
            HttpRequestMessage httpRequestMessage,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return scimResponse.ToHttpResponseMessage(httpRequestMessage, null, statusCode);
        }

        private static HttpResponseMessage ToHttpResponseMessage<T>(
            this IScimResponse<T> scimResponse, 
            HttpRequestMessage httpRequestMessage,
            Action<T, HttpResponseMessage> responseBuilder,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            bool setResponseContent = true)
        {
            if (scimResponse == null)
            {
                throw new ArgumentNullException("scimResponse");
            }

            if (httpRequestMessage == null)
            {
                throw new ArgumentNullException("httpRequestMessage");
            }

            HttpResponseMessage response;
            HttpStatusCode responseStatusCode = scimResponse.IsLeft
                ? GetStatusCode(scimResponse.GetLeft())
                : statusCode;
            bool shouldSetResponseContent = setResponseContent && ShouldSetResponseContent(httpRequestMessage, responseStatusCode);

            if (shouldSetResponseContent)
            {
                /*
                if (httpRequestMessage.Headers.Accept.Count == 0)
                {
                    MediaTypeFormatter mediaTypeFormatter = httpRequestMessage.GetConfiguration().Formatters.JsonFormatter;
                    MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/scim+json") { CharSet = "utf-8" };
                    response = scimResponse.IsLeft
                        ? httpRequestMessage.CreateResponse(responseStatusCode, scimResponse.GetLeft(), mediaTypeFormatter, mediaType)
                        : httpRequestMessage.CreateResponse(responseStatusCode, scimResponse.GetRight(), mediaTypeFormatter, mediaType);
                }
                else
                */
                {
                    response = scimResponse.IsLeft 
                        ? httpRequestMessage.CreateResponse(responseStatusCode, scimResponse.GetLeft()) 
                        : httpRequestMessage.CreateResponse(responseStatusCode, scimResponse.GetRight());
                }
            }
            else
            {
                response = httpRequestMessage.CreateResponse(responseStatusCode);
            }

            if (scimResponse.IsRight && responseBuilder != null)
            {
                responseBuilder.Invoke(scimResponse.GetRight(), response);
            }

            return response;
        }

        /// <summary>
        /// Invokes the specified <paramref name="responseBuilder" /> action if <paramref name="scimResponse" /> does not contain an error - returning the configured <see cref="HttpResponseMessage" />.
        /// If <paramref name="scimResponse" /> contains errors, the returned response with contain the error content and will attempt to parse the <see cref="ScimError.Status" /> as an
        /// <see cref="HttpStatusCode" /> and assign it to the response message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scimResponse">The <see cref="IScimResponse{T}" /> used to build the <see cref="HttpResponseMessage" />.</param>
        /// <param name="httpRequestMessage">The active <see cref="HttpRequestMessage" />.</param>
        /// <param name="responseBuilder">The response builder method to invoke when no errors exist.</param>
        /// <param name="setResponseContent">Determines whether to set the <see ref="scimResponse.Data" /> as the response content.</param>
        /// <returns>HttpResponseMessage instance.</returns>
        public static HttpResponseMessage ToHttpResponseMessage<T>(
            this IScimResponse<T> scimResponse,
            HttpRequestMessage httpRequestMessage,
            Action<T, HttpResponseMessage> responseBuilder,
            bool setResponseContent = true)
        {
            return scimResponse.ToHttpResponseMessage(httpRequestMessage, responseBuilder, HttpStatusCode.OK, setResponseContent);
        }

        private static object GetContent<T>(this IScimResponse<T> response)
        {
            return response.IsLeft ? (object)response.GetLeft() : response.GetRight();
        }

        private static HttpStatusCode GetStatusCode(ScimError error)
        {
            if (error != null) return error.Status;

            return HttpStatusCode.BadRequest;
        }

        private static bool ShouldSetResponseContent(HttpRequestMessage httpRequestMessage, HttpStatusCode responseStatusCode)
        {
            return httpRequestMessage.Method != HttpMethod.Head &&
                   responseStatusCode != HttpStatusCode.NoContent &&
                   responseStatusCode != HttpStatusCode.ResetContent &&
                   responseStatusCode != HttpStatusCode.NotModified &&
                   responseStatusCode != HttpStatusCode.Continue;
        }
    }
}