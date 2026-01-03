namespace WriterS_Platform.Models
{
    public class User
    {
        public int id { get; set; }
        // логина не будет, будет никнейм
        public string NikeName { get; set; }
        // пароль будет храниться в зашифрованном виде в бд
        public string PasswordHASH { get; set; }
        public string Email { get; set; }
    }
}
