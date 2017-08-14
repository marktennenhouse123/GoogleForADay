using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Data.Entity.Validation;
using MiniGoogle.Models;

namespace MiniGoogle.DataServices
{
    //TODO: SImplify the # of methods in here.

    public class MessageLogger
    {
        static CloudDBEntities DB = new CloudDBEntities();

        

        public static void LogThis(Exception ex, string someData)
        {
            try
            {
                AppLog msg = new AppLog();
                msg.FunctionName = GetExecutingMethodName(ex);
                msg.ObjectData = someData.Length > 2000 ? someData.Substring(0, 1999) : someData;
                msg.EntityErrors = "";
                msg.PageName = ex.Source;
                msg.FullMessage = ex.StackTrace.Length > 1500 ? ex.StackTrace.Substring(0, 1499) : ex.StackTrace;
                msg.MessageText = ex.Message;
                msg.AppName = "MiniGoogle";
                msg.DateCreated = DateTime.Now;
                DB.AppLogs.Add(msg);
                DB.SaveChanges();
                HttpContext.Current.Server.ClearError();
            }
            catch (Exception ex2)
            {
                HttpContext.Current.Server.ClearError();
            }

        }
        public static void LogThis(Exception ex)
        {
            LogThis(ex, "");

        }
        public static void LogThis(DbEntityValidationException ex, string someData)
        {
            try
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
                msg.ObjectData = someData.Length > 2000 ? someData.Substring(0, 1999) : someData;
                msg.EntityErrors = logText;
                msg.PageName = ex.Source;
                msg.FullMessage = ex.StackTrace.Length > 1500 ? ex.StackTrace.Substring(0, 1499) : ex.StackTrace;
                msg.MessageText = ex.Message;
                msg.AppName = "MiniGoogle";
                msg.DateCreated = DateTime.Now;
                DB.AppLogs.Add(msg);
                DB.SaveChanges();


                HttpContext.Current.Server.ClearError();
            }
            catch (Exception ex2)
            { HttpContext.Current.Server.ClearError();
            }
        }

        private static string GetExecutingMethodName(Exception exception)
        {
            try
            {
                var trace = new StackTrace(exception);
                var frame = trace.GetFrame(0);
                var method = frame.GetMethod();

                return string.Concat(method.DeclaringType.FullName, ".", method.Name);
            }
            catch (Exception ex)
            {
                HttpContext.Current.Server.ClearError();
            }
            return "";
        }
    }
}