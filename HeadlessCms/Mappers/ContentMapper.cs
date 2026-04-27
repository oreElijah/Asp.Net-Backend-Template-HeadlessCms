using HeadlessCms.Dtos.Content;
using HeadlessCms.Models;
using HeadlessCms.Mappers;

namespace HeadlessCms.Mappers
{
    public static class ContentMapper
    {
        public static ContentTypeDto ToContentTypeDto(this ContentType content)
        {
            return new ContentTypeDto
            {
                Id = content.Id,
                Name = content.Name,
                Fields = content.Fields.Select(f => f.ToFieldDto()).ToList()
            };
        }

        public static ContentType ToCreateContentType(this CreateContentTypeRequestDto createContentTypeDto)
        {
            return new ContentType
            {
                Name = createContentTypeDto.Name
            };
        }

        public static ContentEntryDto TocontentEntryDto(this ContentEntry contentEntry)
        {
            return new ContentEntryDto
            {
                Id = contentEntry.Id,
                ContentTypeId = contentEntry.ContentTypeId,
                ContentType = contentEntry.ContentType.ToContentTypeDto()
            };
        }

        public static ContentValueResponseDto ToContentValueResponseDto(this ContentValue cv)
        {
            return new ContentValueResponseDto
            {
                Id = cv.Id,
                FieldId = cv.FieldId,
                Value = cv.Value,
                Field = cv.Field.ToFieldDto(),
                ContentEntryId = cv.ContentEntryId,
                ContentEntry = cv.ContentEntry.TocontentEntryDto()
            };
        }
    }
}
