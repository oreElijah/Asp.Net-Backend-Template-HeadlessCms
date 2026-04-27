using HeadlessCms.Dtos.Field;
using HeadlessCms.Models;

namespace HeadlessCms.Dtos.Content
{
    public class ContentValueResponseDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public int FieldId { get; set; }
        public FieldDto Field { get; set; }
        public int ContentEntryId { get; set; }
        public ContentEntryDto ContentEntry { get; set; }
    }
}
