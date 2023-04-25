using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace PPDuckSim_HTTP_Controller.endpoints
{
    internal class POSTEndpoints
    {
        public static HttpServer.Response POSTDuckName(HttpListenerContext context)
        {
            GeneralManager manager = GeneralManager.Instance;
            if (manager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Error loading GeneralManager"));
            }

            if (context.Request.InputStream == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing body"));
            }

            string body = new StreamReader(context.Request.InputStream).ReadToEnd();

            if (string.IsNullOrEmpty(body))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing body"));
            }

            DuckNameChange nameChange;
            try
            {
                nameChange = JsonConvert.DeserializeObject<DuckNameChange>(body);
            } catch (Exception e)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Body parsing failed"));
            }

            if (nameChange == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Malformed body"));
            }

            if (string.IsNullOrEmpty(nameChange.duckId))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing duckId"));
            }

            if (string.IsNullOrEmpty(nameChange.name))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing name"));
            }

            Type type = manager.GetType();
            FieldInfo field = type.GetField("allDucks", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, AssetReference> ducks = (Dictionary<string, AssetReference>)field.GetValue(manager);

            if (!ducks.Keys.Contains(nameChange.duckId))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(404, "Duck not found"));
            }

            DuckUIManager.instance.ChangeName(nameChange.duckId, nameChange.name);

            JObject obj = new JObject();
            obj.Add("message", $"Successfully changed duck ({nameChange.duckId}) to {nameChange.name}");

            return new HttpServer.Response(true, obj);
        }

        public static HttpServer.Response POSTDuckSpectate(HttpListenerContext context)
        {
            GeneralManager manager = GeneralManager.Instance;
            if (manager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Error loading GeneralManager"));
            }

            if (context.Request.InputStream == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing body"));
            }

            string body = new StreamReader(context.Request.InputStream).ReadToEnd();

            if (string.IsNullOrEmpty(body))
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Missing body"));
            }

            DuckNameChange nameChange;
            try
            {
                nameChange = JsonConvert.DeserializeObject<DuckNameChange>(body);
            }
            catch (Exception e)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(400, "Body parsing failed"));
            }

            DuckManager mgr = Mod.GetDucksManager(nameChange.duckId);

            if (mgr == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(404, "Duck not found"));
            }

            GameObject duck = mgr.transform.gameObject;
            GeneralManager.Instance.ChangeDuck(GeneralManager.Instance.GetDuckIndex(duck));

            JObject obj = new JObject();
            obj.Add("message", $"Successfully spectated duck ({nameChange.duckId})");
            return new HttpServer.Response(true, obj);
        }

        public class DuckNameChange
        {
            public string duckId { get; set; }
            public string name { get; set; }
        }
    }
}
