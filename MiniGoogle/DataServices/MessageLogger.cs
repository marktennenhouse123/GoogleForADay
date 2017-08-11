using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Data.Entity.Validation;
using MiniGoogle.Models;

namespace MiniGoogle.DataServices
{
    public class MessageLogger
    {
        static CloudDBEntities DB = new CloudDBEntities();

        public static void LogThis(Exception ex)
        {
            AppLog msg = new AppLog();
            msg.FunctionName = GetExecutingMethodName(ex);
            msg.ObjectData = "";
            msg.EntityErrors = "";
            msg.PageName = ex.Source;
            msg.FullMessage = ex.StackTrace;
            msg.MessageText = ex.Message;
            msg.AppName = "MiniGoogle";
            msg.DateCreated = DateTime.Now;
            DB.AppLogs.Add(msg);
            DB.SaveChanges();


        }
        public static void LogThis(DbEntityValidationException ex, string someData)
        {
            AppLog msg = new AppLog();
            string logText = "";

             foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                    logText += string.Format("Property: {0} Error: {1}",
                                            validationError.PropertyName,
                                            validationError.ErrorMessage);
                    }
                }
            
            
            msg.FunctionName = GetExecutingMethodName(ex);
            msg.ObjectData = someData;
            msg.EntityErrors = logText;
            msg.PageName = ex.Source;
            msg.FullMessage = ex.StackTrace;
            msg.MessageText = ex.Message;
            msg.AppName = "MiniGoogle";
            msg.DateCreated = DateTime.Now;
            DB.AppLogs.Add(msg);
            DB.SaveChanges();
                       
            
            
        }

        private static string GetExecutingMethodName(Exception exception)
        {
            var trace = new StackTrace(exception);
            var frame = trace.GetFrame(0);
            var method = frame.GetMethod();

            return string.Concat(method.DeclaringType.FullName, ".", method.Name);
        }
    }
}