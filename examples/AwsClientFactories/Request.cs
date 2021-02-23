namespace Lambdajection.Examples.AwsClientFactories
{
    public class Request
    {
        public string RoleArn { get; set; } = "";

        public string BucketName { get; set; } = "";

        public string FileName { get; set; } = "";

        public string Contents { get; set; } = "";
    }
}
