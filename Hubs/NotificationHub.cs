using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MVC_PROJECT.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("NotificationHub connected: ConnectionId={conn}, UserIdentifier={user}", Context.ConnectionId, Context.UserIdentifier);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation(exception, "NotificationHub disconnected: ConnectionId={conn}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinStudentGroup(int studentId)
        {
            _logger.LogInformation("JoinStudentGroup called for studentId={id}, connection={conn}", studentId, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Student-{studentId}");
        }

        public async Task JoinSectionGroup(int sectionId)
        {
            _logger.LogInformation("JoinSectionGroup called for sectionId={id}, connection={conn}", sectionId, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Section-{sectionId}");
        }

        public async Task LeaveStudentGroup(int studentId)
        {
            _logger.LogInformation("LeaveStudentGroup called for studentId={id}, connection={conn}", studentId, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Student-{studentId}");
        }

        public async Task LeaveSectionGroup(int sectionId)
        {
            _logger.LogInformation("LeaveSectionGroup called for sectionId={id}, connection={conn}", sectionId, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Section-{sectionId}");
        }
    }
}
