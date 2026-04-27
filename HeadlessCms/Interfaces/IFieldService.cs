using HeadlessCms.Dtos.Field;

namespace HeadlessCms.Interfaces
{
    public interface IFieldService
    {
        public Task<List<FieldDto>> GetFields();
        public Task<FieldDto> GetUniqueField(int id);
        public Task<List<FieldDto>> GetExistingFieldsForContentType(int contentTypeId);
        public Task<FieldDto> CreateNewField(CreateFieldRequestDto createFieldDto);
        public Task DeleteExistingField(int id);
    }
}
