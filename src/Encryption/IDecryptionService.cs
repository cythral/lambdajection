using System.Threading.Tasks;

namespace Lambdajection.Encryption
{
    public interface IDecryptionService
    {
        Task<string> Decrypt(string ciphertext);
    }
}
