namespace BitRex.Application.Common.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> FromBase64ToFile(string base64File, string filename);
        Task<string> UploadImage(string base64string, string userid);
    }
}
