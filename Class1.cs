using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace plugin_test
{
    [BepInPlugin("yuzhou.test", "test", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin is loaded!");
            HarmonyFileLog.Enabled = true;
            Harmony.CreateAndPatchAll(typeof(MyLobbyController));
            Harmony.CreateAndPatchAll(typeof(MyBlorfGamePlay));
            Harmony.CreateAndPatchAll(typeof(MyCharController));
            Harmony.CreateAndPatchAll(typeof(MyDiceGamePlay));
        }
    }

    [HarmonyPatch(typeof(BlorfGamePlay))]
    internal class MyBlorfGamePlay
    {
        static AccessTools.FieldRef<BlorfGamePlay, int> revolverbullletRef = AccessTools.FieldRefAccess<BlorfGamePlay, int>("revolverbulllet");

        [HarmonyPatch("WaitforRevolver")]
        [HarmonyPrefix]
        static void WaitforRevolver(BlorfGamePlay __instance)
        {
            if(MyCharController.open4)revolverbullletRef(__instance) = 9999;
        }
    }


    [HarmonyPatch(typeof(CharController))]
    internal class MyCharController
    {
        private static float sinValue = 0f;
        private static bool open1 = false;
        private static bool open2 = false;
        private static bool open3 = false;
        public static bool open4 = false;

        [DllImport("kernel32.dll")]
        public static extern bool Beep(int freq, int duration);


        static void dodo(bool a)
        {
            if (a)
            {
                Beep(800, 500);
            }
            else
            {
                Beep(500, 500);
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void Update(CharController __instance)
        {
            if (!__instance.isOwned) return;
            float zoomSpeed = 0.1f;
            Vector3 vector = Vector3.zero;

            if (Input.GetKey(KeyCode.Keypad4))
            {
                vector += Vector3.forward * zoomSpeed;
            }
            if (Input.GetKey(KeyCode.Keypad6))
            {
                vector += Vector3.back * zoomSpeed;
            }
            if (Input.GetKey(KeyCode.Keypad8))
            {
                vector += Vector3.left * zoomSpeed;
            }
            if (Input.GetKey(KeyCode.Keypad2))
            {
                vector += Vector3.right * zoomSpeed;
            }
            //float axis = Input.GetAxis("Mouse Y");
            //float axis2 = Input.GetAxis("Mouse X");

            //vector += Vector3.up * axis * zoomSpeed2;
            //vector += Vector3.right * axis2 * zoomSpeed2;

            //__instance.VRCamera.transform.RotateAround(__instance.VRCamera.transform.position, __instance.VRCamera.transform.right, axis * -zoomSpeed);
            //__instance.VRCamera.transform.RotateAround(__instance.VRCamera.transform.position, Vector3.up, axis2 * zoomSpeed);
            __instance.HeadPivot.transform.Translate(vector);

            vector = Vector3.zero;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                vector += Vector3.forward * zoomSpeed;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                vector += Vector3.back * zoomSpeed;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                vector += Vector3.left * zoomSpeed;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                vector += Vector3.right * zoomSpeed;
            }
            if(Input.GetKeyDown(KeyCode.F3))
            {
                open1 = !open1;
                dodo(open1);
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                open2 = !open2;
                dodo(open2);
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                open3 = !open3;
                dodo(open3);
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                open4 = !open4;
                dodo(open4);
            }
            if (open1) { vector.y += (float)(Math.Sin(sinValue++) * 0.01); }
            if (open2) { __instance.transform.Rotate(new Vector3(0, 2f, 0), Space.Self); }
            if (open3) { __instance.transform.Rotate(new Vector3(0, 0, 2f), Space.Self); }
            
            __instance.transform.Translate(vector);
        }
    }


    [HarmonyPatch(typeof(LobbyController))]
    internal class MyLobbyController
    {
        static AccessTools.FieldRef<LobbyController, GameObject> StartButtonActive = AccessTools.FieldRefAccess<LobbyController, GameObject>("StartButtonActive");
        static AccessTools.FieldRef<LobbyController, GameObject> StartButtonPassive = AccessTools.FieldRefAccess<LobbyController, GameObject>("StartButtonPassive");

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update(ref LobbyController __instance)
        {
            StartButtonActive(__instance).SetActive(true);
            StartButtonPassive(__instance).SetActive(false);
        }
    }


    [HarmonyPatch(typeof(DiceGamePlay))]
    internal class MyDiceGamePlay
    {
        static AccessTools.FieldRef<DiceGamePlay, PlayerStats> playerStats = AccessTools.FieldRefAccess<DiceGamePlay, PlayerStats>("playerStats");

        [HarmonyPatch("UpdateCall")]
        [HarmonyPrefix]
        static void UpdateCall(ref DiceGamePlay __instance)
        {
            if(__instance.isOwned && MyCharController.open4)
            {
                playerStats(__instance).Dead = false;
                playerStats(__instance).NetworkDead = false;
            }
        }
    }
}
