using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KerbalGraph
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KerbalGraphFactory : MonoBehaviour
    {
        internal static KerbalGraphFactory instance;

        public void Awake()
        {
            Debug.Log("[KG] Awake");
            DontDestroyOnLoad(this);
            Assembly thisA = Assembly.GetExecutingAssembly();
            Version thisV= thisA.GetName().Version;
            String assemName = thisA.GetName().Name;
            Version highestVersion  = new Version(0,0);
            var aList = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == assemName);
            foreach(var a in aList)
            {
                var version = a.assembly.GetName().Version;
                if(version > highestVersion)
                {
                    highestVersion = version;
                }
            }
            Debug.Log("[KG] Elected version " + highestVersion);
            if(highestVersion == thisV)
            {
               Invoke("SetInstances",0);
            }
            Debug.Log("[KG] /Awake");
        }

        private void SetInstances()
        {
            Assembly thisA = Assembly.GetExecutingAssembly();
            Version thisV= thisA.GetName().Version;
            var aList = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == thisA.GetName().Name);
            Debug.Log("[KG] Version " + thisV + " setting instances");
            foreach(var a in aList)
            {
                a.assembly.GetType(this.GetType().Name).GetField("instance").SetValue(null, this);
                Debug.Log("[KG] Set an instance successfully");
            }
        }

        internal KerbalGraph EmitGraph(int width, int height)
        {
            Debug.Log("[KG] Emitting graph from version " + Assembly.GetExecutingAssembly().GetName().Version);
            return new KerbalGraph(width, height);
        }

        internal KerbalGraph EmitGraph(int width,int height, double minx,double maxx,double miny,double maxy)
        {
            return new KerbalGraph(width, height, minx, maxx, miny, maxy);
        }
    }
}
