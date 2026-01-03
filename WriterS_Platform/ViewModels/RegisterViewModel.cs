namespace WriterS_Platform.ViewModels
{
    public class RegisterViewModel
    {
        public int id { get; set; }

        // Поле, которое будет отображаться в форме
        public string NikeName { get; set; }
        // !!! ЭТО ПОЛЕ ПРИНИМАЕТ ПАРОЛЬ В ЧИСТОМ ВИДЕ С ФОРМЫ !!!
        public string Password { get; set; }
        // поле для повторного ввода пароля для подтверждения
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
    }
}
