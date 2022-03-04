using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using QModManager.API.ModLoading;
using Logger = QModManager.Utility.Logger;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace SnowFoxQuantumLocker
{
    [QModCore]
    public static class QMod
    {
        public const string version = "1.0.1.0";

        public static Config Config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        [QModPatch]
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var testMod = ($"Nagorogan_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {testMod}");
            Harmony harmony = new Harmony(testMod);
            harmony.PatchAll(assembly);
            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }
    }
    [Menu("SnowFox Quantum Locker")]
    public class Config : ConfigFile
    {
        [Keybind("Quantum Locker Keybind", Tooltip = "When on the snowfox, press this key to open the built in quantum locker")]
        public KeyCode LockerKey = KeyCode.C;
        [Choice("Locker Type", new[] {"Standard", "Quantum", "Snowfox"}, Tooltip = "Decides what type of locker the snowfox will have. Standard is a standard locker, Quantum is a quantum locker that shares inventory with all other quantum lockers, Snowfox is a locker that shares inventory with all other snowfoxes")]
        public string LockerType = "Standard";
    }
}