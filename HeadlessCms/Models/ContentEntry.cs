namespace HeadlessCms.Models
{
    public class ContentEntry
    {
        public int Id { get; set; }
        public int ContentTypeId { get; set; }
        public ContentType ContentType { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
