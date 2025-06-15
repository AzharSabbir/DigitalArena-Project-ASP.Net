using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class ProductFileModel
    {
        public int ProductFileId { get; set; }
        public string FileName { get; set; }
        public string FileFormat { get; set; }
        public int ProductId { get; set; }

        public virtual ProductModel Product { get; set; }
    }
}