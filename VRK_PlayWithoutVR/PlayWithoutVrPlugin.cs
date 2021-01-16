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
        private static PlayWithoutVrPlugin Instance;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Harmony.CreateAndPatchAll(typeof(PlayWithoutVrPlugin));

            enabled = false;
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
                Logger.LogMessage("- Scroll mouse wheel in H scenes to change speed. You can't touch.");
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
                    cam.Set(new Vector3(0.65f, 1.02f, 0), Vector3.zero, 1f);

                    // Need to reset the camera settings
                    rig.gameObject.SetActive(false);
                    rig.gameObject.SetActive(true);

                    var look = mainGameObject.transform.FindChild("Look");
                    look.localPosition = Vector3.zero;
                    look.localEulerAngles = Vector3.zero;

                    CreateCrosshair();
                    Instance.enabled = true;
                }

                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!VRDevice.isPresent)
            {
                // Fix lockState getting reset after losing focus
                Cursor.lockState = hasFocus ? CursorLockMode.Confined : CursorLockMode.None;
            }
        }

        private static CanvasGroup _cursorCg;
        private float _inactiveTime = 5;

        private void Update()
        {
            if (!_cursorCg) return;

            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
                _inactiveTime = 0;

            _inactiveTime += Time.deltaTime;

            _cursorCg.alpha = Mathf.Max(0, 1f - (_inactiveTime - 4));
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

            _cursorCg = crosshairRoot.AddComponent<CanvasGroup>();
            _cursorCg.interactable = false;
            _cursorCg.blocksRaycasts = false;
            _cursorCg.alpha = 1;

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
