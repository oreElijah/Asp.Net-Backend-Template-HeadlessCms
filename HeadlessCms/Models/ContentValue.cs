namespace HeadlessCms.Models
{
    public class ContentValue
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public int FieldId { get; set; }
        public Field Field { get; set; }
        public int ContentEntryId { get; set; }
        public ContentEntry ContentEntry { get; set; } 
    }
}
