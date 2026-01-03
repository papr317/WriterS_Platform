
CREATE PROC pRegisterUser
    -- Входные параметры (в контроллере передаются из C#)
    @NikeName NVARCHAR(32),
    @PasswordHash VARCHAR(256), -- Хеш пароля, полученный из BCrypt/Identity
    @Email VARCHAR(64)
AS
BEGIN

    DECLARE @NewUserId INT;
    
    -- 1. ПРОВЕРКА НА СУЩЕСТВОВАНИЕ (Дубликат)
    IF EXISTS (SELECT 1 FROM Users WHERE NikeName = @NikeName OR Email = @Email)
    BEGIN
        -- Если пользователь с таким ником или email уже существует,
        -- возвращаем 0, чтобы сигнализировать об ошибке в C# коде.
        SELECT 0 AS NewUserId;
        RETURN;
    END

    -- 2. ВСТАВКА НОВОГО ПОЛЬЗОВАТЕЛЯ
    INSERT INTO Users (NikeName, PasswordHASH, Email)
    VALUES (@NikeName, @PasswordHash, @Email);
    
    -- 3. ВОЗВРАТ ID
    -- Получаем автоматически сгенерированный ID новой записи
    SET @NewUserId = SCOPE_IDENTITY();
    
    -- Возвращаем ID нового пользователя (это будет > 0)
    SELECT @NewUserId AS NewUserId;
END
GO