using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;

#pragma warning disable SA1119

namespace Lambdajection.CustomResource
{
    /// <inheritdoc />
    public sealed class CustomResourceLambdaHost<TLambda, TLambdaParameter, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
        : LambdaHostBase<TLambda, CustomResourceRequest<TLambdaParameter>, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>
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
        internal CustomResourceLambdaHost(Action<LambdaHostBase<TLambda, CustomResourceRequest<TLambdaParameter>, TLambdaOutput, TLambdaStartup, TLambdaConfigurator, TLambdaConfigFactory>> build)
            : base(build)
        {
        }

        /// <inheritdoc />
        public override async Task<TLambdaOutput> InvokeLambda(
            Stream inputStream,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            CustomResourceResponse<TLambdaOutput> response;
            var httpClient = Scope.ServiceProvider.GetRequiredService<IHttpClient>();
            var input = await DeserializeInput(inputStream, cancellationToken);

            try
            {
                var request = GetFullRequest(input);
                Lambda.Validate(request);

                var requestType = Lambda.RequiresReplacement(request)
                    ? CustomResourceRequestType.Create
                    : input!.RequestType;

                var data = await (requestType switch
                {
                    CustomResourceRequestType.Create => Lambda.Create(request, cancellationToken),
                    CustomResourceRequestType.Update => Lambda.Update(request, cancellationToken),
                    CustomResourceRequestType.Delete => Lambda.Delete(request, cancellationToken),
                    _ => throw new Exception($"Unknown RequestType '{input!.RequestType}'"),
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
            catch (Exception e)
            {
                response = new CustomResourceResponse<TLambdaOutput>
                {
                    Status = CustomResourceResponseStatus.Failed,
                    RequestId = input.RequestId,
                    StackId = input.StackId,
                    LogicalResourceId = input.LogicalResourceId,
                    PhysicalResourceId = input.PhysicalResourceId,
                    Reason = e.Message,
                };
            }

            await httpClient.PutJson(
                requestUri: input.ResponseURL,
                payload: response,
                contentType: null,
                cancellationToken: cancellationToken
            );

            return response.Data!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<CustomResourceRequest> DeserializeInput(Stream stream, CancellationToken cancellationToken)
        {
            var input = await JsonSerializer.DeserializeAsync<CustomResourceRequest>(stream, cancellationToken: cancellationToken);
            return input ?? throw new SerializationException("Request unexpectedly deserialized to null.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CustomResourceRequest<TLambdaParameter> GetFullRequest(CustomResourceRequest request)
        {
            return new CustomResourceRequest<TLambdaParameter>
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLambdaParameter? GetExtraPropertyAsParameter(CustomResourceRequest request, string propertyName)
        {
            if (request.ExtraProperties.TryGetValue(propertyName, out var element))
            {
                var text = element.GetRawText();
                var parameter = JsonSerializer.Deserialize<TLambdaParameter>(text);
                return parameter;
            }

            return null;
        }
    }
}
