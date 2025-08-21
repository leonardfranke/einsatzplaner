namespace DTO
{
    public class TokenDTO
    {
        public string Uid { get; set; }
        public string IdToken { get; set; }
        public AuthError? Error { get; set; }

        public enum AuthError
        {
            InvalidPassword,
            EmailAlreadyExists,
            EmailNotFound,
            UserDisabled,
            Unknown
        }
    }
}
