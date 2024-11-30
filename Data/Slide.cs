namespace PresentationMeetup.Data;

public class Slide
{
    public int SlideId { get; set; }
    public int PresentationId { get; set; }
    public string? Content { get; set; }

    public int SlideNumber { get; set; } 
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public virtual Presentation? Presentation { get; set; }
}
