using System;
using System.Collections.Generic;

namespace WriterS_Platform.Models
{
    public class Work
    {
        public int WorkID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; } // Содержание произведения
        public string Genre { get; set; }
        public DateTime PublicationDate { get; set; }
        public int AvgRating { get; set; } // От 0 до 100
        public int AuthorID { get; set; } // ID пользователя, который опубликовал произведение

        // Навигационные свойства (для Entity Framework Core, но полезно для понимания связей)
        public User Author { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Rating> Ratings { get; set; }
    }
}
