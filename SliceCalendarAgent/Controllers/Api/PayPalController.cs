using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using SliceCalendarAgent.Models.Data;

namespace SliceCalendarAgent.Controllers.Api
{
    public class PayPalController : ApiController
    {
        // GET: api/PayPal
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/PayPal/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/PayPal
        public HttpResponseMessage Post([FromBody] string trans)
        {
            try
            {
                if (trans == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Transaction is null");
                }

                var transString = GetFormDataString(trans);

                //Post back to either sandbox or live
                const string SANDBOX_URL = "https://www.sandbox.paypal.com/cgi-bin/webscr";
                const string LIVE_URL = "https://www.paypal.com/cgi-bin/webscr";
                var req = (HttpWebRequest)WebRequest.Create(SANDBOX_URL);

                //Set values for the request back
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                var strRequest = transString + "&cmd=_notify-validate";
                req.ContentLength = strRequest.Length;

                //Send the request to PayPal and get the response
                var streamOut = new StreamWriter(req.GetRequestStream());
                streamOut.Write(strRequest);
                streamOut.Close();

                string strResponse;
                var response = req.GetResponse();

                using (var streamIn = new StreamReader(response.GetResponseStream()))
                {
                    strResponse = streamIn.ReadToEnd();
                }


                // Set txn status
                //trans.txn_status = strResponse;

                // Get repository object
                var repo = new Repository();

                // Save txn
                //repo.SaveTransaction(trans);

                if (strResponse == "VERIFIED")
                {
                    //check the payment_status is Completed
                    //if (trans.payment_status.ToLower() == "completed")
                    //{
                    //    //check that txn_id has not been previously processed
                    //    var existingTxn = repo.GetTransaction(trans.txn_id);
                    //    if (existingTxn == null)
                    //    {
                    //        //check that receiver_email is your Primary PayPal email
                    //        //check that payment_amount/payment_currency are correct
                    //        //process payment
                    //        // add txn to google calendar
                    //    }
                    //}
                }
                else if (strResponse == "INVALID")
                {
                    //log for manual investigation
                }
                else
                {
                    //Response wasn't VERIFIED or INVALID, log for manual investigation
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception exc)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message, exc);
            }
        }

        private object GetFormDataString(string trans)
        {
            var jsonTrans = JsonConvert.SerializeObject(trans);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonTrans);

            var sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(HttpUtility.UrlEncode(kvp.Key));
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(kvp.Value));
            }

            return sb.ToString();
        }
    }
}
