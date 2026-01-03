using WriterS_Platform.Models;
using WriterS_Platform.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WriterS_Platform.Services
{
    public interface IWorkService
    {
        Task<IEnumerable<WorkViewModel>> GetAllWorksAsync();
        Task<WorkViewModel> GetWorkByIdAsync(int id);
        Task<int> CreateWorkAsync(Work work);
        Task<bool> UpdateWorkAsync(Work work);
        Task<bool> DeleteWorkAsync(int id);
        Task<IEnumerable<WorkViewModel>> GetWorksByAuthorAsync(int authorId);
        Task<IEnumerable<CommentViewModel>> GetCommentsByWorkIdAsync(int workId);
        Task<int> AddCommentAsync(Comment comment);
        Task<bool> AddRatingAsync(Rating rating);
        Task<int> GetUserRatingForWorkAsync(int workId, int userId);
        Task<bool> UpdateWorkAvgRatingAsync(int workId);
        Task<IEnumerable<WorkViewModel>> SearchWorksAsync(string searchTerm, string genre, string sortBy, int pageNumber, int pageSize);
        Task<int> GetTotalWorksCountAsync(string searchTerm, string genre);
    }
}
