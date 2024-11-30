namespace PresentationMeetup.Data;

public class Presentation
{
    public int PresentationId { get; set; }
    public string? Title { get; set; }
    public string? Creator { get; set; }
    public string? CanvasData { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public virtual List<string> Editors { get; set; } = new List<string>();
    public ICollection<Slide> Slides { get; set; } = new List<Slide>();
    public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
}
