using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedAtmosphereToolsRedux.Interfaces;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)] //orbits are giving me nullrefs on main menu. otherwise i'd do this on main menu
    public class FinalSetupHandlerAsync : MonoBehaviour
    {
        public static FinalSetupHandlerAsync Instance;
        
        TaskWrapper[] tasks;
        bool Running = false;
        
        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Utils.LogWarning("Invoking duplicate protections for FinalSetupHandlerAsync class.");
                DestroyImmediate(this);
            }
        }

        public void Update()
        {
            if (Running)
            {
                return;
            }
            IEnumerable<TaskWrapper> taskwrappers = AtmosphereData.GetFinalSetupTasks();
            if (taskwrappers != null && taskwrappers.Count() > 0)
            {
                tasks = taskwrappers.ToArray();
                Utils.LogInfo("Starting Final Setup on all Atmosphere Modifiers that require it. This process will run asynchronously.");
                foreach (TaskWrapper task in tasks)
                {
                    task.StartTask();
                }
            }
            Running = true;
            this.enabled = false;
        }

        void OnDestroy()
        {
            if (tasks != null && tasks.Length > 0)
            {
                Utils.LogInfo("Finalizing Final Setup Tasks.");
                foreach (TaskWrapper task in tasks)
                {
                    task.FinalizeTask();
                }
                tasks = null;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    

    internal class TaskWrapper
    {
        private object lockobject = new object();
        
        private IRequiresFinalSetup obj;
        private Task task;

        internal TaskWrapper(IRequiresFinalSetup req)
        {
            obj = req;
            task = new Task(obj.FinalSetup);
        }

        internal void StartTask()
        {
            string type = (obj.GetType()).ToString();
            Utils.LogInfo("Starting Final Setup for type " + type + ".");
            task.Start();
        }

        internal void FinalizeTask()
        {
            lock (lockobject)
            {
                if (!task.IsCompleted)
                {
                    task.Wait();
                }

                string type = (obj.GetType()).ToString();
                if (task.IsFaulted)
                {
                    Utils.LogWarning("Purging faulted object of type " + type + " from Atmosphere Data. Exception thrown by object: \n" + task.Exception.InnerException.ToString());
                    AtmosphereData.PurgeFromAll(obj);
                }
                else
                {
                    Utils.LogInfo("Object of type " + type + " completed Final Setup successfully.");
                }
                task.Dispose();
            }
        }
    }
}
