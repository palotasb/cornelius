using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cornelius.IO.Tables
{
    [Table(Name = "emails")]
    class XEmail : AbstractRow
    {
        [Column(Name = "user", Number = 2)]
        public XUser User;

        [Column(Name = "email", Number = 3)]
        public string Email;

        public XEmail(XUser user, string email)
        {
            this.User = user;
            this.Email = email;
        }
    }
}
