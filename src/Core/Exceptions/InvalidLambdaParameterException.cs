using System;

namespace Lambdajection.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when there is a problem with the lambda request parameter.
    /// </summary>
    public class InvalidLambdaParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLambdaParameterException" /> class.
        /// </summary>
        public InvalidLambdaParameterException()
            : base("The given lambda parameter was invalid and could not be read from the input stream.")
        {
        }
    }
}
