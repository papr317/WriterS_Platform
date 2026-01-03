using System.Collections.Generic;
using WriterS_Platform.Models;

namespace WriterS_Platform.ViewModels
{
    public class WorkDetailsViewModel
    {
        public WorkViewModel Work { get; set; }
        public IEnumerable<CommentViewModel> Comments { get; set; }
        public int CurrentUserRating { get; set; } // Оценка текущего пользователя (-1, если нет)
        public CommentViewModel NewComment { get; set; } = new CommentViewModel(); // Для формы комментария
        public RatingViewModel NewRating { get; set; } = new RatingViewModel(); // Для формы оценки
    }
}
