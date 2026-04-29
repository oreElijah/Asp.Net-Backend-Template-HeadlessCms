namespace HeadlessCms.Dtos.Field
{
    public class CreateFieldWithFieldTypeStringDto
    {
            [Required]
            public string Name { get; set; }
            [Required]
            public string Type { get; set; }
            [Required]
            public int ContentTypeId { get; set; }
    }
}