using HeadlessCms.Dtos.Field;
using HeadlessCms.Models;

namespace HeadlessCms.Mappers
{
    public static class FieldMapper
    {
        public static FieldDto ToFieldDto(this Field field)
        {
            return new FieldDto
            {
                Id = field.Id,
                Name = field.Name,
                Type = field.Type,
            };
        }

        public static Field ToCreateField(this CreateFieldRequestDto createFieldDto)
        {
            return new Field
            {
                Name = createFieldDto.Name,
                Type = createFieldDto.Type,
                ContentTypeId = createFieldDto.ContentTypeId
            };
        }
    }
}
