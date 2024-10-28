using System.Collections.Generic;
using System.Linq;
using ADV.Commands.Base;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KK_SensibleH.Caress;
using KKAPI.MainGame;
using UnityEngine;
using static KK_SensibleH.Caress.Kiss;
using UniRx;
using KKAPI;

namespace KK_SensibleH
{
    [BepInPlugin(GUID, "KK_SensibleH", Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInDependency(KoikatuAPI.GUID)]
#if KK
    [BepInDependency(KK_VR.VRPlugin.GUID)]
#else
    [BepInDependency(KKS_VR.VRPlugin.GUID)]
#endif
    [BepInDependency(KK_BetterSquirt.BetterSquirt.GUID)] // F it, normal version stays in dependencies, no clue how to pass unknown enum type to the delegate.

    public class SensibleH : BaseUnityPlugin
    {
        public const string GUID = "kk.sensible.h";
        public const string Version = "0.1.1";
        public new static PluginInfo Info { get; private set; }
        public new static ManualLogSource Logger;
        public static ConfigEntry<AutoModeKind> AutoMode { get; set; }
        public static ConfigEntry<AutoPoseType> AutoPickPose{ get; set; }
        public static ConfigEntry<bool> AutoRestartAction { get; set; }
        public static ConfigEntry<EdgeType> Edge { get; set; }
        public static ConfigEntry<bool> MomiMomi { get; set; }
        public static ConfigEntry<bool> EyeNeckControl { get; set; }
        public static ConfigEntry<bool> HoldPubicHair { get; set; }
        public static ConfigEntry<bool> DisablePeskySounds { get; set; }
#if KKS
        public static ConfigEntry<bool> ProlongObi { get; set; }
#endif
        public static ConfigEntry<float> ActionFrequency { get; set; }
        public static ConfigEntry<float> EdgeFrequency { get; set; }
        public static ConfigEntry<float> NeckLimit { get; set; }
        public static ConfigEntry<int> GaugeSpeed { get; set; }
        public static ConfigEntry<FrenchType> FrenchKiss { get; set; }
        public static ConfigEntry<int> KissEyesLimit { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey2 { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey3 { get; set; }
        public static bool MoveNeckGlobal;
        public static int[] EyeNeckPtn = { -1, -1, -1 };

        internal static List<GirlController> _girlControllers;
        internal static HandCtrl _handCtrl;
        internal static HandCtrl _handCtrl1;
        internal static HMotionEyeNeckFemale _eyeneckFemale;
        internal static HMotionEyeNeckFemale _eyeneckFemale1;
        internal static HFlag hFlag;
        internal static List<ChaControl> _chaControl;
        internal static ChaControl _chaControlM;
        internal static HVoiceCtrl _hVoiceCtrl;
        internal static HSprite _sprite;
        internal static GameObject MalePoI;
        internal static GameObject[] FemalePoI;

        internal static float BiasF;
        internal static float BiasM;
        internal static float gaugeMultiplier;
        internal static bool OLoop;
        internal static bool MoMiActive;
        internal static bool FirstTouch;
        internal static int CurrentMain;
        internal static int MaleOrgCount;
        internal static bool SuppressVoice;
        internal static bool OverrideSquirt;
        //internal static bool BetterSquirtEnabled;
        internal static bool[] IsNeckSet = new bool[2];
        internal static Dictionary<string, int> LstHeroine;
        //internal static float[] NeckChangeRate = { 1f, 1f };
        internal static AnimatorStateInfo sLoopInfo;
        //internal delegate bool RunSquirt(bool softSE, FakeType trigger, bool sound, MonoBehaviour handCtrl, bool setTouchCooldown);
        //internal static RunSquirt RunSquirtsDelegate;
        public enum AutoModeKind
        {
            Disabled,
            PromptAtStart,
            PromptAtStartAndFinish,
            //PromptAtStartAndDisableOnFinish,
            Automatic
        }
        public enum AutoPoseType
        {
            Disabled,
            OnlyService,
            OnlyIntercourse,
            FemdomOnly,
            AllPositions
        }
        public enum EdgeType
        {
            Disabled,
            Outside,
            Inside,
            Both
        }

        public void Awake()
        {
            Logger = base.Logger;

            AutoMode = Config.Bind(
                section: "AutoMode",
                key: "AutoMode",
                defaultValue: AutoModeKind.Disabled,
                "Minimizes the need for inputs from the player during H.\n" +
                "Disabled - Waits for setting to change any moment.\n" +
                "BeginWithPrompt - For action to begin necessary:\n" +
                "Click on any interactable element of the lower half of the screen(except leaving H) or Touch or Kiss\n" +
                "BeginAndProceedWithPrompt - Same as above but now after climax too.\n" +
                "Automatic - Will start/finish action, change position (If setting is enabled) and restart action (If setting is enabled will do it considerably faster) without any user input.\n" +
                "Warning. All proactive functions have mild disrespect for persistent user input, bad things (that will be solved only by the reboot of the game) may happen " +
                "for those who seek two pilots in one seat." 
                );
            AutoPickPose = Config.Bind(
                section: "AutoMode",
                key: "PositionChange",
                defaultValue: AutoPoseType.AllPositions,
                "Allows auto change of positions after climax.\n" +
                "Disabled - Waits for setting to change any moment.\n" +
                "FemdomOnly - Choses only position where the girl is dominant. By default it's only one (game's default) cowgirl position. With modified AnimationLoader manifest comes much more.\n" +
                "AllPositions - Choses random non caress animation found.\n" +
                "If both AutoPositionChange and AutoRestart are enabled, position change takes a bit of precedence."
                );
            AutoRestartAction = Config.Bind(
                section: "AutoMode",
                key: "Restart",
                defaultValue: true,
                "With  AutoPositionChange enabled, attempts to restart action after climax (and all the voices). If unsuccessful, changes position.\n" +
                "With AutoPositionChange disabled,  restarts action after climax (and all the voices). \n" +
                "Even if disabled, as long as AutoMode is in functional state (enabled and if necessary with inputs from user) sooner or later " +
                "restart will happen."
                );
            Edge = Config.Bind(
                section: "AutoMode",
                key: "Edge",
                defaultValue: EdgeType.Outside,
                "Allows participants to pull out/stop for a moment " +
                "for whatever reason it may be. Available in service and intercourse.\n" +
                "Requires enabled AutoMode."
                );
            ActionFrequency = Config.Bind(
                section: "AutoMode",
                key: "ChangeFrequency",
                defaultValue: 1f,
                new ConfigDescription("Frequency of manipulations in AutoMode.\n" +
                "Lesser value -> smaller pause between actions.",
                new AcceptableValueRange<float>(0.1f, 2f))
                ); 
            EdgeFrequency = Config.Bind(
                section: "AutoMode",
                key: "EdgeFrequency",
                defaultValue: 1f,
                new ConfigDescription("Cooldown of edge mode.\n" +
                "Lesser value -> smaller pause between edges.",
                new AcceptableValueRange<float>(0.1f, 2f))
                );
            NeckLimit = Config.Bind(
                section: "Tweaks",
                key: "NeckLimit",
                defaultValue: 1f,
                new ConfigDescription("Adjust the limits of neck movements, 1.0 being the default value.\n" +
                "Changes take place on scene reload or new position.",
                new AcceptableValueRange<float>(0.5f, 1.5f))
                );
            EyeNeckControl = Config.Bind(
                section: "Tweaks",
                key: "EyeNeckControl",
                defaultValue: true,
                "Allow plugin to introduce alternative control of eyes and neck."
                );
            HoldPubicHair = Config.Bind(
                section: "Tweaks",
                key: "HoldPubicHair",
                defaultValue: true,
                "Hold the scale of pubic hair accessory attached to the crouch."
                );
            //AutoADV = Config.Bind(
            //    section: "Tweaks",
            //    key: "ADV Auto",
            //    defaultValue: true,
            //    "Enables auto mode in any text scenario by default."
            //    );
            DisablePeskySounds = Config.Bind(
                section: "Tweaks",
                key: "Disable button click sfx",
                defaultValue: true,
                "."
                );
            GaugeSpeed = Config.Bind(
                section: "Tweaks",
                key: "Excitement slowdown",
                defaultValue: 4,
                new ConfigDescription("Decreases the speed of excitement gauge increase by value times.",
                new AcceptableValueRange<int>(1, 5))
                );
#if KKS
            ProlongObi = Config.Bind(
                section: "Tweaks",
                key: "Fluids stay longer",
                defaultValue: true,
                "Increases the time when ejaculation fluid is present."
                );
#endif
            FrenchKiss = Config.Bind(
                section: "Caress",
                key: "KissType",
                defaultValue: Kiss.FrenchType.Auto,
                "Set alternative type of the mouth during kiss.\n" +
                "Disabled - Waits for setting to change any moment.\n" +
                "Auto - Changes mouth with certain probability based on random/girl's H experience/girl's excitement.\n" +
                "Always - Self explanatory."
                );
            KissEyesLimit = Config.Bind(
                section: "Caress",
                key: "KissEyes",
            defaultValue: 50,
            new ConfigDescription("Maximum openness of eyes and eyelids during kissing.\n" +
            "Set to 0 to keep eyes closed during kiss",
            new AcceptableValueRange<int>(0, 100))
                );
            MomiMomi = Config.Bind(
                section: "Caress",
                key: "MomiMomi",
                defaultValue: true,
                "Attach items (hands/tongue/etc) to girl's points of interest then press and hold the mouse button for a second (or trigger if in MainGameVR) " +
                "and enjoy items moving by themselves (button may be released). A click anywhere to stop it. This setting is just a description."
                );

            Cfg_TestKey = Config.Bind(
                section: "SensibleH",
                key: "TestKey1",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );
            Cfg_TestKey2 = Config.Bind(
                section: "SensibleH",
                key: "TestKey2",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );
            Cfg_TestKey3 = Config.Bind(
                section: "SensibleH",
                key: "TestKey3",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );



            sLoopInfo = new AnimatorStateInfo();
            object dummyInfo = sLoopInfo;
            Traverse.Create(dummyInfo).Field("m_Name").SetValue(-1715982390);
            Traverse.Create(dummyInfo).Field("m_SpeedMultiplier").SetValue(3f);
            Traverse.Create(dummyInfo).Field("m_Speed").SetValue(1f);
            Traverse.Create(dummyInfo).Field("m_Loop").SetValue(1);
            Traverse.Create(dummyInfo).Field("m_NormalizedTime").SetValue(59.73729f);
            Traverse.Create(dummyInfo).Field("m_Length").SetValue(0.4444448f);
            sLoopInfo = (AnimatorStateInfo)dummyInfo;

            GameAPI.RegisterExtraBehaviour<SensibleHController>(GUID);
        }
    }
}


