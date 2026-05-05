using System.ComponentModel.DataAnnotations;
using HeadlessCms.Models;

namespace HeadlessCms.Dtos.Field
{
    public class CreateFieldWithContentTypeDto
    {
            [Required]
            public string Name { get; set; }
            [Required]
            public FieldType Type { get; set; }
    }
}