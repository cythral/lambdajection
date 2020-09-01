using NUnit.Framework;

using static System.Environment;

[SetUpFixture]
public class Setup
{
    [OneTimeSetUp]
    public void SetupEnvironmentVariables()
    {
        SetEnvironmentVariable("AWS_REGION", "us-east-1");
        SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "AWS_SECRET_ACCESS_KEY");
        SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "AWS_ACCESS_KEY_ID");
    }
}
