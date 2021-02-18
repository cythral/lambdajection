using System;

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
            CustomResourceRequest<TLambdaParameter> parameter,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            CustomResourceResponse<TLambdaOutput> response;
            var httpClient = Scope.ServiceProvider.GetRequiredService<IHttpClient>();

            try
            {
                Lambda.Validate(parameter);
                var data = await (parameter.RequestType switch
                {
                    CustomResourceRequestType.Create => Lambda.Create(parameter, cancellationToken),
                    CustomResourceRequestType.Update => Lambda.Update(parameter, cancellationToken),
                    CustomResourceRequestType.Delete => Lambda.Delete(parameter, cancellationToken),
                    _ => throw new Exception($"Unknown RequestType '{parameter.RequestType}'"),
                });

                response = new CustomResourceResponse<TLambdaOutput>
                {
                    Status = CustomResourceResponseStatus.Success,
                    RequestId = parameter.RequestId,
                    StackId = parameter.StackId,
                    LogicalResourceId = parameter.LogicalResourceId,
                    PhysicalResourceId = data.Id,
                    Data = data,
                };
            }
            catch (Exception e)
            {
                response = new CustomResourceResponse<TLambdaOutput>
                {
                    Status = CustomResourceResponseStatus.Failed,
                    RequestId = parameter.RequestId,
                    StackId = parameter.StackId,
                    LogicalResourceId = parameter.LogicalResourceId,
                    PhysicalResourceId = parameter.PhysicalResourceId,
                    Reason = e.Message,
                };
            }

            await httpClient.PutJson(
                requestUri: parameter.ResponseURL,
                payload: response,
                contentType: null,
                cancellationToken: cancellationToken
            );

            return response.Data!;
        }
    }
}
