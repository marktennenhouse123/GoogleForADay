using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace MiniGoogle.Services
{
// when you search a node collection and it turns up empty it throws a null exception
//this class returns instead an empty collection and avoids the error.
//http://htmlagilitypack.codeplex.com/workitem/29175    

        static class ExtensionToHtmlAgility
        {
            public static HtmlNodeCollection NullGuard(this HtmlNodeCollection self)
            {
                if (self == null)
                    return new HtmlNodeCollection(null);
                else
                    return self;
            }
       
    }
}