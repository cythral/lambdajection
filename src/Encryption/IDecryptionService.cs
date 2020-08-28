using System.Threading.Tasks;

namespace Lambdajection.Encryption
{
    /// <summary>
    /// Describes a service to decrypt values.
    /// </summary>
    public interface IDecryptionService
    {
        /// <summary>
        /// Decrypts a value.
        /// </summary>
        /// <param name="ciphertext">The value to be decrypted.</param>
        /// <returns>The decrypted value.</returns>
        Task<string> Decrypt(string ciphertext);
    }
}
