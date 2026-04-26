using Microsoft.AspNetCore.SignalR;

namespace MVC_PROJECT.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinStudentGroup(int studentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Student-{studentId}");
        }

        public async Task JoinSectionGroup(int sectionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Section-{sectionId}");
        }

        public async Task LeaveStudentGroup(int studentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Student-{studentId}");
        }

        public async Task LeaveSectionGroup(int sectionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Section-{sectionId}");
        }
    }
}
