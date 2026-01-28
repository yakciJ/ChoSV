using ChoSV.Models.DTOs.University;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class UniversityMapper
    {
        public static UniversityDTO ToUniversityDTOFromUniversity(this University university)
        {
            return new UniversityDTO
            {
                UniversityId = university.UniversityId,
                UniversityName = university.UniversityName,
                UniversityEmail = university.UniversityEmail,
                UniversityLogo = university.UniversityLogo
            };
        }
    }
}
