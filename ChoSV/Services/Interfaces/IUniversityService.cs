using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.University;

namespace ChoSV.Services.Interfaces
{
    public interface IUniversityService
    {
        Task<UniversityDTO> AddUniversityAsync(AddUniversityDTO addUniversityDTO);
        Task<PagedResult<UniversityDTO>> GetAllUniversityAsync(int page, int pageSize);
        Task<UniversityDTO> UpdateUniversityAsync(int id, UpdateUniversityDTO updateUniversityDTO);
        Task DeleteUniversityAsync(int id);
    }
}
