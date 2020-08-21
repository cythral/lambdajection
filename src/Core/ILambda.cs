using System.Threading.Tasks;

using Amazon.Lambda.Core;

namespace Lambdajection.Core
{
    public interface ILambda<TLambdaParameter, TLambdaOutput>
    {
        Task<TLambdaOutput> Handle(TLambdaParameter parameter, ILambdaContext context);
    }
}
