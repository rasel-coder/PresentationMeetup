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
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax
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

            Slide newSlide = new Slide
            {
                PresentationId = id,
                SlideNumber = 1,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            await _presentationService.CreateSlide(newSlide);

            return RedirectToAction(nameof(Details), new { presentationId = id, nickName = presentation.Creator });
        }
        return View(presentation);
    }

    public async Task<IActionResult> Details(int presentationId, string? nickName)
    {
        var presentation = await _context.Presentations
            .Include(p => p.Slides)
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.PresentationId == presentationId);

        if (presentation == null) return NotFound();

        if (!string.IsNullOrEmpty(nickName) && !presentation.Users.Any(u => u.Nickname == nickName))
        {
            ViewBag.UserRole = "Viewer";
        }
        else
        {
            ViewBag.UserRole = "Admin";
        }

        var viewModel = new PresentationViewModel
        {
            PresentationId = presentation.PresentationId,
            Title = presentation.Title,
            Creator = presentation.Creator,
            Slides = presentation.Slides.Select(s => new SlideViewModel
            {
                SlideId = s.SlideId,
                SlideData = s.Content
            }).ToList()
        };

        ViewBag.Nickname = nickName;
        return View(viewModel);
    }

    public async Task<IActionResult> DeletePresentation(int presentationId)
    {
        var presentation = await _context.Presentations.FindAsync(presentationId);
        if (presentation != null)
        {
            _context.Presentations.Remove(presentation);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        return Json(new { success = false, message = "Presentation not found" });
    }
}
