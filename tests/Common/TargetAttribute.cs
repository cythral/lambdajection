using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Parameter)]
public class TargetAttribute : Attribute
{
    public TargetAttribute(Type? overridesType = null, string? overridesMemberName = null)
    {
        if (overridesType == null || overridesMemberName == null)
        {
            return;
        }

        var fieldsQuery = from member in overridesType.GetFields(BindingFlags.Static | BindingFlags.Public)
                          where member.Name == overridesMemberName
                          select member;

        var value = (Dictionary<Type, object>?)fieldsQuery.First().GetValue(null!);
        Overrides = value ?? Overrides;
    }

    public Dictionary<Type, object> Overrides { get; } = new Dictionary<Type, object>();
}
