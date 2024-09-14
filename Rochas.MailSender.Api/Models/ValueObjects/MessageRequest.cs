using System;
using System.Collections.Generic;
using System.Text;

namespace Rochas.MailSender.Api.Models.ValueObjects
{
    public class MessageRequest
    {
        public string Sender { get; set; }
		public string From { get; set; }
        public string To { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }

    }
}
