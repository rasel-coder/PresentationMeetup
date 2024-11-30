namespace PresentationMeetup.Data;

public class UserRole
{
    public int UserRoleId { get; set; }
    public string? Nickname { get; set; }
    public string? UserId { get; set; }
    public string? Role { get; set; }
    public int PresentationId { get; set; }
    public Presentation? Presentation { get; set; }
}
