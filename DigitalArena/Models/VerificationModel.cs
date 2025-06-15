using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class VerificationModel
    {
        public int VerificationId { get; set; }
        public string Type { get; set; }
        public string Otp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }

        public virtual UserModel User { get; set; }
    }
}