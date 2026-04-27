using HeadlessCms.Models;
using HeadlessCms.Dtos.Content;

namespace HeadlessCms.Dtos.Field
{
    public class FieldDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FieldType Type { get; set; }
    }
}
