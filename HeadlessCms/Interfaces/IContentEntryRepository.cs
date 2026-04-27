using HeadlessCms.Dtos.Content;

namespace HeadlessCms.Interfaces
{
    public interface IContentEntryRepository
    {
        public Task<ContentEntryDto> AddContentEntry(int ContentTypeId);
        public Task<ContentEntryDto> GetContentEntry(int EntryId);
    }
}
