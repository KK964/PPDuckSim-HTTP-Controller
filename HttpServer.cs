using System.Threading;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using PPDuckSim_HTTP_Controller.endpoints;

namespace PPDuckSim_HTTP_Controller
{
    internal class HttpServer
    {
        protected HttpListener listener;

        public static HttpServer Instance { get { return HttpServer._instance; } }

        private static HttpServer _instance { get; set; }

        private int port;

        public HttpServer(int port)
        {
            this.port = port;
            if (!HttpListener.IsSupported)
            {
                Mod.Instance.Unregister("HttpListener is unsupported!");
                return;
            }

            _instance = this;

            listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{port}/");

            new Thread(ServerThread).Start();
        }

        private static Dictionary<string, Dictionary<string, ExecuteMethod>> methods = new Dictionary<string, Dictionary<string, ExecuteMethod>>()
        {
            ["GET"] = new Dictionary<string, ExecuteMethod>()
            {
                ["/ducks"] = GETEndpoints.GETDuckIDs,
            },
            ["POST"] = new Dictionary<string, ExecuteMethod>()
            {
                ["/name"] = POSTEndpoints.POSTDuckName,
                ["/spectate"] = POSTEndpoints.POSTDuckSpectate,
            }
        };

        private void ServerThread()
        {
            listener.Start();
            Mod.Instance.LoggerInstance.Msg($"Started HTTP server on 0.0.0.0:{port}");

            while (Mod.Instance.Registered)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string method = request.HttpMethod;
                string endpoint = request.Url.PathAndQuery.Split('?')[0];

                JObject respObj = new JObject();

                Dictionary<string, ExecuteMethod> methodEndpoints = null;
                methods.TryGetValue(method, out methodEndpoints);

                if (methodEndpoints == null || !methodEndpoints.ContainsKey(endpoint))
                {
                    respObj.Add("success", false);
                    respObj.Add("error", CreateErrorObject(404, "Endpoint not found"));
                } else
                {
                    ExecuteMethod outp = methodEndpoints[endpoint];
                    Response res = outp.Invoke(context);
                    respObj.Add("success", res.success);
                    respObj.Add(res.success ? "data" : "error", res.output);
                }

                string responseString = respObj.ToString();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }

            listener.Stop();
        }

        public static JObject CreateErrorObject(int code, string message)
        {
            JObject err = new JObject();
            err.Add("code", code);
            err.Add("message", message);
            return err;
        }

        public delegate Response ExecuteMethod(HttpListenerContext context);

        public class Response
        {
            public bool success;
            public JToken output;

            public Response(bool success, JToken output)
            {
                this.success = success;
                this.output = output;
            }
        }
    }
}
