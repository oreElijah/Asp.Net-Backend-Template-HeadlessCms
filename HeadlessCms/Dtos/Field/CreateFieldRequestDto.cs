using HeadlessCms.Models;

namespace HeadlessCms.Dtos.Field
{
    public class CreateFieldRequestDto
    {
        public string Name { get; set; }
        public FieldType Type { get; set; }
        public int ContentTypeId { get; set; }
    }
}
