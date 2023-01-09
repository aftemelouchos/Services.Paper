using GenericRepositoryManager;

namespace Services.Paper.Model
{
    public class Paper: BaseEntity
    {
        public Guid   UserId { get; set; }
        public string ScopeName { get; set; }
        public string PaperType { get; set; }
        public string PaperName { get; set; }
        public string Authors { get; set; }
        public int    AuthorCount { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string PaperLang { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string PublicationStatusName { get; set; }
        public string PublicationType { get; set; }
        public string FirstPage { get; set; }
        public string LastPage { get; set; }
        public string PaperPresentationTypeName { get; set; }
        public string PaperUrl { get; set; }
        public string Fields { get; set; }
    }
}
