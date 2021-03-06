using System;

namespace Lambdajection.Generator
{
    public readonly struct AwsServiceMetadata : IEquatable<AwsServiceMetadata>
    {
        public AwsServiceMetadata(string serviceName, string interfaceName, string implementationName, string namespaceName)
        {
            ServiceName = serviceName;
            InterfaceName = interfaceName;
            ImplementationName = implementationName;
            NamespaceName = namespaceName;
        }

        public string ServiceName { get; }

        public string InterfaceName { get; }

        public string ImplementationName { get; }

        public string NamespaceName { get; }

        public static bool operator ==(AwsServiceMetadata metadataA, AwsServiceMetadata metadataB)
        {
            return metadataA.Equals(metadataB);
        }

        public static bool operator !=(AwsServiceMetadata metadataA, AwsServiceMetadata metadataB)
        {
            return !metadataA.Equals(metadataB);
        }

        public override bool Equals(object? obj)
        {
            return obj is AwsServiceMetadata result && result.ServiceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(AwsServiceMetadata result)
        {
            return result.ServiceName.Equals(ServiceName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return ServiceName.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
