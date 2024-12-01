using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PresentationMeetup.Data;
using System;
using System.Collections.Concurrent;
using System.Data;
using static PresentationMeetup.Utility.AppEnum;

namespace PresentationMeetup.Utility;

public class CollaborationHub : Hub
{
    private readonly PresentationDbContext _context;

    public CollaborationHub(PresentationDbContext context)
    {
        _context = context;
    }

    public async Task<int> UpdateSlide(int slideId, string content)
    {
        var slides = await _context.Slides.ToListAsync();
        int updatedSlideId;

        if (slideId <= 0)
        {
            var newSlide = new Slide
            {
                PresentationId = slides.FirstOrDefault()?.PresentationId ?? 0,
                Content = content,
                SlideNumber = slides.Count + 1,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            await _context.AddAsync(newSlide);
            await _context.SaveChangesAsync();

            updatedSlideId = newSlide.SlideId;
        }
        else
        {
            var existingSlide = slides.FirstOrDefault(x => x.SlideId == slideId);
            if (existingSlide != null)
            {
                existingSlide.Content = content;
                existingSlide.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                updatedSlideId = existingSlide.SlideId;
            }
            else
            {
                throw new Exception("Slide not found for update.");
            }
        }
        await Clients.Others.SendAsync("ReceiveSlideUpdate", updatedSlideId, content);
        return updatedSlideId;
    }


    public async Task DeleteSlide(int slideId)
    {
        var slide = await _context.Slides.FindAsync(slideId);
        if (slide != null)
        {
            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
            await Clients.Others.SendAsync("ReceiveSlideDelete");
        }
    }

    private static readonly ConcurrentDictionary<string, (string Nickname, string PresentationId, string Role)> ConnectedUsers = new();

    public async Task JoinPresentation(int PresentationId, string NickName, string role)
    {
        var admin = await _context.UserRoles.FirstOrDefaultAsync(x => x.PresentationId == PresentationId);

        await Groups.AddToGroupAsync(Context.ConnectionId, PresentationId.ToString());
        ConnectedUsers[Context.ConnectionId] = (NickName, PresentationId.ToString(), role);

        await NotifyGroupUserListUpdate(PresentationId.ToString());
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
        {
            await Clients.Group(userInfo.PresentationId).SendAsync("UserDisconnected", userInfo.Nickname);
            await NotifyGroupUserListUpdate(userInfo.PresentationId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    private async Task NotifyGroupUserListUpdate(string presentationId)
    {
        var usersInGroup = ConnectedUsers.Values
        .Where(user => user.PresentationId == presentationId)
        .Select(user => new { user.Nickname, user.Role })
        .ToList();

        await Clients.Group(presentationId).SendAsync("UserListUpdated", usersInGroup);
    }



    //public async Task JoinPresentation(int presentationId, string nickname, string role = "Viewer")
    //{
    //    await NotifyGroupUserListUpdate(presentationId);
    //}

    //public override async Task OnDisconnectedAsync(Exception exception)
    //{
    //    if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
    //    {
    //        var groupName = userInfo.GroupName;
    //        var nickname = userInfo.Nickname;

    //        await Clients.Group(groupName).SendAsync("UserDisconnected", nickname);
    //        await NotifyGroupUserListUpdate(userInfo.GroupName);
    //    }

    //    await base.OnDisconnectedAsync(exception);
    //}

    //private async Task NotifyGroupUserListUpdate(int presentationId)
    //{
    //    var usersPresents = _context.UserRoles
    //    .Where(user => user.PresentationId == presentationId)
    //    .Select(user => new { user.Nickname, user.Role })
    //    .ToList();

    //    await Clients.Others.SendAsync("UserListUpdated", usersPresents);
    //}
}
