using HeadlessCms.Dtos.Content;

namespace HeadlessCms.Interfaces
{
    public interface IContentEntryRepository
    {
        public Task<ContentEntryDto> AddContentEntry(int ContentTypeId, string appUserId);
        public Task<ContentEntryDto> GetContentEntry(int EntryId);
    }
}
