using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Manager;
using UnityEngine;
using UnityEngine.VR;
using VRKanojo;

namespace VRK_Plugins
{
    [BepInPlugin(GUID, Name, Version)]
    public class PlayWithoutVrPlugin : BaseUnityPlugin
    {
        public const string Name = "Play Without VR Connected";
        public const string Version = "1.0";
        public const string GUID = "PlayWithoutVR";

        private static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            Harmony.CreateAndPatchAll(typeof(PlayWithoutVrPlugin));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InitScene2), "CheckHMD")]
        private static void AllowNoHmdPatch(ref bool __result)
        {
            if (!__result && !VRDevice.isPresent)
            {
                Singleton<Game>.Instance.playCount++;
                GlobalVRKanojo.ForceMouse_ = true;
                __result = true;

                Logger.LogMessage("No HMD was found, entering mouse control mode.");
                Logger.LogMessage("- Hold buttons and drag to move camera.");
                Logger.LogMessage("- Left button for yes, Right button for no.");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LogoScene), "Start")]
        private static void AllowNoHmdPatch2()
        {
            if (!VRDevice.isPresent)
            {
                var mainGameObject = Camera.main.gameObject;
                if (!mainGameObject.GetComponent<IllusionCamera>())
                {
                    var cam = mainGameObject.AddComponent<IllusionCamera>();
                    cam.Set(new Vector3(0.65f, 1.02f, 0), Vector3.zero, 0.73f);
                }
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && !VRDevice.isPresent)
            {
                // Fix lockState getting reset after losing focus
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
}
