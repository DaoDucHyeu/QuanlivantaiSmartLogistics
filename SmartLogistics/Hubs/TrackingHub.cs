using Microsoft.AspNetCore.SignalR;

namespace SmartLogistics.Hubs
{
    public class TrackingHub : Hub
    {
        /// <summary>
        /// Tài xế gửi vị trí GPS lên server
        /// </summary>
        public async Task SendLocation(int tripId, decimal lat, decimal lng, decimal speed)
        {
            await Clients.Group("admin").SendAsync("ReceiveLocation", tripId, lat, lng, speed, DateTime.Now);
        }

        /// <summary>
        /// Admin tham gia nhóm theo dõi
        /// </summary>
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }

        /// <summary>
        /// Gửi thông báo đến tất cả
        /// </summary>
        public async Task SendNotification(string title, string message, string type)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, message, type, DateTime.Now);
        }
    }
}
