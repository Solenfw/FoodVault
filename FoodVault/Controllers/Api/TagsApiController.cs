using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Controllers.Api
{
    [ApiController]
    [Route("api/tags")]
    public class TagsApiController : ControllerBase
    {
        private readonly FoodVaultDbContext _context;

        public TagsApiController(FoodVaultDbContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var tags = await _context.Tags
                .Select(t => new { id = t.Id, name = t.Name })
                .ToListAsync();
            return Ok(tags);
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { error = "Tên tag không được để trống." });
            }

            try
            {
                // Check if tag already exists
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower());

                if (existingTag != null)
                {
                    return Ok(new { id = existingTag.Id, name = existingTag.Name, message = "Tag đã tồn tại." });
                }

                // Create new tag
                var tag = new Tag
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name.Trim()
                };

                await _context.Tags.AddAsync(tag);
                await _context.SaveChangesAsync();

                return Ok(new { id = tag.Id, name = tag.Name, message = "Tag đã được tạo thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Có lỗi xảy ra khi tạo tag." });
            }
        }

        public class CreateTagRequest
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}


