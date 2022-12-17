namespace Library.Database.Auth.Models
{
    public class TokenViewModel
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public string expiration { get; set; }
    }
}
