using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Manager;
using UnityEngine;
using UnityEngine.UI;
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

                Logger.LogMessage("No HMD was found! Entering mouse control mode.");
                Logger.LogMessage("- Hold mouse buttons and drag to rotate the camera.");
                Logger.LogMessage("- Use arrow keys to move the camera.");
                Logger.LogMessage("- Press Left Mouse Button for \"Yes\", and Right Mouse Button for \"No\".");
                Logger.LogMessage("- Center your camera on buttons and blue head icons to select them.");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LogoScene), "Start")]
        private static void AllowNoHmdPatch2()
        {
            if (!VRDevice.isPresent)
            {
                var mainGameObject = Camera.main.gameObject;
                var rig = mainGameObject.GetComponentInParent<OVRCameraRig>();
                if (!rig.GetComponent<IllusionCamera>())
                {
                    var cam = rig.gameObject.AddComponent<IllusionCamera>();
                    cam.Set(new Vector3(0.65f, 1.02f, 0), Vector3.zero, 0.73f);

                    // Need to reset the camera settings
                    rig.gameObject.SetActive(false);
                    rig.gameObject.SetActive(true);

                    var look = mainGameObject.transform.FindChild("Look");
                    look.localPosition = Vector3.zero;
                    look.localEulerAngles = Vector3.zero;

                    CreateCrosshair();
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

        private static void CreateCrosshair()
        {
            var managerObj = Chainloader.ManagerObject.transform;
            if (managerObj.FindChild("CrosshairCanvas")) return;

            var cursorData = ResourceUtils.GetEmbeddedResource("crosshair.png") ?? throw new Exception("crosshair.png resource not found");

            var crosshairRoot = new GameObject("CrosshairCanvas");
            crosshairRoot.transform.parent = managerObj;

            var canvas = crosshairRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            crosshairRoot.AddComponent<CanvasRenderer>();

            var sc = crosshairRoot.AddComponent<CanvasScaler>();
            sc.referenceResolution = new Vector2(1920, 1080);
            sc.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;

            var imageTr = new GameObject("CrosshairImage");
            imageTr.transform.parent = crosshairRoot.transform;
            imageTr.transform.localPosition = Vector3.zero;
            imageTr.transform.localEulerAngles = Vector3.zero;
            imageTr.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            var cursorTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            cursorTex.LoadImage(cursorData);
            var imageComp = imageTr.AddComponent<Image>();
            imageComp.sprite = Sprite.Create(cursorTex, new Rect(0, 0, 300, 300), new Vector2(0.5f, 0.5f));
        }
    }
}
