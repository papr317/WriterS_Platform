using System.ComponentModel.DataAnnotations;
using WriterS_Platform.Models;

namespace WriterS_Platform.ViewModels
{
    public class WorkViewModel
    {
        public int WorkID { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите название произведения.")]
        [StringLength(100, ErrorMessage = "Название не может превышать 100 символов.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите содержание произведения.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Пожалуйста, выберите жанр.")]
        public string Genre { get; set; }

        // Дополнительные поля для отображения (например, имя автора, средний рейтинг)
        public string AuthorNikeName { get; set; }
        public DateTime PublicationDate { get; set; }
        public int AvgRating { get; set; }
        public int CommentsCount { get; set; }
    }
}
