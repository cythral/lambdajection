using NUnit.Framework;

using static System.Environment;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeSetUp]
    public void Setup()
    {
        SetEnvironmentVariable("AWS_REGION", "us-east-1");
        SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "AWS_SECRET_ACCESS_KEY");
        SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "AWS_ACCESS_KEY_ID");
    }
}