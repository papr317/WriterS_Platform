using System;

namespace WriterS_Platform.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
        public int WorkID { get; set; } // ID произведения, к которому относится комментарий
        public int UserId { get; set; } // ID пользователя, который оставил комментарий

        // Навигационные свойства
        public Work Work { get; set; }
        public User User { get; set; }
    }
}
