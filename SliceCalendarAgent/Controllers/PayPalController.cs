using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using SliceCalendarAgent.Models;
using SliceCalendarAgent.Models.Data;

namespace SliceCalendarAgent.Controllers
{
    public class PayPalController : Controller
    {
        [HttpGet]
        public ActionResult Ping()
        {
            var result = new ContentResult {Content = "Pong"};
            return result;
        }

        [HttpPost]
        public HttpResponseMessage IpnMessage()
        {
            try
            {
                // UrlDecode form params
                var trans = HttpUtility.UrlDecode(Request.Form.ToString());
                var jsonTrans = GetJson(trans);

                var repo = new Repository();
                repo.AddRawTransaction(new RawTransaction() {Data = jsonTrans});

                // Post Paypal (sandbox or live per config file)
                var payPalUrl = WebConfigurationManager.AppSettings["PayPalUrl"];
                var req = (HttpWebRequest)WebRequest.Create(payPalUrl);

                //Set values for the request back
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                var strRequest = trans + "&cmd=_notify-validate";
                req.ContentLength = strRequest.Length;

                //Send the request to PayPal and get the response
                var streamOut = new StreamWriter(req.GetRequestStream());
                streamOut.Write(strRequest);
                streamOut.Close();

                // Get response from PayPal
                string strResponse;
                using (var streamIn = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    strResponse = streamIn.ReadToEnd();
                }

                // Build Transaction entity from form params
                var txn = GetTransaction(trans);

                // Set Transaction status equal to the response from PayPal (VERIFIED or INVALID)
                txn.txn_status = strResponse;

                // Save txn
                repo.SaveTransaction(txn);

                //if (strResponse == "VERIFIED")
                //{
                    //check the payment_status is Completed
                    if (txn.payment_status.ToLower() == "completed")
                    {
                        //check that txn_id has not been previously processed
                        var existingTxn = repo.GetTransaction(txn.txn_id);
                        if (existingTxn.Count == 1)
                        {
                            var agent = new GoogleAgent();
                            agent.ScheduleEvent(txn, jsonTrans);
                            //check that receiver_email is your Primary PayPal email
                            //check that payment_amount/payment_currency are correct
                            //process payment
                            // add txn to google calendar
                        }
                    }
                //}
                //else if (strResponse == "INVALID")
                //{
                    //log for manual investigation
                //}
                else
                {
                    //Response wasn't VERIFIED or INVALID, log for manual investigation
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception exc)
            {
                var repo = new Repository();
                
                repo.AddErrorLog(new ErrorLog() {Message = "Exception: " + exc + " Message: " + exc.Message + " Inner: " + exc.InnerException + " Stack Trace: " + exc.StackTrace});
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        private Transaction GetTransaction(string trans)
        {
            var json = GetJson(trans);

            return JsonConvert.DeserializeObject<Transaction>(json);
        }

        private string GetJson(string trans)
        {
            var json = new StringBuilder("{");
            var keyValuePairs = trans.Split('&');

            foreach (var kvp in keyValuePairs)
            {
                var item = kvp.Split('=');
                json.Append("\"" + item[0] + "\"" + " : " + "\"" + item[1] + "\",");
            }

            json.Append("}");

            return json.ToString();
        }

        public ActionResult TestFileRead()
        {
            var text = string.Empty;
            var path = HostingEnvironment.MapPath("~/Certs/test.txt");
            using (var sr = new StreamReader(path))
            {
                text = sr.ReadToEnd();
            }

            return new ContentResult() {Content = text};
        }


    }
}