# AWS Client Factories Example

This example illustrates the use of both the IServiceCollection.UseAwsServices helper and IAwsFactory features.

## Files

- [Lambda Code](Handler.cs)
- [Options Code](Options.cs)
- [Startup Code](Startup.cs)
- [CloudFormation Template](cloudformation.template.yml)

## Steps to Recreate

1. Create a new netcoreapp3.1 and add the following packages:

   - Lambdajection
   - AWSSDK.Service (replace service with the aws service you want to use)
   - AWSSDK.SecurityToken
   - Amazon.Lambda.Serialization.SystemTextJson (optional - may use other serializer)

2. Add a [Startup Class](Startup.cs) and configure services that will be injected into your Lambda Handler in here.

   - For each AWS service, add a line like this:
     ```cs
     services.UseAwsService<TAmazonService>(); // Replace TAmazonService with the interface type of the client you want to add
     ```

3. Add a [Lambda Handler](Handler.cs) containing the code that will run when the Lambda is invoked.

   - In the constructor, add a parameter like this:

   ```cs
   Handler(IAwsFactory<TAmazonService> serviceFactory) // Replace TAmazonService with the interface type of the client you want to add
   ```

   - When you need to assume a role, do the following:

   ```cs
   var client = await serviceFactory.Create("arn:aws:iam::account:role/RoleName");
   ```

4. Create a [CloudFormation template](cloudformation.template.yml) for your Lambda.
   - Create a lambda execution role (the serverless transform creates one for us automatically)
   - Create another role that the execution role can assume
   - Give the role that the lambda assumes permissions to do something the execution role cannot
   - Note that we are using the serverless transform in the example, however using the transform is optional.

## How it Works

1. During a build, the Code Generator scans your startup class for instances of serviceCollection.UseAwsService<>().
2. For each invocation, it figures out the implementation type and adds it to the IoC container, by putting the following in the ConfigureAwsServices method of the LambdaConfigurator subclass:
   ```cs
   services.AddSingleton<TInterface, TImplementation>();
   services.AddSingleton<IAwsFactory<TInterface>, AwsFactory>();
   ```
   - A basic IAwsFactory implementation is generated for each service and is added as a subclass to the configurator. If AWSSDK.SecurityToken is not a dependency of your project, the factory is not generated.
3. When building the Lambda Host / IoC container, the LambdaConfigurator.ConfigureAwsServices method is called, adding the appropriate Amazon Service Clients and factories.
