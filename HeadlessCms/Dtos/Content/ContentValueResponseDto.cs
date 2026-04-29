using HeadlessCms.Dtos.Field;
using HeadlessCms.Models;

namespace HeadlessCms.Dtos.Content
{
    public class ContentValueResponseDto
    {
        public int Id { get; set; }
        [Required]
        public string Value { get; set; } = string.Empty;
        [Required]
        public int FieldId { get; set; }
        public FieldDto Field { get; set; }
        [Required]
        public int ContentEntryId { get; set; }
        public ContentEntryDto ContentEntry { get; set; }
    }
}
