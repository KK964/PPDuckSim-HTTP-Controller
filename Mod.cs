using MelonLoader;
using UnityEngine;

namespace PPDuckSim_HTTP_Controller
{
    public class Mod : MelonMod
    {
        public static Mod Instance { get { return Mod._instance; } }
        private static Mod _instance { get; set; }

        public MelonPreferences_Category preferences;
        public MelonPreferences_Entry<int> port;

        public override void OnInitializeMelon()
        {
            _instance = this;
            preferences = MelonPreferences.CreateCategory("OurFirstCategory");
            port = preferences.CreateEntry<int>("Port", 25523);
            new HttpServer(port.Value);
        }

        public static DuckManager GetDucksManager(string duckId)
        {
            DuckManager[] managers = GameObject.FindObjectsOfType<DuckManager>();

            foreach (DuckManager mngr in managers)
            {
                if (mngr.duckID != duckId) continue;
                return mngr;
            }

            return null;
        }
    }
}
