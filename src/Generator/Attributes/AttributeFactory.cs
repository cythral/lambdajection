using System;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Lambdajection.Generator.Attributes
{
    public static class AttributeFactory
    {
        public static T Create<T>(AttributeData attributeData)
        {
            var parameters = from arg in attributeData.ConstructorArguments
                             select arg.Value;

            var result = (T?)Activator.CreateInstance(typeof(T), parameters.ToArray());
            foreach (var (key, constant) in attributeData.NamedArguments)
            {
                var value = constant.Value;
                var prop = typeof(T).GetProperty(key, BindingFlags.Instance | BindingFlags.Public);
                var setter = prop?.GetSetMethod(true);
                setter?.Invoke(result, new[] { value });
            }

            return result == null
                ? throw new Exception("Could not create attribute from attributedata")
                : result;
        }
    }
}
