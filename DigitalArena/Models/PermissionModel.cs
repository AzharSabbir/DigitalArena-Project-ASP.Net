using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class PermissionModel
    {
        public int PermissionId { get; set; }
        public bool IsValid { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<DateTime> LastDownloadedAt { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }

        public virtual ProductModel Product { get; set; }
        public virtual UserModel User { get; set; }
    }
}