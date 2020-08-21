using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambdajection.Generator
{
    public readonly struct LambdaCompilationScanResult : IEquatable<LambdaCompilationScanResult>
    {
        public LambdaCompilationScanResult(Dictionary<string, ClassDeclarationSyntax> optionClasses, IEnumerable<string> awsServices)
        {
            this.OptionClasses = optionClasses;
            this.AwsServices = awsServices;
        }

        public Dictionary<string, ClassDeclarationSyntax> OptionClasses { get; }
        public IEnumerable<string> AwsServices { get; }


        public override bool Equals(object? obj)
        {
            return obj is LambdaCompilationScanResult result && result.OptionClasses.Equals(OptionClasses);
        }

        public bool Equals(LambdaCompilationScanResult result)
        {
            return result.OptionClasses.Equals(OptionClasses);
        }

        public override int GetHashCode()
        {
            return OptionClasses.GetHashCode();
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
