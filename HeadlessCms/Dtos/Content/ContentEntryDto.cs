using System.ComponentModel.DataAnnotations;

namespace HeadlessCms.Dtos.Content
{
    public class ContentEntryDto
    {
        public int Id { get; set; }
        [Required]
        public int ContentTypeId { get; set; }
        public ContentTypeDto ContentType { get; set; }
    }
}
