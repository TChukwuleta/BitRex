namespace BitRex.Application.Common.Interfaces
{
    public interface IEncryptionService
    {
        string EncryptData(string request);
        string DecryptData(string request);
    }
}
