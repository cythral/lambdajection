using System;
using System.Collections.Generic;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Lambdajection.Utils
{
    public class IntrinsicNodeDeserializer : INodeDeserializer
    {
        private readonly HashSet<string> intrinsicFunctionShortForms = new() { "!GetAtt", "!Sub" };

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (reader.Accept<Scalar>(out var scalar))
            {
                if (intrinsicFunctionShortForms.Contains(scalar.Tag.ToString()))
                {
                    value = $"{scalar.Tag} {scalar.Value}";
                    reader.MoveNext();
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
