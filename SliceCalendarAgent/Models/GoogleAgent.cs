using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Ajax.Utilities;
using SliceCalendarAgent.Models.Data;

namespace SliceCalendarAgent.Models
{
    public class GoogleAgent
    {
        private readonly String _serviceAccountEmail = WebConfigurationManager.AppSettings["GoogleServiceAccountEmail"];
        private readonly string _googleCert = WebConfigurationManager.AppSettings["GoogleCertificateName"];
        private readonly string _appName = WebConfigurationManager.AppSettings["GoogleApplicationName"];
        private readonly string _calendarId = WebConfigurationManager.AppSettings["GoogleCalendarId"];
        private readonly string _newLine = Environment.NewLine;

        public void ScheduleEvent(Transaction txn, string jsonTrans)
        {
            if (null == ServicePointManager.ServerCertificateValidationCallback)
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chaing, sslPolicyErrors) => true;
            }

            String path = HostingEnvironment.MapPath("~/Certs/" + _googleCert);
            var cert = new X509Certificate2(path, "notasecret", X509KeyStorageFlags.Exportable|X509KeyStorageFlags.MachineKeySet);

            var credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(_serviceAccountEmail)
               {
                   Scopes = new[] { CalendarService.Scope.Calendar },
               }.FromCertificate(cert));

            // Create the service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName,

            });

            var orderDetails = "Pie Order:" + _newLine + "Name: " + txn.first_name + " " + txn.last_name + _newLine;

            // DEBUG:
            var repo = new Repository();
            repo.AddErrorLog(new ErrorLog() {Message = "txn.payment_date=" + txn.payment_date});

            var paymentDate = ConvertPayPalDateTime(txn.payment_date);
            repo.AddErrorLog(new ErrorLog() { Message = "parsed txn.payment_date=" + paymentDate.ToLongDateString() });

            
            service.Events.Insert(
                new Event()
                {
                    Summary = "Pie Order",
                    Description =  jsonTrans, //orderDetails + _newLine + "Details: " + txn.custom,
                    Start = new EventDateTime { DateTime = paymentDate },
                    End = new EventDateTime() { DateTime = paymentDate },
                    
                },
                _calendarId).Execute();
        }

        public static DateTime ConvertPayPalDateTime(string payPalDateTime)
        {
            // accept a few different date formats because of PST/PDT timezone and slight month difference in sandbox vs. prod.
            string[] dateFormats = { "HH:mm:ss MMM dd, yyyy PST", "HH:mm:ss MMM. dd, yyyy PST", "HH:mm:ss MMM dd, yyyy PDT", "HH:mm:ss MMM. dd, yyyy PDT" };
            DateTime outputDateTime;

            DateTime.TryParseExact(payPalDateTime, dateFormats, new CultureInfo("en-US"), DateTimeStyles.None, out outputDateTime);

            // convert to local timezone
            //outputDateTime = outputDateTime.AddHours(3);

            return outputDateTime;
        }








    }
}