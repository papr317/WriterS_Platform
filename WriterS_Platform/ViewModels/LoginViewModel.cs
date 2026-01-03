using System.ComponentModel.DataAnnotations;

namespace WriterS_Platform.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите никнейм или Email.")]
        public string Identifier { get; set; } // Может быть никнеймом или Email

        [Required(ErrorMessage = "Пожалуйста, введите пароль.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
