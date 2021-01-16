using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using XUnity.ResourceRedirector;

namespace VRK_Plugins
{
    [BepInDependency("gravydevsupreme.xunity.resourceredirector")]
    [BepInPlugin(GUID, Name, Version)]
    public class UncensorLoaderPlugin : BaseUnityPlugin
    {
        public const string Name = "Uncensor Loader";
        public const string Version = "1.0";
        public const string GUID = "UncensorLoader";

        private static new ManualLogSource Logger;

        private const string NoUncensorValue = "None (Censored)";
        private static ConfigEntry<string> _maleUncSetting;
        private static ConfigEntry<string> _femaleUncSetting;

        private static List<UncensorInfo> _uncensors;

        private void Awake()
        {
            Logger = base.Logger;

            LoadUncensors();

            _maleUncSetting = Config.Bind("Uncensors", "Male uncensor",
                _uncensors.FirstOrDefault(x => x.Type == UncensorType.Male)?.Name ?? NoUncensorValue,
                new ConfigDescription("Which uncensor to use for the player. Uncensors are loaded from BepInEx/Uncensors. You might need to restart the game to apply this setting.",
                    new AcceptableValueList<string>(_uncensors.Where(x => x.Type == UncensorType.Male).Select(x => x.Name).ToArray().AddToArray(NoUncensorValue))));

            _femaleUncSetting = Config.Bind("Uncensors", "Female uncensor",
                _uncensors.OrderByDescending(x => x.Name.Contains("Modelled pussy")).FirstOrDefault(x => x.Type == UncensorType.Female)?.Name ?? NoUncensorValue,
                new ConfigDescription("Which uncensor to use for the heroine. Uncensors are loaded from BepInEx/Uncensors. You might need to restart the game to apply this setting.",
                    new AcceptableValueList<string>(_uncensors.Where(x => x.Type == UncensorType.Female).Select(x => x.Name).ToArray().AddToArray(NoUncensorValue))));

            ResourceRedirection.RegisterAssetBundleLoadingHook(UncensorBundleReplaceHook);

            Harmony.CreateAndPatchAll(typeof(UncensorLoaderPlugin));
        }

        private static void LoadUncensors()
        {
            _uncensors = new List<UncensorInfo>();

            var uncPath = Path.Combine(Paths.BepInExRootPath, "Uncensors");
            var upi = new DirectoryInfo(uncPath);
            upi.Create();
            var infos = upi.GetFiles("UncensorInfo.xml", SearchOption.AllDirectories);

            foreach (var fileInfo in infos)
            {
                try
                {
                    var info = UncensorInfo.LoadFromFile(fileInfo);

                    Logger.LogInfo($"Found {info.Type} uncensor: {info.Name}");

                    _uncensors.Add(info);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Failed to load uncensor at {fileInfo.FullName} - " + e);
                }
            }

            Logger.LogInfo("Finished loading uncensors");
        }

        private static UncensorInfo GetSelectedUncensor(UncensorType type)
        {
            var setting = type == UncensorType.Male ? _maleUncSetting.Value : _femaleUncSetting.Value;

            return _uncensors.FirstOrDefault(x => x.Type == type && x.Name == setting);
        }

        private static void UncensorBundleReplaceHook(AssetBundleLoadingContext context)
        {
            var uncf = GetSelectedUncensor(UncensorType.Female);
            var uncm = GetSelectedUncensor(UncensorType.Male);

            var normalizedPath = context.GetNormalizedPath();
            //Console.WriteLine("BUNDLE: " + normalizedPath);

            if (uncf != null && uncf.ReplacementBundles.TryGetValue(normalizedPath, out var replacement))
            {
                Logger.LogInfo("Applying uncensor to heroine: " + uncf.Name);
                Logger.LogDebug($"Replacing `{normalizedPath}` with `{replacement}`");
                context.Parameters.Path = replacement;
            }
            else if (uncm != null && uncm.ReplacementBundles.TryGetValue(normalizedPath, out replacement))
            {
                Logger.LogInfo("Applying uncensor to player: " + uncm.Name);
                Logger.LogDebug($"Replacing `{normalizedPath}` with `{replacement}`");
                context.Parameters.Path = replacement;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharBody), "MozInit")]
        private static void DemosaicPatch(ref string[] mozObjName)
        {
            mozObjName = mozObjName.Where(x =>
            {
                // Disable mosaic when any uncensor is loaded
                if (GetSelectedUncensor(UncensorType.Male) != null && x.StartsWith("cm_O_dan")) return false;
                if (GetSelectedUncensor(UncensorType.Female) != null && x.StartsWith("cf_O_mn")) return false;
                return true;
            }).ToArray();
        }
    }
}