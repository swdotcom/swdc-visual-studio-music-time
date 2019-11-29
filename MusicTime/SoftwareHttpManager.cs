using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;

namespace MusicTime
{
    class SoftwareHttpManager
    {
        public static bool IsOk(HttpResponseMessage response)
        {
            return (response != null && response.StatusCode == HttpStatusCode.OK);
        }
        public static async Task<HttpResponseMessage> SendDashboardRequestAsync(HttpMethod httpMethod, string uri)
        {
            return await SendRequestAsync(httpMethod, uri, null, 60);
        }

        public static async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string uri, string optionalPayload)
        {
            return await SendRequestAsync(httpMethod, uri, optionalPayload, 10);
        }

        public static async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string uri, string optionalPayload, string jwt)
        {
            return await SendRequestAsync(httpMethod, uri, optionalPayload, 10, jwt);
        }

        public static async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string uri, string optionalPayload, int timeout, string jwt = null, bool isOnlineCheck = false)
        {

            if (!SoftwareCoUtil.isTelemetryOn())
            {
                return null;
            }

            if (!isOnlineCheck && !SoftwareUserSession.isOnline)
            {
                return null;
            }

            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            var cts = new CancellationTokenSource();
            HttpResponseMessage response = null;
            if (jwt == null)
            {
                object jwtObj = SoftwareUserSession.GetJwt();
                if (jwtObj != null)
                {
                    jwt = (string)jwtObj;
                }
            }
            if (jwt != null)
            {
                // add the authorizationn
                client.DefaultRequestHeaders.Add("Authorization", jwt);
            }
            HttpContent contentPost = null;
            try
            {
                if (optionalPayload != null)
                {
                    contentPost = new StringContent(optionalPayload, Encoding.UTF8, "application/json");
                }
            }
            catch (Exception e)
            {
                NotifyPostException(e);
            }
            bool isPost = (httpMethod.Equals(HttpMethod.Post));
            try
            {
                string endpoint = Constants.api_endpoint + "" + uri;
                if (isPost)
                {
                    response = await client.PostAsync(endpoint, contentPost, cts.Token);
                }
                else 
                {
                    response = await client.GetAsync(endpoint, cts.Token);
                }
            }
            catch (HttpRequestException e)
            {
                if (isPost)
                {
                    NotifyPostException(e);
                }
            }
            catch (TaskCanceledException e)
            {
                if (e.CancellationToken == cts.Token)
                {
                    // triggered by the caller
                    if (isPost)
                    {
                        NotifyPostException(e);
                    }
                }
                else
                {
                    // a web request timeout (possibly other things!?)
                    //Logger.Info("We are having trouble receiving a response from Software.com");
                }
            }
            catch (Exception e)
            {
                if (isPost)
                {
                    NotifyPostException(e);
                }
            }
            finally
            {
            }
            return response;
        }

        public static async Task<HttpResponseMessage> SendRequestPutAsync(string api,string Payload)
        {
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            string jwt = null;
            
            try
            {
                object jwtObj = SoftwareUserSession.GetJwt();
                if (jwtObj != null)
                {
                    jwt = (string)jwtObj;
                }

                if (jwt != null)
                {
                    // add the authorizationn
                    client.DefaultRequestHeaders.Add("Authorization", jwt);
                }

                if(Payload==null)
                {
                    Payload = "";
                }
                HttpContent contentPost = null;
                contentPost = new StringContent(Payload, Encoding.UTF8, "application/json");
                string endpoint = Constants.api_endpoint + "" + api;
                response = await client.PutAsync(endpoint, contentPost);


            }
            catch (Exception ex)
            {

            }

            return response;

        }

        private static void NotifyPostException(Exception e)
        {
           // Logger.Error("We are having trouble sending data to Software.com, reason: " + e.Message);
        }

    }
}
