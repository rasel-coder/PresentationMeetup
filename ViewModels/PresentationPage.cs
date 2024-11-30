
using System.ComponentModel.DataAnnotations;
using PresentationMeetup.Data;

namespace PresentationMeetup.ViewModels;

public class PresentationPage
{
    public int PresentationId { get; set; }

    [Display(Name = "Nick Name")]
    public string? NickName { get; set; }

    public Presentation Presentation { get; set; } = new Presentation();
    public IEnumerable<Presentation> MyPresentations { get; set; } = new List<Presentation>();
    public IEnumerable<Presentation> OtherPresentations { get; set; } = new List<Presentation>();
}
