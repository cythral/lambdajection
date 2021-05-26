using System;
using System.Collections.Generic;

using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Lambdajection.Utils
{
    public class IntrinsicNodeTypeResolver : INodeTypeResolver
    {
        private readonly HashSet<string> intrinsicFunctionShortForms = new() { "!GetAtt", "!Sub" };

        public bool Resolve(NodeEvent? nodeEvent, ref Type currentType)
        {
            if (!string.IsNullOrEmpty(nodeEvent?.Tag.ToString()) && intrinsicFunctionShortForms.Contains(nodeEvent?.Tag.ToString() ?? string.Empty))
            {
                currentType = typeof(string);
                return true;
            }

            return false;
        }
    }
}
