using LearnToDev.Api.Data;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LearnToDev.Api.Repositories;

public class StudyQuestionRepository : IStudyQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public StudyQuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    //The filters are added only when they are given, so the database does the filtering (R-05)
    public async Task<List<StudyQuestion>> SearchAsync(string? text, int? schoolId)
    {
        var query = WithDetails();

        if (!string.IsNullOrWhiteSpace(text))
            //LIKE ignores upper and lower case in SQLite (for a–z)
            query = query.Where(q => EF.Functions.Like(q.Text, $"%{text.Trim()}%"));

        if (schoolId != null)
            query = query.Where(q => q.SchoolId == schoolId);

        return await query.OrderBy(q => q.School.Name).ThenBy(q => q.Id).ToListAsync();
    }

    public async Task<StudyQuestion?> GetByIdAsync(int id)
    {
        return await WithDetails().FirstOrDefaultAsync(q => q.Id == id);
    }

    //All questions in one school with their answer options, used by the exam
    public async Task<List<StudyQuestion>> GetBySchoolAsync(int schoolId)
    {
        return await _context.StudyQuestions
            .Include(q => q.AnswerOptions)
            .Where(q => q.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<StudyQuestion> AddAsync(StudyQuestion question)
    {
        _context.StudyQuestions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    //Replaces the text, the school and all the answer options
    public async Task<bool> UpdateAsync(StudyQuestion question)
    {
        var existing = await _context.StudyQuestions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == question.Id);
        if (existing == null)
            return false;

        existing.SchoolId = question.SchoolId;
        existing.Text = question.Text;
        existing.Explanation = question.Explanation;

        _context.AnswerOptions.RemoveRange(existing.AnswerOptions);
        existing.AnswerOptions = question.AnswerOptions;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.StudyQuestions.FindAsync(id);
        if (existing == null)
            return false;

        _context.StudyQuestions.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    //The question with its school (for the name) and answer options
    private IQueryable<StudyQuestion> WithDetails()
    {
        return _context.StudyQuestions
            .Include(q => q.School)
            .Include(q => q.AnswerOptions);
    }
}
