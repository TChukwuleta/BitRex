namespace BitRex.Core.Model.Response
{
    public class PaystackInitializationResponse
    {
        public string AuthorizationCode { get; set; }
        public string Reference { get; set; }
        public string Url { get; set; }
        public string ErrorMessage { get; set; }
    }
}
