using ChoSV.Models.DTOs.University;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UniversityController : ControllerBase
    {
        private readonly IUniversityService _universityService;
        public UniversityController(IUniversityService universityService) => _universityService = universityService;

        [HttpGet]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> GetAllUniversityAsync([FromQuery] int page, [FromQuery] int pageSize)
        {
            var universities = await _universityService.GetAllUniversityAsync(page, pageSize);
            return Ok(universities);
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AddUniversityAsync(AddUniversityDTO addUniversityDTO)
        {
            var university = await _universityService.AddUniversityAsync(addUniversityDTO);
            return Ok(university);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> UpdateUniversityAsync([FromRoute] int id, UpdateUniversityDTO updateUniversityDTO)
        {
            var university = await _universityService.UpdateUniversityAsync(id, updateUniversityDTO);
            return Ok(university);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteUniversityAsync([FromRoute] int id)
        {
            await _universityService.DeleteUniversityAsync(id);
            return Ok();
        }
    }
}
