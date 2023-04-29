using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace PPDuckSim_HTTP_Controller.endpoints
{
    internal class POSTEndpoints : MonoBehaviour
    {
        public static HttpServer.Response POSTDuckName(HttpListenerContext context)
        {
            GeneralManager manager = GeneralManager.Instance;
            if (manager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Error loading GeneralManager"));
            }

            JObject body = HttpServer.ParseBody(context);

            if (body == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing body"));
            }

            if (!HttpServer.HasKeys(body, new string[] { "duckId", "name" }))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Malformed body"));
            }

            string duckId = body.Value<string>("duckId");
            string name = body.Value<string>("name");

            if (!Saves.instance.GetSettings().showNamed)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Duck names are not shown"));
            }

            Type type = manager.GetType();
            FieldInfo field = type.GetField("allDucks", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, AssetReference> ducks = (Dictionary<string, AssetReference>)field.GetValue(manager);

            if (!ducks.Keys.Contains(duckId))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(404, "Duck not found"));
            }

            DuckManager mgr = Mod.GetDucksManager(duckId);

            if (mgr == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(404, "Duck not found"));
            }

            mgr.duckName.gameObject.SetActive(Saves.instance.GetSettings().showNamed && name != "");

            DuckUIManager.instance.ChangeName(duckId, name);

            JObject obj = new JObject();
            obj.Add("message", $"Successfully changed duck ({duckId}) to {name}");

            return new HttpServer.Response(true, obj);
        }

        public static HttpServer.Response POSTDuckSpectate(HttpListenerContext context)
        {
            GeneralManager manager = GeneralManager.Instance;
            if (manager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Error loading GeneralManager"));
            }

            JObject body = HttpServer.ParseBody(context);

            if (body == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing body"));
            }

            if (!HttpServer.HasKeys(body, new string[] { "duckId" }))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Malformed body"));
            }

            string duckId = body.Value<string>("duckId");

            DuckManager mgr = Mod.GetDucksManager(duckId);

            if (mgr == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(404, "Duck not found"));
            }

            GameObject duck = mgr.transform.gameObject;
            GeneralManager.Instance.ChangeDuck(GeneralManager.Instance.GetDuckIndex(duck));

            JObject obj = new JObject();
            obj.Add("message", $"Successfully spectated duck ({duckId})");
            return new HttpServer.Response(true, obj);
        }
    }
}
