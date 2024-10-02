namespace Utilities.Data
{
    [System.Serializable]
    public record LoginData(string AuthToken, string RefreshToken, string Email, string Password)
    {
        public string AuthToken { get; set; } = AuthToken;

        public string RefreshToken { get; set; } = RefreshToken;

        public string Email { get; } = Email;

        public string Password { get; } = Password;
    }
}
