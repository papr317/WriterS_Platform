using Microsoft.AspNetCore.Authentication.Cookies;
using WriterS_Platform.Services; // Импортирует ваш сервис!

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

// РЕГИСТРАЦИЯ: Теперь компилятор знает, что IUser - это UserService
builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddScoped<IWorkService, WorkService>(); // Регистрация IWorkService

// НАСТРОЙКА АУТЕНТИФИКАЦИИ ПО COOKIE
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Register";
        options.AccessDeniedPath = "/AccessDenied";
        options.Cookie.Name = "MyAppAuthCookie";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Work}/{action=Index}/{id?}");

app.Run();