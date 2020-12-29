using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AutoFixture.AutoNSubstitute;
using AutoFixture.Kernel;

public class TargetRelay : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        var parameterRequest = request as ParameterInfo;
        var targetAttribute = (TargetAttribute?)parameterRequest?.GetCustomAttribute(typeof(TargetAttribute), true);

        if (parameterRequest == null || targetAttribute == null)
        {
            return new NoSpecimen();
        }

        var type = parameterRequest.ParameterType;
        var constructors = from ctor in type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                           where ctor.GetParameters().Any()
                           select ctor;

        var constructor = constructors.FirstOrDefault();
        var parameterTypes = from parameter in constructor?.GetParameters() ?? Array.Empty<ParameterInfo>()
                             let @override = GetOrDefault(targetAttribute.Overrides, parameter.ParameterType)
                             select
                                @override ??
                                context.Resolve(parameter.ParameterType) ??
                                context.Resolve(new SubstituteRequest(parameter.ParameterType));

        var parameters = parameterTypes.ToArray();
        var instance = parameters.Any()
            ? Activator.CreateInstance(type, parameters)
            : Activator.CreateInstance(type, true);

        return instance ?? new NoSpecimen();
    }

    private static object? GetOrDefault(Dictionary<Type, object> dictionary, Type key)
    {
        dictionary.TryGetValue(key, out var result);
        return result;
    }
}
