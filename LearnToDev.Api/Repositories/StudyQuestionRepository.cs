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

    public async Task<List<StudyQuestion>> GetAllAsync()
    {
        return await WithDetails().OrderBy(q => q.School.Name).ThenBy(q => q.Id).ToListAsync();
    }

    public async Task<StudyQuestion?> GetByIdAsync(int id)
    {
        return await WithDetails().FirstOrDefaultAsync(q => q.Id == id);
    }

    //Admin search (#25, R-05). Both filters are optional, and the filtering runs in the database.
    //ToLower on both sides ignores case. Note: SQLite's lower() only knows A-Z, so Æ/Ø/Å still differ from æ/ø/å
    public async Task<List<StudyQuestion>> SearchAsync(string? search, int? schoolId)
    {
        var query = WithDetails();

        if (schoolId.HasValue)
            query = query.Where(q => q.SchoolId == schoolId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(q => q.Text.ToLower().Contains(term));
        }

        return await query
            .OrderBy(q => q.School.Name)
            .ThenBy(q => q.Id)
            .ToListAsync();
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
