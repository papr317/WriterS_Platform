namespace WriterS_Platform.Models
{
    public class WorkList
    {
        public class WorkListViewModel
        {
            public string AuthorID { get; set; }
            public IEnumerable<Work> Works { get; set; } // Список произведений
        }
    }
}