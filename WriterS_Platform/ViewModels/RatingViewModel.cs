using System.ComponentModel.DataAnnotations;

namespace WriterS_Platform.ViewModels
{
    public class RatingViewModel
    {
        public int WorkID { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Оценка должна быть от 0 до 100.")]
        [Range(0, 100, ErrorMessage = "Оценка должна быть от 0 до 100.")]
        public int Value { get; set; }
    }
}
