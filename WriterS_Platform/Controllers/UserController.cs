using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WriterS_Platform.Models;
using WriterS_Platform.Services; // Импортирует ваш IUser и другие сервисы
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WriterS_Platform.ViewModels; // Добавляем using для доступа к ViewModels

public class UserController : Controller
{
    private readonly IUser _userService;

    public UserController(IUser userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Route("register/")]
    public IActionResult RegisterGet()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Profile");
        }
        return View("Register"); // Ищет View в /Views/User/Register.cshtml
    }

    // POST: /User/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Хеширование пароля
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var userEntity = new WriterS_Platform.Models.User
            {
                NikeName = model.NikeName,
                Email = model.Email,
                PasswordHASH = hashedPassword
            };

            int newUserId = await _userService.RegisterUserAsync(userEntity);

            if (newUserId > 0)
            {
                // АВТОМАТИЧЕСКИЙ ВХОД СРАЗУ ПОСЛЕ РЕГИСТРАЦИИ
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, newUserId.ToString()),
                    new Claim(ClaimTypes.Name, userEntity.NikeName)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Profile", "User");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким никнеймом или Email уже существует.");
            }
        }
        return View(model);
    }

    [HttpGet]
    [Route("login")]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Profile");
        }
        return View();
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userService.GetUserByLoginAsync(model.Identifier, model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                    new Claim(ClaimTypes.Name, user.NikeName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Profile", "User");
            }
            ModelState.AddModelError(string.Empty, "Неверный никнейм/Email или пароль.");
        }
        return View(model);
    }

    [HttpGet]
    [Route("profile")]
    [Authorize]
    public async Task<IActionResult> Profile() // Делаем метод асинхронным
    {
        // 1. Получаем ID текущего пользователя
        // ClaimTypes.NameIdentifier содержит ID, который мы сохранили при входе
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            // Если ID не найден или некорректен, перенаправляем на вход
            return RedirectToAction("Register");
        }

        // 2. Вызываем сервис для получения всех данных профиля
        var userProfile = await _userService.GetProfileByIdAsync(userId);

        if (userProfile == null)
        {
            // Пользователь не найден в БД, но есть куки. Лучше разлогинить.
            return RedirectToAction("Logout");
        }

        // 3. Передаем модель (сущность User) в представление
        return View(userProfile); // Передаем модель!
    }

    [HttpGet]
    [Route("profile/edit")]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return RedirectToAction("Login");
        }

        var user = await _userService.GetProfileByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Logout");
        }

        var model = new EditProfileViewModel
        {
            Id = user.id,
            NikeName = user.NikeName,
            Email = user.Email
        };

        return View(model);
    }

    [HttpPost]
    [Route("profile/edit")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId) || userId != model.Id)
            {
                ModelState.AddModelError(string.Empty, "Ошибка авторизации.");
                return View(model);
            }

            var user = await _userService.GetProfileByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            user.NikeName = model.NikeName;
            user.Email = model.Email;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHASH = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            var updateSuccessful = await _userService.UpdateUserAsync(user);
            if (updateSuccessful)
            {
                // Если никнейм изменился, обновим ClaimTypes.Name
                if (User.Identity.Name != model.NikeName)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                        new Claim(ClaimTypes.Name, user.NikeName)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                }
                return RedirectToAction("Profile");
            }
            ModelState.AddModelError(string.Empty, "Не удалось обновить профиль.");
        }
        return View(model);
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Work"); // Перенаправляем на главную страницу
    }

    [HttpPost]
    [Route("profile/delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return RedirectToAction("Login");
        }

        // Здесь важно: Согласно требованиям, произведения и комментарии должны остаться.
        // Мы просто удаляем пользователя из таблицы Users.
        var deleteSuccessful = await _userService.DeleteUserAsync(userId);
        if (deleteSuccessful)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Выход из системы
            return RedirectToAction("RegisterGet"); // Перенаправляем на страницу регистрации
        }
        // Если удаление не удалось, можно перенаправить на страницу профиля с сообщением об ошибке
        return RedirectToAction("Profile");
    }
}