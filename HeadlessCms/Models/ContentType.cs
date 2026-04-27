namespace HeadlessCms.Models
{
    public class ContentType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Field> Fields { get; set; } = new List<Field>();
    }
}
