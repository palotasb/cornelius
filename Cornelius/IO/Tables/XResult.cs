using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cornelius.Criteria;

namespace Cornelius.IO.Tables
{
    [Table(Name = "results")]
    class XResult : AbstractRow
    {
        [Column(Name = "user", Number = 2)]
        public XUser User;

        [Column(Name = "parent", Number = 3)]
        public XResult Parent;

        [Column(Name = "title", Number = 4)]
        public string Title;

        [Column(Name = "value", Number = 5)]
        public bool Value;

        public XResult(XUser user, XResult parent, Result result)
        {
            this.User = user;
            this.Parent = parent;
            this.Title = result.Name;
            this.Value = result.Value;
        }
    }
}
