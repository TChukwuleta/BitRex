using BitRex.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace BitRex.Infrastructure.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IConfiguration _config;
        public EncryptionService(IConfiguration config)
        {
            _config = config;
        }

        public string DecryptData(string request)
        {
            var salt = _config["Encryption:SaltValue"];
            var passPhrase = _config["Encryption:PassPhrase"];
            var blockSize = _config["Encryption:Blocksize"];
            var Iv = _config["Encryption:IV"];
            var passwordIteration = _config["Encryption:PasswordIteration"];
            try
            {
                var saltValueBytes = Encoding.ASCII.GetBytes(salt);
                var password = new Rfc2898DeriveBytes(passPhrase, saltValueBytes, int.Parse(passwordIteration));
                var keyBytes = password.GetBytes(int.Parse(blockSize));
                var symmetricKey = new RijndaelManaged();
                var initVectorBytes = Encoding.ASCII.GetBytes(Iv);
                var encryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

                ICryptoTransform decryptor = encryptor;
                byte[] buffer = Convert.FromBase64String(request);
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    using (CryptoStream cs = new CryptoStream((Stream)ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cs))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string EncryptData(string request)
        {
            var salt = _config["Encryption:SaltValue"];
            var passPhrase = _config["Encryption:PassPhrase"];
            var blockSize = _config["Encryption:Blocksize"];
            var iteration = _config["Encryption:PasswordIteration"];
            var Iv = _config["Encryption:IV"];
            try
            {
                var saltValueBytes = Encoding.ASCII.GetBytes(salt);
                var password = new Rfc2898DeriveBytes(passPhrase, saltValueBytes, int.Parse(iteration));
                var keyBytes = password.GetBytes(int.Parse(blockSize));
                var symmetricKey = new RijndaelManaged();
                var initVectorBytes = Encoding.ASCII.GetBytes(Iv);
                var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                var plainTextBytes = Encoding.UTF8.GetBytes(request);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                var cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                var cipherText = Convert.ToBase64String(cipherTextBytes);
                return cipherText;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
