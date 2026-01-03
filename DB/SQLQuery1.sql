create table Users
(
id int identity primary key,
-- изначально в плане логин но это скучно поэтому будут ники
NikeName nvarchar(32),
PasswordHASH varchar(256),
Email varchar(64)
--IsActive int(3)
)