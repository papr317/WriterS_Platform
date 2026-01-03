using System.ComponentModel.DataAnnotations;

namespace WriterS_Platform.ViewModels
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите никнейм.")]
        [StringLength(50, ErrorMessage = "Никнейм не может превышать 50 символов.")]
        public string NikeName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите Email.")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email.")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmNewPassword { get; set; }
    }
}


