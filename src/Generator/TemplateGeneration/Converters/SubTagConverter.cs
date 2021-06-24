using System;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Lambdajection.Generator.TemplateGeneration
{
    public class SubTagConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(SubTag);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            parser.MoveNext();
            return new SubTag();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var result = (SubTag)value!;
            emitter.Emit(new Scalar(
                null,
                "!Sub",
                $"{result.Expression}",
                ScalarStyle.Plain,
                false,
                false
            ));
        }
    }
}
