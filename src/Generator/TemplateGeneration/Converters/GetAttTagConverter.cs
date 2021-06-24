using System;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Lambdajection.Generator.TemplateGeneration
{
    public class GetAttTagConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(GetAttTag);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            parser.MoveNext();
            return new GetAttTag();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var result = (GetAttTag)value!;
            emitter.Emit(new Scalar(
                null,
                "!GetAtt",
                $"{result.Name}.{result.Attribute}",
                ScalarStyle.Plain,
                false,
                false
            ));
        }
    }
}
