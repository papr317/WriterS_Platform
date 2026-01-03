using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using WriterS_Platform.Models;
using WriterS_Platform.ViewModels;

namespace WriterS_Platform.Services
{
    public class WorkService : IWorkService
    {
        private readonly IConfiguration _config;
        private string ConnectionString => _config["db"];

        public WorkService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task<IEnumerable<WorkViewModel>> GetAllWorksAsync()
        {
            // Теперь просто вызываем SearchWorksAsync без параметров поиска/сортировки/пагинации
            return await SearchWorksAsync(null, null, null, 1, int.MaxValue);
        }

        public async Task<IEnumerable<WorkViewModel>> SearchWorksAsync(string searchTerm, string genre, string sortBy, int pageNumber, int pageSize)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = new System.Text.StringBuilder();
                sql.Append(@"SELECT w.WorkID, w.Title, w.Content, w.Genre, w.PublicationDate, w.AvgRating as CurrentRating,
                                    u.NikeName as AuthorNikeName,
                                    (SELECT COUNT(*) FROM Comments WHERE WorkID = w.WorkID) as CommentsCount
                              FROM Works w
                              JOIN Users u ON w.AuthorID = u.id");

                var conditions = new List<string>();
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    conditions.Add("(w.Title LIKE '%' + @SearchTerm + '%' OR u.NikeName LIKE '%' + @SearchTerm + '%')");
                    parameters.Add("SearchTerm", searchTerm);
                }
                if (!string.IsNullOrWhiteSpace(genre))
                {
                    conditions.Add("w.Genre = @Genre");
                    parameters.Add("Genre", genre);
                }

                if (conditions.Any())
                {
                    sql.Append(" WHERE " + string.Join(" AND ", conditions));
                }

                // Сортировка
                string orderByClause = " ORDER BY w.PublicationDate DESC"; // По умолчанию
                switch (sortBy?.ToLower())
                {
                    case "ratingdesc":
                        orderByClause = " ORDER BY w.AvgRating DESC";
                        break;
                    case "ratingasc":
                        orderByClause = " ORDER BY w.AvgRating ASC";
                        break;
                    case "newest":
                        orderByClause = " ORDER BY w.PublicationDate DESC";
                        break;
                    case "oldest":
                        orderByClause = " ORDER BY w.PublicationDate ASC";
                        break;
                    case "titleasc":
                        orderByClause = " ORDER BY w.Title ASC";
                        break;
                    case "titledesc":
                        orderByClause = " ORDER BY w.Title DESC";
                        break;
                    case "commentsdesc":
                        orderByClause = " ORDER BY (SELECT COUNT(*) FROM Comments WHERE WorkID = w.WorkID) DESC";
                        break;
                }
                sql.Append(orderByClause);

                // Пагинация
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", (pageNumber - 1) * pageSize);
                parameters.Add("PageSize", pageSize);


                return await connection.QueryAsync<WorkViewModel>(sql.ToString(), parameters);
            }
        }

        public async Task<int> GetTotalWorksCountAsync(string searchTerm, string genre)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("SearchTerm", searchTerm);
                parameters.Add("Genre", genre);

                return await connection.QuerySingleAsync<int>(
                    "pGetTotalWorksCount",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
        }
        public async Task<WorkViewModel> GetWorkByIdAsync(int id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT w.WorkID, w.Title, w.Content, w.Genre, w.PublicationDate, w.AvgRating as CurrentRating, 
                                    u.NikeName as AuthorNikeName, 
                                    (SELECT COUNT(*) FROM Comments WHERE WorkId = w.WorkID) as CommentsCount
                              FROM Works w
                              JOIN Users u ON w.AuthorID = u.id
                             WHERE w.WorkID = @Id";
                return await connection.QuerySingleOrDefaultAsync<WorkViewModel>(sql, new { Id = id });
            }
        }

        public async Task<int> CreateWorkAsync(Work work)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var parameters = new
                {
                    work.AuthorID,
                    work.Title,
                    work.Genre,
                    work.PublicationDate,
                    work.Content
                };
                var newWorkID = await connection.QuerySingleAsync<int>(
                    "pWork",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return newWorkID;
            }
        }

        public async Task<bool> UpdateWorkAsync(Work work)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = @"UPDATE Works
                              SET Title = @Title, Content = @Content, Genre = @Genre, AvgRating = @AvgRating
                            WHERE WorkID = @WorkID";
                var rowsAffected = await connection.ExecuteAsync(sql, work);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteWorkAsync(int id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = "DELETE FROM Works WHERE WorkID = @Id";
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
        }

        public async Task<IEnumerable<WorkViewModel>> GetWorksByAuthorAsync(int authorId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var sql = @"SELECT w.WorkID, w.Title, w.Content, w.Genre, w.PublicationDate, w.AvgRating as CurrentRating, 
                                    u.NikeName as AuthorNikeName, 
                                    (SELECT COUNT(*) FROM Comments WHERE WorkId = w.WorkID) as CommentsCount
                              FROM Works w
                              JOIN Users u ON w.AuthorID = u.id
                             WHERE w.AuthorID = @AuthorId";
                return await connection.QueryAsync<WorkViewModel>(sql, new { AuthorId = authorId });
            }
        }

        public async Task<IEnumerable<CommentViewModel>> GetCommentsByWorkIdAsync(int workId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                return await connection.QueryAsync<CommentViewModel>(
                    "pGetCommentsByWorkId",
                    new { WorkID = workId },
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
        }

        public async Task<int> AddCommentAsync(Comment comment)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var parameters = new
                {
                    comment.WorkID,
                    comment.UserId,
                    comment.Content,
                    comment.CommentDate
                };
                var newCommentID = await connection.QuerySingleAsync<int>(
                    "pAddComment",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return newCommentID;
            }
        }

        public async Task<bool> AddRatingAsync(Rating rating)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var parameters = new
                {
                    rating.WorkID,
                    rating.UserId,
                    rating.Value
                };
                var rowsAffected = await connection.QuerySingleAsync<int>(
                    "pAddRating",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return rowsAffected > 0;
            }
        }

        public async Task<int> GetUserRatingForWorkAsync(int workId, int userId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var parameters = new { WorkID = workId, UserId = userId };
                var ratingValue = await connection.QuerySingleOrDefaultAsync<int>(
                    "pGetUserRatingForWork",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return ratingValue;
            }
        }

        public async Task<bool> UpdateWorkAvgRatingAsync(int workId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var parameters = new { WorkID = workId };
                var rowsAffected = await connection.ExecuteAsync(
                    "pUpdateWorkAvgRating",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return rowsAffected > 0;
            }
        }
    }
}
