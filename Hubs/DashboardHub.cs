using Microsoft.AspNetCore.SignalR;

namespace MVC_PROJECT.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task JoinStudentGroup(int studentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Student-{studentId}");
        }

        public async Task JoinTAGroup(int taId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"TA-{taId}");
        }

        public async Task JoinDoctorGroup(int doctorId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Doctor-{doctorId}");
        }

        public async Task LeaveStudentGroup(int studentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Student-{studentId}");
        }

        public async Task LeaveTAGroup(int taId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"TA-{taId}");
        }

        public async Task LeaveDoctorGroup(int doctorId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Doctor-{doctorId}");
        }
    }
}
