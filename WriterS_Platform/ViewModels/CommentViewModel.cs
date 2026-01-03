using System.ComponentModel.DataAnnotations;
using System;

namespace WriterS_Platform.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public int WorkID { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage = "Комментарий не может быть пустым.")]
        [StringLength(1000, ErrorMessage = "Комментарий не может превышать 1000 символов.")]
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
    }
}
