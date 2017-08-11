using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniGoogle.Models
{
    public class AppLogVM
    {
        public int MessageID { get; set; }
        public string MessageText { get; set; }
        public string FullMessage { get; set; }
        public string FunctionName { get; set; }
        public string PageName { get; set; }
        public string AppName { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string EntityErrors { get; set; }
        public string ObjectData { get; set; }
    }
}