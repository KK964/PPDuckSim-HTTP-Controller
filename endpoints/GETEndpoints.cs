using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine.AddressableAssets;

namespace PPDuckSim_HTTP_Controller.endpoints
{
    internal class GETEndpoints
    {
        public static HttpServer.Response GETDuckIDs(HttpListenerContext context)
        {
            GeneralManager manager = GeneralManager.Instance;
            if (manager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Error loading GeneralManager"));
            }

            Type type = manager.GetType();
            FieldInfo field = type.GetField("allDucks", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, AssetReference> ducks = (Dictionary<string, AssetReference>)field.GetValue(manager);

            JObject arr = new JObject();
            foreach (string duckId in ducks.Keys)
            {
                DuckManager mngr = Mod.GetDucksManager(duckId);
                if (mngr == null) continue;
                arr.Add(duckId, mngr.CustomName);
            }

            return new HttpServer.Response(true, arr);
        }

        public static HttpServer.Response GETCurrentDuckID(HttpListenerContext context)
        {
            GeneralManager manager = GeneralManager.Instance;
            if (manager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Error loading GeneralManager"));
            }

            int currentDuck = manager.CurrentDuck;
            DuckManager duckManager = manager.Ducks[currentDuck].GetComponent<DuckManager>();

            if (duckManager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Current duck has no DuckManager"));
            }


            JObject obj = new JObject();
            obj.Add("duckId", duckManager.duckID);
            obj.Add("name", duckManager.CustomName);

            return new HttpServer.Response(true, obj);
        }

        public static HttpServer.Response GETAreDucksSpawned(HttpListenerContext context)
        {
            GeneralManager manager = GeneralManager.Instance;
            if (manager == null)
            {
                return new HttpServer.Response(false, HttpServer.CreateErrorObject(500, "Error loading GeneralManager"));
            }

            Type type = manager.GetType();
            FieldInfo field = type.GetField("ducksToSpawn", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<string, AssetReference> ducksToSpawn = (Dictionary<string, AssetReference>)field.GetValue(manager);

            bool allDucksSpawned = true;

            foreach (string duckId in ducksToSpawn.Keys)
            {
                DuckManager mngr = Mod.GetDucksManager(duckId);
                if (mngr == null)
                {
                    allDucksSpawned = false;
                    break;
                }
            }

            JObject obj = new JObject();
            obj.Add("allDucksSpawned", allDucksSpawned);

            return new HttpServer.Response(true, obj);
        }
    }
}
