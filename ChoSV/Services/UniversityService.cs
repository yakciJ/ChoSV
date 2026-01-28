using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.University;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class UniversityService : IUniversityService
    {
        private readonly ApplicationDBContext _dbContext;

        public UniversityService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<UniversityDTO>> GetAllUniversityAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            int skip = (page - 1) * pageSize;

            var totalCount = await _dbContext.Universities.CountAsync();

            var universities = await _dbContext.Universities
                .AsNoTracking()
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            var universityDTOs = universities.Select(u => u.ToUniversityDTOFromUniversity()).ToList();

            return new PagedResult<UniversityDTO>
            {
                Items = universityDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<UniversityDTO> AddUniversityAsync(AddUniversityDTO addUniversityDTO)
        {
            var university = new University
            {
                UniversityName = addUniversityDTO.UniversityName,
                UniversityEmail = addUniversityDTO.UniversityEmail,
                UniversityLogo = addUniversityDTO.UniversityLogo,
            };
            await _dbContext.Universities.AddAsync(university);
            await _dbContext.SaveChangesAsync();
            return university.ToUniversityDTOFromUniversity();
        }

        public async Task<UniversityDTO> UpdateUniversityAsync(int id, UpdateUniversityDTO updateUniversityDTO)
        {
            var university = await _dbContext.Universities.FirstOrDefaultAsync(u => u.UniversityId == id);
            if (university == null)
            {
                throw new ArgumentException("Trường học không tồn tại!");
            }
            if (!string.IsNullOrEmpty(updateUniversityDTO.UniversityName))
                university.UniversityName = updateUniversityDTO.UniversityName;

            if (!string.IsNullOrEmpty(updateUniversityDTO.UniversityEmail))
                university.UniversityEmail = updateUniversityDTO.UniversityEmail;

            if (!string.IsNullOrEmpty(updateUniversityDTO.UniversityLogo))
                university.UniversityLogo = updateUniversityDTO.UniversityLogo;

            var res = await _dbContext.SaveChangesAsync();
            return university.ToUniversityDTOFromUniversity();
        }

        public async Task DeleteUniversityAsync(int id)
        {
            var university = await _dbContext.Universities.FirstOrDefaultAsync(u => u.UniversityId == id);
            if (university == null)
            {
                throw new ArgumentException("Trường học không tồn tại!");
            }
            _dbContext.Universities.Remove(university);
            await _dbContext.SaveChangesAsync();
        }
    }
}
