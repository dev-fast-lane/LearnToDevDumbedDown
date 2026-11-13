using LearnToDev.Api.Models;

namespace LearnToDev.Api.Repositories.Interfaces;

public interface IStudyQuestionRepository
{
    //Search on text and school. Both parameters are optional (R-05)
    Task<List<StudyQuestion>> SearchAsync(string? text, int? schoolId);
    Task<StudyQuestion?> GetByIdAsync(int id);
    Task<List<StudyQuestion>> GetBySchoolAsync(int schoolId);
    Task<StudyQuestion> AddAsync(StudyQuestion question);
    Task<bool> UpdateAsync(StudyQuestion question);
    Task<bool> DeleteAsync(int id);
}
