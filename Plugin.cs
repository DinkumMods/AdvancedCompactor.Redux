using System;
using AdvancedCompactor.Redux;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AdvancedCompactor.Redux
{
    // Token: 0x02000003 RID: 3
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        // Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
        private void Awake()
        {
            Logger.LogInfo(Plugin.Log);
            ManualLogSource log = Plugin.Log;
            bool flag;
            BepInExInfoLogInterpolatedStringHandler bepInExInfoLogInterpolatedStringHandler = new BepInExInfoLogInterpolatedStringHandler(14, 0, out flag);
            if (flag)
            {
                bepInExInfoLogInterpolatedStringHandler.AppendLiteral("Plugin loaded!");
            }
            log.LogInfo(bepInExInfoLogInterpolatedStringHandler);
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002098 File Offset: 0x00000298
        private void Start()
        {
            Plugin.Settings_Key = base.Config.Bind<KeyCode>("compactor_godeeper", "Settings key", (UnityEngine.KeyCode)285, "Opens the seting menu. Default F4");
            Plugin.activatekey = base.Config.Bind<KeyCode>("compactor_godeeper", "activation key", (UnityEngine.KeyCode)306, "Key for the activation of going deeper. Default: LeftControl");
            Plugin.changemodekey = base.Config.Bind<KeyCode>("compactor_godeeper", "change mode key", (UnityEngine.KeyCode)108, "Key to change mode. Default: L");
            Plugin.mode = base.Config.Bind<int>("compactor_godeeper", "mode", (int)0, "0 = digging beneath the player level, 1 = digging above the player level, 2 = placing beneath the player level, 3 = placing above the player level, 4 = remove Path");
            Plugin.depth = base.Config.Bind<int>("compactor_godeeper", "depth", (int)1, "How many blocks the compactor can go down or up. Default:1");
            new Harmony("chilla55.compactor").PatchAll();
            foreach (InventoryItem inventoryItem in Inventory.Instance.allItems)
            {
                if (inventoryItem.getItemId() == 682)
                {
                    inventoryItem.anyHeight = true;
                    inventoryItem.canDamagePath = true;
                }
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002198 File Offset: 0x00000398
        private void OnDestroy()
        {
            if (this.windowShow)
            {
                CameraController.control.lockCamera(false);
                CameraController.control.cameraShowingSomething = false;
                Cursor.visible = false;
            }
            base.StopAllCoroutines();
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000021C4 File Offset: 0x000003C4
        private void OnGUI()
        {
            if (this.windowShow)
            {
                GUIStyle guistyle = new GUIStyle(GUI.skin.label);
                GUIStyle guistyle2 = new GUIStyle(GUI.skin.button);
                guistyle.fontSize = (int)(0.0129629625f * (float)Screen.height);
                guistyle2.fontSize = (int)(0.0129629625f * (float)Screen.height);
                GUI.Box(ui.createRect(10f, 265f, 332f, 235f), "Advanced Compactor Settings");
                GUI.Label(ui.createRect(18f, 290f, 250f, 22f), "Press butten to assign hotkey", guistyle);
                GUI.Label(ui.createRect(15f, 315f, 195f, 22f), string.Format("Activation Key: {0}", Plugin.activatekey.Value), guistyle);
                this.keybind_activate = GUI.Button(ui.createRect(215f, 315f, 122f, 22f), "Activation Key", guistyle2);
                GUI.Label(ui.createRect(15f, 340f, 195f, 22f), string.Format("Mode Key: {0}", Plugin.changemodekey.Value), guistyle);
                this.keybind_mode = GUI.Button(ui.createRect(215f, 340f, 122f, 22f), "Mode Switch Key", guistyle2);
                GUI.Label(ui.createRect(15f, 365f, 195f, 22f), string.Format("Settings Key: {0}", Plugin.Settings_Key.Value), guistyle);
                this.keybind_settings = GUI.Button(ui.createRect(215f, 365f, 122f, 22f), "Settings Key", guistyle2);
                GUI.Label(ui.createRect(18f, 390f, 284f, 22f), string.Format("Mode: {0}", this.Modes[Plugin.mode.Value]), guistyle);
                Plugin.mode.Value = (int)Math.Floor((double)GUI.HorizontalSlider(ui.createRect(18f, 415f, 284f, 22f), (float)Plugin.mode.Value, 0f, 4f));
                GUI.Label(ui.createRect(18f, 440f, 284f, 22f), string.Format("Depth: {0}", Plugin.depth.Value), guistyle);
                Plugin.depth.Value = (int)Math.Floor((double)GUI.HorizontalSlider(ui.createRect(18f, 465f, 284f, 22f), (float)Plugin.depth.Value, 1f, 10f));
            }
            if (this.keybind_activate)
            {
                this.keybind = 1;
            }
            if (this.keybind_settings)
            {
                this.keybind = 2;
            }
            if (this.keybind_mode)
            {
                this.keybind = 3;
            }
            if (this.keybind != 0)
            {
                GUI.Box(ui.createRect(960f, 525f, 120f, 35f), string.Format("Press key for {0}", this.keybinddisplay[this.keybind]));
                GUI.Label(ui.createRect(978f, 540f, 100f, 22f), "ESC for abort");
                if (Input.anyKeyDown)
                {
                    foreach (object obj in this.keyCodes)
                    {
                        KeyCode keyCode = (KeyCode)obj;
                        if (Input.GetKey(keyCode))
                        {
                            if (keyCode != (UnityEngine.KeyCode)27)
                            {
                                if (this.keybind == 1)
                                {
                                    Plugin.activatekey.Value = keyCode;
                                }
                                else if (this.keybind == 3)
                                {
                                    Plugin.changemodekey.Value = keyCode;
                                }
                                else if (this.keybind == 2)
                                {
                                    Plugin.Settings_Key.Value = keyCode;
                                }
                            }
                            this.keybind = 0;
                            this.keybind_finished = true;
                            break;
                        }
                    }
                }
            }
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000025C8 File Offset: 0x000007C8
        private void Update()
        {
            if (!(NetworkMapSharer.Instance.localChar == null) && this.keybind == 0 && !this.keybind_finished)
            {
                if (Input.GetKeyDown(Plugin.activatekey.Value))
                {
                    Plugin.active = true;
                }
                if (Input.GetKeyDown(Plugin.changemodekey.Value))
                {
                    Plugin.mode.Value = (Plugin.mode.Value + 1) % 5;
                    NotificationManager.manage.createChatNotification("Switched mode to " + this.Modes[Plugin.mode.Value], false);
                    Plugin.Log.LogInfo(this.Modes[Plugin.mode.Value]);
                }
                if (Input.GetKeyDown(Plugin.Settings_Key.Value))
                {
                    this.windowShow = !this.windowShow;
                    CameraController.control.lockCamera(this.windowShow);
                    CameraController.control.cameraShowingSomething = this.windowShow;
                    Cursor.visible = this.windowShow;
                }
                if (Input.GetKeyUp(Plugin.activatekey.Value))
                {
                    Plugin.active = false;
                    return;
                }
            }
            else if (this.keybind_finished)
            {
                this.keybind_finished = false;
            }
        }

        // Token: 0x04000001 RID: 1
        private readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));

        // Token: 0x04000002 RID: 2
        public string[] Modes = new string[]
        {
            "digging beneath the player level",
            "digging above the player level",
            "placing beneath the player level",
            "placing above the player level",
            "Remove Path"
        };

        // Token: 0x04000003 RID: 3
        public string[] keybinddisplay = new string[]
        {
            "",
            "Activation",
            "Settings",
            "Mode"
        };

        // Token: 0x04000004 RID: 4
        public static ConfigEntry<KeyCode> Settings_Key;

        // Token: 0x04000005 RID: 5
        public static ConfigEntry<KeyCode> activatekey;

        // Token: 0x04000006 RID: 6
        public static ConfigEntry<KeyCode> changemodekey;

        // Token: 0x04000007 RID: 7
        public static ConfigEntry<int> depth;

        // Token: 0x04000008 RID: 8
        public static ConfigEntry<int> mode;

        // Token: 0x04000009 RID: 9
        public static bool active = false;

        // Token: 0x0400000A RID: 10
        public static CharInteract myCharInteract;

        // Token: 0x0400000B RID: 11
        internal static ManualLogSource Log = new ManualLogSource("AdvancedCompactor");

        // Token: 0x0400000C RID: 12
        private bool windowShow;

        // Token: 0x0400000D RID: 13
        private bool keybind_settings;

        // Token: 0x0400000E RID: 14
        private bool keybind_activate;

        // Token: 0x0400000F RID: 15
        private bool keybind_mode;

        // Token: 0x04000010 RID: 16
        private int keybind;

        // Token: 0x04000011 RID: 17
        private bool keybind_finished;

        // Token: 0x02000006 RID: 6
        [HarmonyPatch(typeof(CharInteract), "Start")]
        private class CharInteract_Start_Patch
        {
            // Token: 0x0600000B RID: 11 RVA: 0x000027EC File Offset: 0x000009EC
            public static void Postfix(ref CharInteract __instance)
            {
                Plugin.myCharInteract = __instance;
            }
        }

        // Token: 0x02000007 RID: 7
        [HarmonyPatch(typeof(LevelTerrain), "doLevelTerrain")]
        private class LevelTerrain_doLevelTerrain_Patch
        {
            // Token: 0x0600000D RID: 13 RVA: 0x00002800 File Offset: 0x00000A00
            public static bool Prefix(ref LevelTerrain __instance)
            {
                if (!Plugin.active && Plugin.mode.Value == 0)
                {
                    __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = -1;
                    return true;
                }
                int num;
                int num2;
                if (__instance.myCharInteract.isLocalPlayer)
                {
                    num = (int)__instance.myCharInteract.selectedTile.x;
                    num2 = (int)__instance.myCharInteract.selectedTile.y;
                }
                else
                {
                    num = (int)__instance.myCharInteract.currentlyAttackingPos.x;
                    num2 = (int)__instance.myCharInteract.currentlyAttackingPos.y;
                }
                if (__instance.myCharInteract.CheckIfCanDamage(new Vector2((float)num, (float)num2)))
                {
                    if (Plugin.mode.Value == 0)
                    {
                        if (WorldManager.Instance.heightMap[num, num2] <= Mathf.RoundToInt(__instance.transform.root.position.y + 1f) && WorldManager.Instance.heightMap[num, num2] >= Mathf.RoundToInt(__instance.transform.root.position.y + 1f) - Plugin.depth.Value && !WorldManager.Instance.CheckTileClientLock(num, num2))
                        {
                            __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = -1;
                            __instance.myCharInteract.doDamage(true);
                            return false;
                        }
                    }
                    else if (Plugin.mode.Value == 1)
                    {
                        if (Plugin.active)
                        {
                            if (WorldManager.Instance.heightMap[num, num2] >= Mathf.RoundToInt(__instance.transform.root.position.y + 1f) && WorldManager.Instance.heightMap[num, num2] <= Mathf.RoundToInt(__instance.transform.root.position.y + 1f) + Plugin.depth.Value && !WorldManager.Instance.CheckTileClientLock(num, num2))
                            {
                                __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = -1;
                                __instance.myCharInteract.doDamage(true);
                                return false;
                            }
                        }
                        else if (WorldManager.Instance.heightMap[num, num2] == Mathf.RoundToInt(__instance.transform.root.position.y + 1f) && !WorldManager.Instance.CheckTileClientLock(num, num2))
                        {
                            __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = -1;
                            __instance.myCharInteract.doDamage(true);
                            return false;
                        }
                    }
                    else if (Plugin.mode.Value == 2)
                    {
                        if (Plugin.active)
                        {
                            if (WorldManager.Instance.heightMap[num, num2] <= Mathf.RoundToInt(__instance.transform.root.position.y - 1f) && WorldManager.Instance.heightMap[num, num2] >= Mathf.RoundToInt(__instance.transform.root.position.y - 1f) - Plugin.depth.Value && !WorldManager.Instance.CheckTileClientLock(num, num2))
                            {
                                __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = 1;
                                __instance.myCharInteract.doDamage(true);
                                return false;
                            }
                        }
                        else if (WorldManager.Instance.heightMap[num, num2] == Mathf.RoundToInt(__instance.transform.root.position.y - 1f) && !WorldManager.Instance.CheckTileClientLock(num, num2))
                        {
                            __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = 1;
                            __instance.myCharInteract.doDamage(true);
                            return false;
                        }
                    }
                    else if (Plugin.mode.Value == 3)
                    {
                        if (Plugin.active)
                        {
                            if (WorldManager.Instance.heightMap[num, num2] >= Mathf.RoundToInt(__instance.transform.root.position.y) && WorldManager.Instance.heightMap[num, num2] <= Mathf.RoundToInt(__instance.transform.root.position.y) + Plugin.depth.Value && !WorldManager.Instance.CheckTileClientLock(num, num2))
                            {
                                __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = 1;
                                __instance.myCharInteract.doDamage(true);
                                return false;
                            }
                        }
                        else if (WorldManager.Instance.heightMap[num, num2] == Mathf.RoundToInt(__instance.transform.root.position.y) && !WorldManager.Instance.CheckTileClientLock(num, num2))
                        {
                            __instance.myCharInteract.myEquip.itemCurrentlyHolding.changeToHeightTiles = 1;
                            __instance.myCharInteract.doDamage(true);
                            return false;
                        }
                    }
                    else if (Plugin.mode.Value == 4 && !WorldManager.Instance.CheckTileClientLock(num, num2) && WorldManager.Instance.tileTypes[WorldManager.Instance.tileTypeMap[(int)__instance.myCharInteract.selectedTile.x, (int)__instance.myCharInteract.selectedTile.y]].dropOnChange)
                    {
                        StatusManager.manage.changeStamina(-__instance.myCharInteract.myEquip.itemCurrentlyHolding.getStaminaCost() / 3f);
                        Inventory.Instance.useItemWithFuel();
                        __instance.myCharInteract.ChangeTileType(0);
                    }
                    if (WorldManager.Instance.heightMap[num, num2] > Mathf.RoundToInt(__instance.transform.root.position.y + 1f))
                    {
                        __instance.myAnim.SetTrigger("Clang");
                        return false;
                    }
                }
                else if (__instance.myAnim)
                {
                    __instance.myAnim.SetTrigger("Clang");
                }
                return false;
            }
        }
    }
}