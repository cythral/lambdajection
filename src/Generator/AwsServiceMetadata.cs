using System;

namespace Lambdajection.Generator
{
    public readonly struct AwsServiceMetadata : IEquatable<AwsServiceMetadata>
    {
        public AwsServiceMetadata(string serviceName, string interfaceName, string implementationName, string namespaceName)
        {
            this.ServiceName = serviceName;
            this.InterfaceName = interfaceName;
            this.ImplementationName = implementationName;
            this.NamespaceName = namespaceName;
        }

        public string ServiceName { get; }
        public string InterfaceName { get; }
        public string ImplementationName { get; }
        public string NamespaceName { get; }



        public override bool Equals(object? obj)
        {
            return obj is AwsServiceMetadata result
                && result.ServiceName.Equals(ServiceName)
                && result.InterfaceName.Equals(InterfaceName)
                && result.ImplementationName.Equals(ImplementationName)
                && result.NamespaceName.Equals(NamespaceName);
        }

        public bool Equals(AwsServiceMetadata result)
        {
            return result.ServiceName.Equals(ServiceName)
                && result.InterfaceName.Equals(InterfaceName)
                && result.ImplementationName.Equals(ImplementationName)
                && result.NamespaceName.Equals(NamespaceName);
        }

        public override int GetHashCode()
        {
            return new { ServiceName, InterfaceName, ImplementationName, NamespaceName }.GetHashCode();
        }

        public static bool operator ==(AwsServiceMetadata scanA, AwsServiceMetadata scanB)
        {
            return scanA.Equals(scanB);
        }

        public static bool operator !=(AwsServiceMetadata scanA, AwsServiceMetadata scanB)
        {
            return !scanA.Equals(scanB);
        }
    }
}
