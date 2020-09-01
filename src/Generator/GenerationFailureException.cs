using System;

namespace Lambdajection.Generator
{
    public class GenerationFailureException : Exception
    {
        public GenerationFailureException() { }
        public GenerationFailureException(string message) : base(message) { }
        public GenerationFailureException(string message, Exception innerException) : base(message, innerException) { }
    }
}
