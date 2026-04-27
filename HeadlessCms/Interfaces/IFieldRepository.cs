using HeadlessCms.Data;
using HeadlessCms.Dtos.Field;

namespace HeadlessCms.Interfaces
{
    public interface IFieldRepository
    {
        public Task<List<FieldDto>> GetAllField();
        public Task<FieldDto> GetFieldById(int id);
        public Task<List<FieldDto>> GetFieldsForContentType(int contentTypeId);
        public Task<FieldDto> CreateField(CreateFieldRequestDto createFieldDto);
        public Task DeleteField(int id);

        public Task<bool> FieldExists(string name, int contentTypeId);
    }
}
