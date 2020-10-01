# Encrypted Options Example

This example illustrates the use of a custom serializer, which is set using the Serializer argument on the Lambda handler.  This also makes use of embedded resources and application load balancer events.

## Files

- [Lambda Code](Handler.cs)
- [Options Code](Options.cs)
- [Startup Code](Startup.cs)
- [Embedded Resource Reader](EmbeddedResourceReader.cs)
- [About HTML Page](Resources/about.html)
- [CloudFormation Template](cloudformation.template.yml)

## Steps to Recreate

1. Create a new netcoreapp3.1 and add the following packages:

   - Lambdajection
   - Amazon.Lambda.ApplicationLoadBalancerEvents

2. Create an [about.html](./Resources/about.html) file and add some HTML to the file.  Instruct MSBuild to add this file as an EmbeddedResource (can be done via the [csproj file](./CustomSerializer.csproj)).


3. Create an [EmbeddedResourceReader](./EmbeddedResourceReader.cs) which can read assembly-embedded resources / files. 


4. Add a [Startup Class](Startup.cs) and configure services that will be injected into your Lambda Handler in here.  The EmbeddedResourceReader will need to be injected in, preferably as a singleton.

5. Add a [Lambda Handler](Handler.cs) containing the code that will run when the Lambda is invoked.

6. Create a [CloudFormation template](cloudformation.template.yml) for your Lambda.
   - Note that we are using the serverless transform in the example, however using the transform is optional.

## How it Works

1. During a build, the generator adds a LambdaSerializer attribute to the `Run` method, passing in the `Serializer` argument you gave to the Lambda attribute.  
  - If you didn't specify one and are targeting netcoreapp3.1, `Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer` is the default serializer used. 
2. 