using System;
using System.Diagnostics;

namespace Lambdajection.Encryption
{
    /// <summary>
    /// Attribute used to indicate an encrypted property. Properties annotated with this
    /// are decrypted on startup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class EncryptedAttribute : Attribute
    {
    }
}
