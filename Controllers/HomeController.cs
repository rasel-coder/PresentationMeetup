using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresentationMeetup.Data;
using PresentationMeetup.Services;
using PresentationMeetup.ViewModels;

namespace PresentationMeetup.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PresentationService _presentationService;

    public HomeController(ILogger<HomeController> logger
        , PresentationService _presentationService)
    {
        _logger = logger;
        this._presentationService = _presentationService;
    }
    public async Task<IActionResult> Index()
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
        PresentationPage presentationPage = new PresentationPage();
        presentationPage.MyPresentations = await _presentationService.GetMyPresentationsAsync(userId);
        presentationPage.OtherPresentations = await _presentationService.GetOtherPresentationsAsync(userId);
        return View(presentationPage);
    }

    public async Task<IActionResult> DeleteAllPresentation()
    {
        await _presentationService.DeleteAllPresentationAsync();
        return RedirectToAction("Index");
    }
}
