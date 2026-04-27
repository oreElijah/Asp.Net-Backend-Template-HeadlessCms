namespace HeadlessCms.Models
{
    public enum FieldType
    {
        String,
        Integer,
        Date,
        Boolean
    }
    public class Field
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FieldType Type { get; set; }
        public int ContentTypeId { get; set; }
        public ContentType ContentType { get; set; }
    }
}
