using PresentationMeetup.Data;

namespace PresentationMeetup.ViewModels;

public class SlideViewModel
{
    public int SlideId { get; set; }
    public string? SlideData { get; set; }
}

public class PresentationViewModel
{
    public PresentationViewModel()
    {
        Slides = new List<SlideViewModel>();
    }

    public int PresentationId { get; set; }
    public string? Title { get; set; }
    public string? Creator { get; set; }
    public List<SlideViewModel> Slides { get; set; }
    public virtual List<string> Editors { get; set; } = new List<string>();
    public List<UserRole> Users { get; set; } = new List<UserRole>();
}
