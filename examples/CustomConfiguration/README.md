# Custom Configuration Example

It is sometimes useful to add multiple configuration sources.  This example illustrates the use of a custom config factory to achieve that.

## Files

- [Lambda Code](Handler.cs)
- [Config Factory Code](ConfigFactory.cs)
- [Startup Code](Startup.cs)
- [CloudFormation Template](cloudformation.template.yml)

## Steps to Recreate

1. Create a new netcoreapp3.1 and add the following packages:

   - Lambdajection

2. Create a [ConfigFactory](./ConfigFactory.cs) that enables file-based configuration.  Set the ConfigFactory argument in the Lambda Handler attribute to point to your config factory's type.


3. Create a [Config / Options](./Config.cs) class that will be used to hold your configuration items.  Inject this class into your handler and read values from it inside the `Handle` method.


4. Add a [Startup Class](Startup.cs) and configure any services that will be injected into your Lambda Handler in here.

5. Create a [CloudFormation template](cloudformation.template.yml) for your Lambda.
   - Note that we are using the serverless transform in the example, however using the transform is optional.

## How it Works

1. During a build, the generator will use your config factory in place of [the default one](../../src/Core/LambdaConfigFactory.cs).  The type of this class is added as a generic argument to the [Lambda Host](../../src/Core/LambdaHost.cs).
2. When the lambda is run, the [Lambda Host Builder](../../src/Core/LambdaHostBuilder.cs) will create a new instance of your config factory and create a configuration instance using that factory.  That config instance will then be added to the service collection / provider passed to the lambda host as well as your startup class.