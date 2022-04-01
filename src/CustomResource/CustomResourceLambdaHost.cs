using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#pragma warning disable SA1119

namespace Lambdajection.CustomResource
{
    /// <inheritdoc />
    public sealed class CustomResourceLambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        : LambdaHostBase<TLambda, CustomResourceRequest<TLambdaParameter>, CustomResourceResponse<TLambdaOutput>, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        where TLambda : class, ICustomResourceProvider<TLambdaParameter, TLambdaOutput>
        where TLambdaParameter : class
        where TLambdaOutput : class, ICustomResourceOutputData
        where TLambdaStartup : class, ILambdaStartup
        where TLambdaConfigurator : class, ILambdaConfigurator
        where TLambdaConfigFactory : class, ILambdaConfigFactory, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomResourceLambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        public CustomResourceLambdaHost()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomResourceLambdaHost{TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory}" /> class.
        /// </summary>
        /// <param name="build">The builder action to run on the lambda.</param>
        internal CustomResourceLambdaHost(Action<LambdaHostBase<TLambda, CustomResourceRequest<TLambdaParameter>, CustomResourceResponse<TLambdaOutput>, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
            : base(build)
        {
        }

        /// <inheritdoc />
        public override async Task<CustomResourceResponse<TLambdaOutput>> InvokeLambda(
            Stream inputStream,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            CustomResourceResponse<TLambdaOutput> response;
            var input = await DeserializeInput(inputStream, cancellationToken);

            try
            {
                var request = GetFullRequest(input);
                Validate(request);

                var requestType = GetRequestType(request);
                var data = await (requestType switch
                {
                    CustomResourceRequestType.Create => Lambda.Create(request, cancellationToken),
                    CustomResourceRequestType.Update => Lambda.Update(request, cancellationToken),
                    CustomResourceRequestType.Delete => Lambda.Delete(request, cancellationToken),
                    _ => throw new Exception($"Unknown RequestType '{request.RequestType}'"),
                });

                response = new CustomResourceResponse<TLambdaOutput>
                {
                    Status = CustomResourceResponseStatus.Success,
                    RequestId = request.RequestId,
                    StackId = request.StackId,
                    LogicalResourceId = request.LogicalResourceId,
                    PhysicalResourceId = data.Id,
                    Data = data,
                };
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, "Received exception from custom resource provider");
                response = new CustomResourceResponse<TLambdaOutput>
                {
                    Status = CustomResourceResponseStatus.Failed,
                    RequestId = input.RequestId,
                    StackId = input.StackId,
                    LogicalResourceId = input.LogicalResourceId,
                    PhysicalResourceId = input.PhysicalResourceId,
                    Reason = exception.Message,
                };
            }

            Logger.LogInformation("Custom resource response: {response}", response);
            await Respond(input, response, cancellationToken);
            return response;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CustomResourceRequestType GetRequestType(CustomResourceRequest<TLambdaParameter> request)
        {
            if (Lambda.RequiresReplacement(request))
            {
                Logger.LogInformation("Non-updatable property/properties changed, new resource must be created.");
                return CustomResourceRequestType.Create;
            }

            return request.RequestType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<CustomResourceRequest> DeserializeInput(Stream stream, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Logger.LogInformation("Starting deserialization of custom resource request.");
            Stopwatch.Restart();

            var input = await Serializer.Deserialize<CustomResourceRequest>(stream, cancellationToken);
            var result = input ?? throw new SerializationException("Request unexpectedly deserialized to null.");

            Stopwatch.Stop();
            Logger.LogInformation("Deserialization of custom resource request finished in {time} ms", Stopwatch.ElapsedMilliseconds);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CustomResourceRequest<TLambdaParameter> GetFullRequest(CustomResourceRequest request)
        {
            var result = new CustomResourceRequest<TLambdaParameter>
            {
                RequestType = request.RequestType,
                ResponseURL = request.ResponseURL,
                StackId = request.StackId,
                RequestId = request.RequestId,
                ResourceType = request.ResourceType,
                LogicalResourceId = request.LogicalResourceId,
                PhysicalResourceId = request.PhysicalResourceId,
                ResourceProperties = GetExtraPropertyAsParameter(request, "ResourceProperties"),
                OldResourceProperties = GetExtraPropertyAsParameter(request, "OldResourceProperties"),
            };

            Logger.LogInformation("Received custom resource request: {request}", result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Validate(CustomResourceRequest<TLambdaParameter> request)
        {
            Logger.LogInformation("Beginning request validation.");
            Stopwatch.Restart();

            Lambda.Validate(request);

            Stopwatch.Stop();
            Logger.LogInformation("Finished request validation in {time} ms", Stopwatch.ElapsedMilliseconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLambdaParameter? GetExtraPropertyAsParameter(CustomResourceRequest request, string propertyName)
        {
            if (request.ExtraProperties.TryGetValue(propertyName, out var element))
            {
                var text = element.GetRawText();
                var parameter = Serializer.Deserialize<TLambdaParameter>(text);
                return parameter;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task Respond(
            CustomResourceRequest request,
            CustomResourceResponse<TLambdaOutput> response,
            CancellationToken cancellationToken
        )
        {
            if (request.ResponseURL == null)
            {
                return;
            }

            Logger.LogInformation("Sending response to {responseURL}", request.ResponseURL);
            Stopwatch.Restart();

            var httpClient = Scope.ServiceProvider.GetRequiredService<IHttpClient>();
            await httpClient.PutJson(
                requestUri: request.ResponseURL,
                payload: response,
                contentType: null,
                cancellationToken: cancellationToken
            );

            Stopwatch.Stop();
            Logger.LogInformation("Sent response to {responseURL} in {time} ms", request.ResponseURL, Stopwatch.ElapsedMilliseconds);
        }
    }
}
