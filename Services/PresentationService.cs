using Microsoft.EntityFrameworkCore;
using PresentationMeetup.Data;

namespace PresentationMeetup.Services;

public class PresentationService
{
    private readonly PresentationDbContext _context;

    public PresentationService(PresentationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreatePresentation(Presentation model)
    {
        //var presentation = new Presentation
        //{
        //    Title = model.Title,
        //    CreatedDate = DateTime.UtcNow,
        //    UpdatedDate = DateTime.UtcNow,
        //    Creator = model.Creator,
        //};

        _context.Presentations.Add(model);
        await _context.SaveChangesAsync();

        return model.PresentationId;
    }

    public async Task<int> CreateUserRole(UserRole model)
    {
        _context.UserRoles.Add(model);
        await _context.SaveChangesAsync();
        return model.UserRoleId;
    }

    public async Task<int> CreateSlide(Slide model)
    {
        _context.Slides.Add(model);
        await _context.SaveChangesAsync();
        return model.SlideId;
    }

    // Add an editor by NickName to the presentation
    public async Task<bool> AddEditor(int presentationId, string editorNickName)
    {
        var presentation = await _context.Presentations.FindAsync(presentationId);
        if (presentation == null)
        {
            return false;
        }

        if (presentation.Creator != editorNickName && !presentation.Editors.Contains(editorNickName))
        {
            presentation.Editors.Add(editorNickName);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    // Check if a user with NickName can edit a presentation
    public async Task<bool> CanEdit(int presentationId, string userNickName)
    {
        var presentation = await _context.Presentations
            .FirstOrDefaultAsync(p => p.PresentationId == presentationId);

        if (presentation == null)
        {
            return false;
        }

        return presentation.Creator == userNickName || presentation.Editors.Contains(userNickName);
    }

    public async Task<List<Presentation>> GetMyPresentationsAsync(string userId)
    {
        return await _context.Presentations.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<List<Presentation>> GetOtherPresentationsAsync(string userId)
    {
        return await _context.Presentations.Where(x => x.UserId != userId).ToListAsync();
    }

    public async Task<int> AddSlideAsync(int presentationId, string content)
    {
        var presentation = await _context.Presentations
            .Include(p => p.Slides)
            .FirstOrDefaultAsync(p => p.PresentationId == presentationId);

        if (presentation == null)
        {
            throw new Exception("Presentation not found.");
        }

        var slideNumber = presentation.Slides.Count + 1;
        var newSlide = new Slide
        {
            PresentationId = presentationId,
            Content = content,
            SlideNumber = slideNumber,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        _context.Slides.Add(newSlide);
        await _context.SaveChangesAsync();

        return newSlide.SlideId;
    }
}
