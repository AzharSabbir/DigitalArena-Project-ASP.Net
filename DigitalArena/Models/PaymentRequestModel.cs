using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalArena.Models
{
    public class PaymentRequestModel
    {
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public string Expiry { get; set; }
        public string Cvc { get; set; }
    }

}