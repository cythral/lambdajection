using System;
using System.Diagnostics;

namespace Lambdajection.Encryption
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class EncryptedAttribute : Attribute
    {
    }
}
