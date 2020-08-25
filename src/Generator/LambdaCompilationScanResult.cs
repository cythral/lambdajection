using System;
using System.Collections.Generic;

namespace Lambdajection.Generator
{
    public readonly struct LambdaCompilationScanResult : IEquatable<LambdaCompilationScanResult>
    {
        public LambdaCompilationScanResult(HashSet<OptionClass> optionClasses, IEnumerable<AwsServiceMetadata> awsServices, bool includeDecryptionFacade)
        {
            this.OptionClasses = optionClasses;
            this.AwsServices = awsServices;
            this.IncludeDecryptionFacade = includeDecryptionFacade;
        }

        public HashSet<OptionClass> OptionClasses { get; }

        public IEnumerable<AwsServiceMetadata> AwsServices { get; }

        public bool IncludeDecryptionFacade { get; }


        public override bool Equals(object? obj)
        {
            return obj is LambdaCompilationScanResult result
                && result.OptionClasses.Equals(OptionClasses)
                && result.AwsServices.Equals(AwsServices)
                && result.IncludeDecryptionFacade == IncludeDecryptionFacade;
        }

        public bool Equals(LambdaCompilationScanResult result)
        {
            return result.OptionClasses.Equals(OptionClasses)
                && result.AwsServices.Equals(AwsServices)
                && result.IncludeDecryptionFacade == IncludeDecryptionFacade;
        }

        public override int GetHashCode()
        {
            return new { OptionClasses, AwsServices, IncludeDecryptionFacade }.GetHashCode();
        }

        public static bool operator ==(LambdaCompilationScanResult scanA, LambdaCompilationScanResult scanB)
        {
            return scanA.Equals(scanB);
        }

        public static bool operator !=(LambdaCompilationScanResult scanA, LambdaCompilationScanResult scanB)
        {
            return !scanA.Equals(scanB);
        }
    }
}
