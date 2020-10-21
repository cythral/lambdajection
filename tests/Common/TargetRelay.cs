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
        var constructors = from ctor in type.GetConstructors()
                           where ctor.GetParameters().Any()
                           select ctor;

        var constructor = constructors.First();
        var parameterTypes = from parameter in constructor.GetParameters()
                             let @override = GetOrDefault(targetAttribute.Overrides, parameter.ParameterType)
                             select
                                @override ??
                                context.Resolve(parameter.ParameterType) ??
                                context.Resolve(new SubstituteRequest(parameter.ParameterType));

        var parameters = parameterTypes.ToArray();
        var instance = Activator.CreateInstance(type, parameters);
        return instance ?? new NoSpecimen() as object;
    }

    private static object? GetOrDefault(Dictionary<Type, object> dictionary, Type key)
    {
        dictionary.TryGetValue(key, out object? result);
        return result;
    }
}
