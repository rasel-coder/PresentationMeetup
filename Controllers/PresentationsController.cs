using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresentationMeetup.Data;
using PresentationMeetup.Services;
using PresentationMeetup.Utility;
using PresentationMeetup.ViewModels;

namespace PresentationMeetup.Controllers;

public class PresentationsController : Controller
{
    private readonly PresentationDbContext _context;
    private readonly PresentationService _presentationService;

    public PresentationsController(PresentationDbContext _context
        , PresentationService presentationService)
    {
        this._context = _context;
        this._presentationService = presentationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Presentation presentation)
    {
        if (ModelState.IsValid)
        {
            var userId = Request.Cookies["userId"];
            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
                Response.Cookies.Append("userId", userId, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
            }

            presentation.UserId = userId;
            int id = await _presentationService.CreatePresentation(presentation);

            UserRole userRole = new UserRole
            {
                Nickname = presentation.Creator,
                Role = AppEnum.Role.Admin.ToString(),
                UserId = userId,
                PresentationId = id
            };
            await _presentationService.CreateUserRole(userRole);

            return RedirectToAction(nameof(Details), new { presentationId = id, nickName = presentation.Creator });
        }
        return View(presentation);
    }

    public async Task<IActionResult> Details(int presentationId, string? nickName)
    {
        var userId = Request.Cookies["userId"];
        var presentation = await _context.Presentations
            .Include(p => p.Slides)
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.PresentationId == presentationId);

        if (presentation == null) return NotFound();

        if (!string.IsNullOrEmpty(nickName) && !presentation.Users.Any(u => u.Nickname == nickName))
        {
            presentation.Users.Add(new UserRole
            {
                Nickname = nickName,
                Role = AppEnum.Role.Viewer.ToString(),
                PresentationId = presentationId
            });
            await _context.SaveChangesAsync();
        }

        ViewBag.Nickname = nickName;
        return View(presentation);
    }

    [HttpPost]
    public async Task<IActionResult> SaveSlide(int presentationId, string slideData)
    {
        var slides = await _context.Slides.Where(x => x.PresentationId == presentationId).ToListAsync();
        _context.Slides.RemoveRange(slides);

        List<Slide> slidesList = new List<Slide>();
        if (slidesList != null)
        {
            var slide = new Slide
            {
                PresentationId = presentationId,
                Content = slideData,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.Slides.Add(slide);
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, presentationId = presentationId });
    }

    [HttpGet]
    public async Task<IActionResult> GetSlide(int id)
    {
        var presentation = await _context.Presentations.FindAsync(id);
        if (presentation != null)
        {
            return Json(new { success = true, slideData = presentation.CanvasData });
        }
        return Json(new { success = false, message = "Presentation not found" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteSlide(int id)
    {
        var presentation = await _context.Presentations.FindAsync(id);
        if (presentation != null)
        {
            _context.Presentations.Remove(presentation);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Presentation not found" });
    }

    [HttpPost("add-editor")]
    public async Task<IActionResult> AddEditor([FromBody] AddEditor model)
    {
        var result = await _presentationService.AddEditor(model.PresentationId, model.EditorNickName);
        if (result)
        {
            return Ok();
        }
        return NotFound("Presentation not found or user cannot be added as editor.");
    }

    [HttpGet("can-edit/{presentationId}/{userNickName}")]
    public async Task<IActionResult> CanEdit(int presentationId, string userNickName)
    {
        var canEdit = await _presentationService.CanEdit(presentationId, userNickName);
        return Ok(new { canEdit });
    }
}
