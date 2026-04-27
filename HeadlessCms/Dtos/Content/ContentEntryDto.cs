namespace HeadlessCms.Dtos.Content
{
    public class ContentEntryDto
    {
        public int Id { get; set; }
        public int ContentTypeId { get; set; }
        public ContentTypeDto ContentType { get; set; }
    }
}
