using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    public readonly struct LambdaCompilationScanResult : IEquatable<LambdaCompilationScanResult>
    {
        public LambdaCompilationScanResult(Dictionary<string, ClassDeclarationSyntax> optionClasses, IEnumerable<AwsServiceMetadata> awsServices)
        {
            this.OptionClasses = optionClasses;
            this.AwsServices = awsServices;
        }

        public Dictionary<string, ClassDeclarationSyntax> OptionClasses { get; }

        public IEnumerable<AwsServiceMetadata> AwsServices { get; }


        public override bool Equals(object? obj)
        {
            return obj is LambdaCompilationScanResult result
                && result.OptionClasses.Equals(OptionClasses)
                && result.AwsServices.Equals(AwsServices);
        }

        public bool Equals(LambdaCompilationScanResult result)
        {
            return result.OptionClasses.Equals(OptionClasses) && result.AwsServices.Equals(AwsServices);
        }

        public override int GetHashCode()
        {
            return new { OptionClasses, AwsServices }.GetHashCode();
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
