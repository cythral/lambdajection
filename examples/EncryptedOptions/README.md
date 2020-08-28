# Encrypted Options Example

This example illustrates the use of both option classes and encrypted values within them. On startup, any option property annotated with the [Encrypted] attribute will be automatically decrypted before being passed to your Lambda Function.

## Files

- [Lambda Code](Handler.cs)
- [Options Code](Options.cs)
- [Startup Code](Startup.cs)
- [CloudFormation Template](cloudformation.template.yml)

## Steps to Recreate

1. Create a new netcoreapp3.1 and add the following packages:

   - Lambdajection
   - Lambdajection.Encryption
   - Amazon.Lambda.Serialization.SystemTextJson (optional - may use other serializer)

2. Add a [Startup Class](Startup.cs) and configure services that will be injected into your Lambda Handler in here.

   - An [IDecryptionService](../../src/Encryption/IDecryptionService.cs) will be injected into your container without any need for specific configuration.
   - The [default IDecryptionService](../../src/Encryption/DefaultDecryptionService.cs) uses KMS to decrypt options.
   - You can use your own IDecryptionService by injecting it into the container as a singleton in your Startup file.

3. Add a [Lambda Handler](Handler.cs) containing the code that will run when the Lambda is invoked.

4. Add one or more [Option classes](Options.cs)

   - Encrypted values should be annotated with the [Encrypted] attribute.
   - Please note that the [Encrypted] attribute cannot be accessed with reflection since it is only present at build time.

5. Create a [CloudFormation template](cloudformation.template.yml) for your Lambda.
   - Add environment variables for each of your options. Naming goes like: `SectionName__OptionName`.
   - Note that we are using the serverless transform in the example, however using the transform is optional.

## How it Works

1. During a build, the Code Generator scans your code for option classes. For each option class it finds, it keeps track of the option class, the section name provided in the [LambdaOptions] attribute, and a list of encrypted properties.
2. Generator builds a LambdaConfigurator subclass inside your Lambda class. This will contain a ConfigureOptions method, which will configure each option class.
3. If there were any encrypted properties found:
   - An IDecryptionService will be injected into the IoC container inside LambdaConfigurator.ConfigureOptions.
   - A Decryptor class is generated that uses the IDecryptionService to decrypt each encrypted property. This will also be injected into the IoC container.
4. When the Lambda Host is setup, it will first call your startup class' ConfigureServices then the LambdaConfigurator.ConfigureOptions method - that way you can use your own decryption service if you want.
