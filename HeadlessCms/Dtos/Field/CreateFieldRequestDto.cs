using System.ComponentModel.DataAnnotations;
using HeadlessCms.Models;

namespace HeadlessCms.Dtos.Field
{
    public class CreateFieldRequestDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public FieldType Type { get; set; }
        [Required]
        public int ContentTypeId { get; set; }
    }
}
