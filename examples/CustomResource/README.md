# Custom Resource Example

This example illustrates the use of Lambdajection to create custom resource providers.  Resource properties can be annotated with data annotation attributes, which will be evaluated during invocation.  If updating a property should result in replacement instead of update, you can annotate that property with the UpdateRequiresReplacementAttribute (in the Lambdajection.Attributes namespace, see [Request.cs](./Request.cs) for example usage). Other Lambdajection features, such as options work seamlessly with Custom Resources.

## Files

- [Lambda Code](./PasswordGenerator.cs)
- [Startup Code](./Startup.cs)
- [Resource Properties Type](./Request.cs)
- [Output Type](./Response.cs)
- [Options Code](./Options.cs)
- [CloudFormation Template](./cloudformation.template.yml)


## Steps To Recreate

1. Create a new netcoreapp3.1 project, and add the following packages:
   - Lambdajection
   - Lambdajection.CustomResource
   - System.ComponentModel.DataAnnotations

2. Create an [Options Class](./Options.cs), and add properties for any options you'd like to add. 

3. Create the [Startup Class](./Startup.cs) and inject RNGCryptoServiceProvider as a singleton.

4. Create the [Request Class](./Request.cs)
   1. Add an option for length, restrict it between 3 and 12 with the RangeAttribute
   2. Add the UpdateRequiresReplacementAttribute to the length property

5. Create the [Response Class](./Response.cs)

6. Create a Handler called [Password Generator](./PasswordGenerator.cs)
   1. Annotate it with the CustomResourceProviderAttribute, pointing the startup argument to the Startup class you just created.
   2. Fill out the Create, Update and Delete methods.

7. Create a [CloudFormation Template](./cloudformation.template.yml) for your Lambda.
   - Note that we are using the serverless transform in the example, however using the transform is optional.

## How it Works

1. During a build, Lambdajection scans the compilation for any classes marked with an attribute extending from [LambdaAttribute](../../src/Attributes/LambdaAttribute.cs).
2. The generator will generate code based on attributes provided on the LambdaAttribute found, including [LambdaHostAttribute](../../src/Framework/LambdaHostAttribute.cs) and [LambdaInterfaceAttribute](../../src/Framework/LambdaInterfaceAttribute.cs).
    - The LambdaHostAttribute tells the Generator the type of host to use for running the Lambda.  In this case, it will use the CustomResourceLambdaHost.
    - The LambdaInterfaceAttribute tells the Generator which interface to validate the attributed class against, and generate any missing methods for.  In this case, it adds the ICustomResourceProvider interface to the PasswordGenerator, and generates the RequiresReplacement and Validate methods for it.
3. The generator will generate a static Run method, which will invoke the [CustomResourceLambdaHost](../../src/CustomResource/CustomResourceLambdaHost.cs).  The host contains the logic for deciding whether to call Create, Update or Delete.