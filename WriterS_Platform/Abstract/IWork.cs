//// Services/IWorkService.cs
//using WriterS_Platform.Models;

//public interface IWork
//{
//    Task<IEnumerable<Work>> GetWorksByAuthorIdAsync(int authorId);
//    // ...
//}

//// UserService (или WorkService)
//public async Task<IEnumerable<Work>> GetWorksByAuthorIdAsync(int authorId)
//{
//    // ... логика подключения к БД и выполнение SELECT * FROM Work WHERE AuthorID = @authorId
//    // с использованием Dapper или Entity Framework
//    return await connection.QueryAsync<Work>(
//        "SELECT * FROM Works WHERE AuthorID = @Id",
//        new { Id = authorId }
//    );
//}