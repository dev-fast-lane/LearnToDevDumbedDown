using LearnToDev.Api.Models;

namespace LearnToDev.Api.Repositories.Interfaces;

public interface IStudyQuestionRepository
{
    Task<List<StudyQuestion>> GetAllAsync();
    Task<StudyQuestion?> GetByIdAsync(int id);
    Task<List<StudyQuestion>> GetBySchoolAsync(int schoolId);
    Task<StudyQuestion> AddAsync(StudyQuestion question);
    Task<bool> UpdateAsync(StudyQuestion question);
    Task<bool> DeleteAsync(int id);
}
