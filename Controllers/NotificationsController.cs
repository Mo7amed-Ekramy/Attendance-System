using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using System.Threading.Tasks;

namespace MVC_PROJECT.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            int userId = User.GetUserId();
            if (userId <= 0)
            {
                return Json(new { success = false, message = "Unable to identify user." });
            }

            var list = await _notificationService.GetNotificationsByUserAsync(userId);

            return Json(new
            {
                success = true,
                unreadCount = list.UnreadCount,
                totalCount = list.TotalCount,
                notifications = list.Notifications.Select(n => new {
                    id = n.NotificationId,
                    title = n.Title,
                    type = n.Type,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = User.GetUserId();
            if (userId <= 0)
            {
                return BadRequest("Unable to identify user.");
            }

            var notificationsList = await _notificationService.GetNotificationsByUserAsync(userId);
            return View(notificationsList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }

            int userId = User.GetUserId();
            if (userId <= 0)
            {
                return BadRequest("Unable to identify user.");
            }

            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            await _notificationService.MarkAsReadAsync(id);

            return Json(new { success = true, message = "Notification marked as read." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int userId = User.GetUserId();
            if (userId <= 0)
            {
                return BadRequest("Unable to identify user.");
            }

            await _notificationService.MarkAllAsReadAsync(userId);

            return Json(new { success = true, message = "All notifications marked as read." });
        }
    }
}
