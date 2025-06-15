using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class NotificationModel
    {
        public int NotificationId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }

        public virtual UserModel User { get; set; }
    }
}