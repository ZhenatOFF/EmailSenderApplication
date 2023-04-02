using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Email
    {
        private string _address;
        private string? _subject;
        private string _message;

        public string Address { get => _address; set => _address = value; }
        public string Subject { get => _subject; set => _subject = value; }
        public string Message { get => _message; set => _message = value; }
    }
}
