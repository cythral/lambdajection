namespace Lambdajection.Generator.TemplateGeneration
{
    public class Output
    {
        public Output(object value, object name)
        {
            Value = value;
            Export = new Export { Name = name };
        }

        public object Value { get; set; }

        public Export Export { get; set; }
    }
}
