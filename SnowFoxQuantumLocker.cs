using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FasterSnowFoxCharging;
using HarmonyLib;
using UnityEngine;
using Logger = QModManager.Utility.Logger;
using SMLHelper.V2.Assets;

namespace SnowFoxQuantumLocker
{
    internal class SnowFoxQuantumLocker
    {
        [HarmonyPatch(typeof(Hoverbike))]
        [HarmonyPatch("Update")]
        internal class PatchHoverbikeUpdate
        {
            [HarmonyPostfix]

            public static void Postfix(Hoverbike __instance)
            {
                string summonKey = QMod.Config.LockerKey; //still not working, just set it to "C" at default for now and never bothered looking into fixing the config
                if (string.IsNullOrWhiteSpace(summonKey))
                {
                    return;//stole this from some other mod, don't think it's needed but can't hurt so whatever
                }
                if (GameInput.GetKeyDown((KeyCode)summonKey.ToLower()[0]) && __instance.GetPilotingCraft())//GetPilotingCraft() just returns true if the player is currently riding the snowfox
                {
                    StorageContainer container = __instance.GetComponentInChildren<StorageContainer>(true);
                    if(container && (QMod.Config.LockerType.Equals("Standard") || QMod.Config.LockerType.Equals("Quantum")))
                    {
                        container.Open(container.transform);
                    }else if(QMod.Config.LockerType.Equals("Snowfox"))
                    {
                        //ignore this
                        //SnowfoxSharedStorage.storageContainer.Open(SnowfoxSharedStorage.storageContainer.transform);
                    }
                }
            }
        }
        [HarmonyPatch(typeof (Hoverbike), nameof(Hoverbike.Awake))]
        internal class PatchHoverbikeStart
        {
            [HarmonyPostfix]
            public static void Postfix(Hoverbike __instance)
            {
                var storageRoot = __instance.transform.Find("StorageRoot")?.gameObject;//Tris to check if the snowfox already has a storage container on it from a previous play session
                StorageContainer storageContainer;
                if(storageRoot == null)
                {
                    storageRoot = new GameObject("StorageRoot"); // make a new object
                    storageRoot.transform.SetParent(__instance.transform); // set it a child of the parent            //these are metious' comments, obviously quite simplistic but it feels rude to delete them I guess
                    storageRoot.SetActive(false); //this was because the storage container was trying to create itself before we could set its values and would cause a null ref exception

                    //I don't even remember what half of this shit does, all I know is that me and mrpurple had no idea for a while and were just changing things between being on the snowfox and on the storageRoot
                    //I still don't know which it's supposed to be on.
                    var coi = __instance.gameObject.GetComponentInChildren<ChildObjectIdentifier>();
                    if(coi == null)
                    {
                        Logger.Log(Logger.Level.Warn,"Oh Shit coi is null", null, true);
                    }
                    var pi = __instance.gameObject.GetComponent<PrefabIdentifier>();
                    if(pi == null)
                    {
                        Logger.Log(Logger.Level.Warn, "Oh shit, pi is null", null, true);
                    }
                    else
                    {
                        coi.classId = pi.classId;
                    }
                    storageContainer = storageRoot.EnsureComponent<StorageContainer>();
                    storageContainer.prefabRoot = __instance.gameObject;
                    storageContainer.storageRoot = coi;

                    storageContainer.width = 5;
                    storageContainer.height = 5;
                    storageContainer.storageLabel = "Snow Fox Storage";
                    storageRoot.SetActive(true); 
                }else
                {
                    storageContainer = storageRoot.GetComponent<StorageContainer>();
                }
                
                if(QMod.Config.LockerType.Equals("Standard"))
                {
                    //Current don't have a need to execute unique code under this condition, but I thought I probably would so I kept it
                    //do stuff
                }
                else if(QMod.Config.LockerType.Equals("Quantum"))
                {
                    var snowfoxQuantum = __instance.gameObject.EnsureComponent<SnowfoxQuantumStorage>();
                    snowfoxQuantum.storageContainer = storageContainer; // assign it the storage container you created earlier here
                }
                else if(QMod.Config.LockerType.Equals("Snowfox"))
                {
                    //haven't done this yet. not entirely sure how to
                    //__instance.gameObject.AddComponent<SnowfoxSharedStorage>();
                }
            }
        }
        //makes sure that this component's storagecontainer is always equal to the quantum locker main
        class SnowfoxQuantumStorage : MonoBehaviour, IProtoEventListener
        {
            public StorageContainer storageContainer;

            private bool synced;//what the fuck does this even do? We never set it to false and only set it to true at the end of the update method so when the fuck do we use it?
            private bool loadedFromSaveGame;

            void Update()
            {
                if (!synced)
                {
                    var quantumStorage = QuantumLockerStorage.GetStorageContainer(!loadedFromSaveGame); //I believe I could just do GetStorageContainer(true) but Lee yelled at me for that so I dunno
                    if (quantumStorage != null)
                    {
                        storageContainer.SetContainer(quantumStorage.container);
                        synced = true;
                    }
                }
            }

            public void OnProtoSerialize(ProtobufSerializer serializer)
            {
            }

            public void OnProtoDeserialize(ProtobufSerializer serializer)
            {
                loadedFromSaveGame = true;
            }
        }
        //unused currently. just ignore, don't feel like cleaning it up
        internal class SharedStorage : Spawnable
        {   
            public SharedStorage() : base(classId: "SharedSnowfoxStorage", friendlyName: "Shared Snowfox Storage", description: "Random Description here")
            {
            }
            public override GameObject GetGameObject()
            {
                var prefab = new GameObject();
                prefab.SetActive(false);
                prefab.AddComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<MainStorage>();
                return prefab;
            }
            /*public override List<SpawnLocation> CoordinatedSpawns => new List<Vector3> {new Vector3(425, 60, 55)};
            CoordinatedSpawns = new List<Spawnable.SpawnLocation>()
				{
					new Spawnable.SpawnLocation(new Vector3(425, 60, 55), new Vector3(344f, 3.77f, 14f))
				}*/
        }
        //used by SnowfoxQuantumStorage
        class MainStorage : MonoBehaviour
        {
            public static MainStorage main;

            private void Awake()
            {
                main = this;
            }
        }
        /* This was me fucking around after mrpurple told me to try adding a storage container to an object that did not already have one so we(he) could see if it was the snowfox battery or upgrade modules storage fucking it over or not
         *Still don't know, I never got this to work either. Pretty sure I could open it and put stuff in it but it would never save. idk
        [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.Start))]
        public class SolarPanelPatch 
        {
            [HarmonyPrefix]
            public static void Prefix(SolarPanel __instance)
            {
                var storageRoot = __instance.transform.Find("StorageRoot")?.gameObject;
                StorageContainer storageContainer;
                if(storageRoot == null)
                {
                    storageRoot = new GameObject("StorageRoot"); // make a new object
                    storageRoot.transform.SetParent(__instance.transform); // set it a child of the parent
                    storageRoot.transform.SetAsFirstSibling();
                    storageRoot.SetActive(false); 
                    var coi = storageRoot.EnsureComponent<ChildObjectIdentifier>();
                    if(coi == null)
                    {
                        Logger.Log(Logger.Level.Warn,"Oh Shit coi is null", null, true);
                    }
                    var pi = __instance.gameObject.GetComponent<PrefabIdentifier>();
                    if(pi == null)
                    {
                        Logger.Log(Logger.Level.Warn, "Oh shit, pi is null", null, true);
                    }
                    else
                    {
                        coi.classId = pi.classId;
                    }
                    storageContainer = storageRoot.EnsureComponent<StorageContainer>();
                    storageContainer.prefabRoot = __instance.gameObject;
                    storageContainer.storageRoot = coi;

                
                
                    // this step is improtant for the game to recognize the storage as "saveable"

                    storageContainer.width = 5;
                    storageContainer.height = 5;
                    storageContainer.storageLabel = "Snow Fox Storage";
                    storageRoot.SetActive(true); 
                }else
                {
                    storageContainer = storageRoot.GetComponent<StorageContainer>();
                }
            }
        }
        [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.Update))]
        public class SolarPanelUpdate 
        {
            [HarmonyPostfix]
            public static void Postfix(SolarPanel __instance)
            {
                string summonKey = QMod.Config.LockerKey;
                if(GameInput.GetKeyDown((KeyCode)summonKey.ToLower()[0]))
                {
                    StorageContainer Container = __instance.GetComponentInChildren<StorageContainer>();
                    Container.Open(Container.transform);
                }
            }
        }*/
    }
}