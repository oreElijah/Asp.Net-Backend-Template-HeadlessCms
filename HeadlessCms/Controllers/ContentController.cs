using HeadlessCms.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HeadlessCms.Dtos.Content;
using Microsoft.AspNetCore.Authorization;
using HeadlessCms.Extensions;

namespace HeadlessCms.Controllers
{
    [Route("api/v{version:apiVersion}/content/")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentValueService _cvService;
        public ContentController(IContentValueService cvService)
        {
            _cvService = cvService;
        }

        [ApiVersion("1.0")]
        [HttpGet("Entry/{EntryId}")]
        public async Task<IActionResult> GetAllContentValues(long EntryId)
        {
            if (EntryId > int.MaxValue || EntryId < int.MinValue)
            {
                return NotFound($"No content values found for entry with id.");
            }

            var contentValues = await _cvService.GetContentValues((int)EntryId);

            if (contentValues == null)
            {
                return NotFound($"No content values found for entry with id.");
            }

            return Ok(contentValues);
        }

        [ApiVersion("1.0")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContentValueById(long id)
        {
            if (id > int.MaxValue || id < int.MinValue)
            {
                return NotFound($"Content value with id {id} not found.");
            }

            var contentValue = await _cvService.GetExistingContentValue((int)id);
            if (contentValue == null)
            {
                return NotFound($"Content value with id {id} not found.");
            }
            return Ok(contentValue);
        }

        [ApiVersion("1.0")]
        [HttpPost("{ContentTypeId}")]
        [Authorize]
        public async Task<IActionResult> CreateContentValue(int ContentTypeId, [FromBody] ContentValueRequestDto contentValueDto)
        {
            var userId = User.GetUserId();
            var contentValue = await _cvService.CreateNewContentValue(ContentTypeId, contentValueDto, userId);
            if (contentValue == null)
            {
                return BadRequest("Failed to create content value.");
            }
            return CreatedAtAction(nameof(GetContentValueById), new { id = contentValue.FirstOrDefault()?.ContentEntryId, version = "1.0" }, contentValue);
        }

        [ApiVersion("1.0")]
        [HttpPut("{ContentTypeId}")]
        [Authorize]
        public async Task<IActionResult> UpdateContentValue(int ContentTypeId, [FromBody] ContentValueRequestDto contentValueDto)
        {
            var userId = User.GetUserId();
            var contentValue = await _cvService.UpdateExistingContentValue(ContentTypeId, contentValueDto, userId);
            if (contentValue == null)
            {
                return BadRequest("Failed to update content value.");
            }
            return Ok(contentValue);
        }
        [ApiVersion("1.0")]
        [HttpDelete("{ContentEntryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteContentValue(long ContentEntryId)
        {
            if (ContentEntryId > int.MaxValue || ContentEntryId < int.MinValue)
            {
                return NoContent();
            }

            var userId = User.GetUserId();
            await _cvService.DeleteExistingContentValue((int)ContentEntryId, userId);
            return NoContent();
        }
    }
}
