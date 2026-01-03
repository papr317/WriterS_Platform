using WriterS_Platform.Models;

namespace WriterS_Platform.Services // Объявление Namespace!
{
    public interface IUser
    {
        // Регистрация
        Task<int> RegisterUserAsync(User user);

        // Поиск для входа
        Task<User> GetUserByLoginAsync(string identifier, string password);

        // Получение данных профиля (по ID)
        Task<User> GetProfileByIdAsync(int userId);

        // Поиск для системы аутентификации (по ID)
        Task<User> FindUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
}