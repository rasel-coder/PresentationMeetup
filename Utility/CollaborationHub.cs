using Microsoft.AspNetCore.SignalR;
using PresentationMeetup.Data;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace PresentationMeetup.Utility;

public class CollaborationHub : Hub
{
    private readonly PresentationDbContext _context;

    public CollaborationHub(PresentationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateSlide(int slideId, string content)
    {
        var slide = await _context.Slides.FindAsync(slideId);
        if (slide != null)
        {
            slide.Content = content;
            await _context.SaveChangesAsync();
        }

        // Broadcast the update to other users
        await Clients.Others.SendAsync("ReceiveSlideUpdate", slideId, content);
    }

    private static readonly ConcurrentDictionary<string, (string Nickname, string GroupName, string Role)> ConnectedUsers = new();

    public async Task JoinPresentation(int presentationId, string nickname, string role = "Viewer")
    {
        var groupName = $"presentation-{presentationId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // Add user to the connected users list
        ConnectedUsers[Context.ConnectionId] = (nickname, groupName, role);

        // Notify the group about the updated user list
        await NotifyGroupUserListUpdate(groupName);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
        {
            var groupName = userInfo.GroupName;
            var nickname = userInfo.Nickname;

            // Notify the group about the user disconnection
            await Clients.Group(groupName).SendAsync("UserDisconnected", nickname);

            // Notify the group about the user leaving
            await NotifyGroupUserListUpdate(userInfo.GroupName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task NotifyGroupUserListUpdate(string groupName)
    {
        // Get all nicknames for the specified group
        var usersInGroup = ConnectedUsers.Values
        .Where(user => user.GroupName == groupName)
        .Select(user => new { user.Nickname, user.Role })
        .ToList();

        // Send the updated user list to the group
        await Clients.Group(groupName).SendAsync("UserListUpdated", usersInGroup);
    }
}
