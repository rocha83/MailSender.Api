using System;
using System.Collections.Generic;
using System.Text;

namespace Rochas.MailSender.Api.Models.ValueObjects
{
    public class ContactRequest
    {
        public string Company { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public long PhoneNumber { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }

    }
}
