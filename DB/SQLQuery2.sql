create Table Works
(
WorkID int primary key,
AuthorID int,
Title NVARCHAR (256),
Genre NVARCHAR (64),
PublicationDate	DATETIME,
Content	TEXT,
-- Средний рейтинг	Диапазон 0-100 (рассчитывается автоматически).
AvgRating INT
)