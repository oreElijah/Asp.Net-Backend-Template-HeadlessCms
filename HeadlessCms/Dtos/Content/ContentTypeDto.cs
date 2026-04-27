using HeadlessCms.Dtos.Field;

namespace HeadlessCms.Dtos.Content
{
    public class ContentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<FieldDto> Fields { get; set; } = new List<FieldDto>();

    }
}
