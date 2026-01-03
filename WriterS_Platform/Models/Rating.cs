namespace WriterS_Platform.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int Value { get; set; } // Оценка от 0 до 100
        public int WorkID { get; set; }
        public int UserId { get; set; }

        // Навигационные свойства
        public Work Work { get; set; }
        public User User { get; set; }
    }
}
