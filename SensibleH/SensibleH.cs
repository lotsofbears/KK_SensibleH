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
using KKAPI.Utilities;

namespace KK_SensibleH
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
#if KK
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
#endif
    [BepInDependency(KoikatuAPI.GUID)]
    [BepInDependency(KK_VR.VRPlugin.GUID)]
    [BepInDependency(KK_BetterSquirt.BetterSquirt.GUID)] // F it, normal version stays in hard dependencies, no clue how to pass reflected enum type to the delegate.

    public class SensibleH : BaseUnityPlugin
    {
        public const string GUID = "kk.sensible.h";
        public const string Name = "KK_SensibleH";
        public const string Version = "1.2";
        public new static PluginInfo Info { get; private set; }
        public new static ManualLogSource Logger;
        public static ConfigEntry<PluginState> Enabled { get; set; }
        public static ConfigEntry<bool> OnlyInVR { get; set; }
        public static ConfigEntry<AutoModeKind> AutoMode { get; set; }
        public static ConfigEntry<AutoPoseType> AutoPickPose{ get; set; }
        public static ConfigEntry<bool> AutoRestartAction { get; set; }
        public static ConfigEntry<EdgeType> Edge { get; set; }
        public static ConfigEntry<bool> MomiMomi { get; set; }
        public static ConfigEntry<bool> EyeNeckControl { get; set; }
#if DEBUG
        public static ConfigEntry<bool> HoldPubicHair { get; set; }
        public static ConfigEntry<KeyboardShortcut> Cfg_TestKey { get; set; }
#endif
        public static ConfigEntry<bool> DisablePeskySounds { get; set; }
#if KKS
        public static ConfigEntry<bool> ProlongObi { get; set; }
#endif
        public static ConfigEntry<float> ActionFrequency { get; set; }
        public static ConfigEntry<bool> AutoADV { get; set; }
        public static ConfigEntry<float> EdgeFrequency { get; set; }
        public static ConfigEntry<float> NeckLimit { get; set; }
        public static ConfigEntry<int> GaugeSpeed { get; set; }
        public static ConfigEntry<FrenchType> FrenchKiss { get; set; }
        public static ConfigEntry<int> KissEyesLimit { get; set; }
        public static ConfigEntry<bool> AddReverb { get; set; }
        public static bool MoveNeckGlobal;
        public static int[] EyeNeckPtn = { -1, -1, -1 };

        internal static List<HeadManipulator> headManipulators = [];
        internal static HandCtrl handCtrl;
        internal static HandCtrl handCtrl1;
        internal static HMotionEyeNeckFemale _eyeneckFemale;
        internal static HMotionEyeNeckFemale _eyeneckFemale1;
        internal static HFlag hFlag;
        internal static HFlag.EMode mode;
        internal static List<ChaControl> lstFemale;
        internal static ChaControl male;
        internal static HVoiceCtrl _hVoiceCtrl;
        internal static HSprite _sprite;
        internal static GameObject MalePoI;
        internal static GameObject[] FemalePoI;

        internal static float BiasF { get; set; }
        internal static float BiasM { get; set; }
        internal static float gaugeMultiplier { get; set; }
        internal static bool OLoop { get; set; }
        internal static bool MoMiActive { get; set; }
        internal static bool FirstTouch { get; set; }
        internal static int CurrentMain { get; set; }
        internal static int MaleOrgCount { get; set; }
        internal static bool SuppressVoice { get; set; }
        internal static bool OverrideSquirt { get; set; }
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
            PromptStart,
            PromptStartFinish,
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
        public enum PluginState
        {
            Disabled,
            OnlyInVr,
            Always,
        }


        public void Awake()
        {
            Logger = base.Logger;


            Enabled = Config.Bind(
                section: "",
                key: "Enable",
                defaultValue: PluginState.OnlyInVr,
                new ConfigDescription(
                    "The changes take place after the scene change")
                );


            AutoMode = Config.Bind(
                section: "AutoMode",
                key: "State",
                defaultValue: AutoModeKind.PromptStartFinish,
                new ConfigDescription(
                "Minimizes the need for inputs from the player during H\n" +
                //"Disabled - Waits for setting to change any moment.\n" +
                "PromptStart - to start click on any interactable element of the lower half of the screen(except leaving H) or touch/kiss\n" +
                "PromptStartFinish - same as above to continue after climax\n" +
                "Automatic - will start/finish action, change position and restart action automatically",
                //"Warning. All proactive functions have mild disrespect for persistent user input, bad things (that will be solved only by the reboot of the game) may happen " +
                //"for those who seek two pilots in one seat." 
                null,
                new ConfigurationManagerAttributes { Order = 10 }
                ));


            AutoPickPose = Config.Bind(
                section: "AutoMode",
                key: "Position change",
                defaultValue: AutoPoseType.AllPositions,
                new ConfigDescription(
                "Change position after climax",
                //"FemdomOnly - Choses only position where the girl is dominant. By default it's only one (game's default) cowgirl position. With modified AnimationLoader manifest comes much more.\n" +
                //"AllPositions - random non caress animation.\n" +
                //"If both AutoPositionChange and AutoRestart are enabled, position change takes a bit of precedence.",
                null,
                new ConfigurationManagerAttributes { Order = 9 }
                ));


            AutoRestartAction = Config.Bind(
                section: "AutoMode",
                key: "Restart",
                defaultValue: true,
                new ConfigDescription(
                "Restart after climax",
                null,
                new ConfigurationManagerAttributes { Order = 7 }
                ));


            Edge = Config.Bind(
                section: "AutoMode",
                key: "Edge",
                defaultValue: EdgeType.Outside,
                new ConfigDescription(
                "Pull out/stop for a moment for whatever reason",
                null,
                new ConfigurationManagerAttributes { Order = 6 }
                ));


            ActionFrequency = Config.Bind(
                section: "AutoMode",
                key: "Change frequency",
                defaultValue: 1f,
                new ConfigDescription("The lesser the value the smaller the pause between actions",
                new AcceptableValueRange<float>(0.1f, 2f),
                new ConfigurationManagerAttributes { Order = 8 }
                )); 


            EdgeFrequency = Config.Bind(
                section: "AutoMode",
                key: "Edge frequency",
                defaultValue: 1f,
                new ConfigDescription("The lesser the value the smaller the pause between actions",
                new AcceptableValueRange<float>(0.1f, 2f),
                new ConfigurationManagerAttributes { Order = 5 }
                ));


            NeckLimit = Config.Bind(
                section: "Tweaks",
                key: "Neck limit",
                defaultValue: 1f,
                new ConfigDescription("Adjust the limits of neck movements, 1.0 being the default value.\n" +
                "Changes take place on scene reload or new position.",
                new AcceptableValueRange<float>(0.5f, 1.5f))
                );


            EyeNeckControl = Config.Bind(
                section: "Tweaks",
                key: "Eye/neck control",
                defaultValue: true,
                "Allow plugin to introduce alternative control of eyes and neck."
                );

#if DEBUG
            HoldPubicHair = Config.Bind(
                section: "Tweaks",
                key: "HoldPubicHair",
                defaultValue: true,
                "Hold the scale of pubic hair accessory attached to the crouch."
                );
#endif

            AutoADV = Config.Bind(
                section: "Tweaks",
                key: "Auto ADV",
                defaultValue: true,
                new ConfigDescription(
                    "Enables auto mode in any text scenario by default if there is no heroine present.",
                    null,
                    new ConfigurationManagerAttributes { Order = -9 }
                ));


            DisablePeskySounds = Config.Bind(
                section: "Tweaks",
                key: "Disable button click SFX",
                defaultValue: true,
                new ConfigDescription(
                "",
                null,
                new ConfigurationManagerAttributes { Order = -10 }
                ));


            GaugeSpeed = Config.Bind(
                section: "Tweaks",
                key: "Excitement slowdown",
                defaultValue: 4,
                new ConfigDescription(
                    "Decreases the speed of excitement gauge increase by value times.",
                    new AcceptableValueRange<int>(1, 10))
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
                section: "Kiss",
                key: "Tongue",
                defaultValue: Kiss.FrenchType.Auto,
                new ConfigDescription(
                "Stick out tongue during kiss.",
                null,
                new ConfigurationManagerAttributes { Order = 10 }
                ));


            KissEyesLimit = Config.Bind(
                section: "Kiss",
                key: "Eyes",
                defaultValue: 50,
                new ConfigDescription("Maximum openness of the eyes during kissing.\n" +
                "Set to 0 to keep eyes closed during kiss",
                new AcceptableValueRange<int>(0, 100),
                new ConfigurationManagerAttributes { Order = 9, ShowRangeAsPercent = false }
                ));


            AddReverb = Config.Bind(
                section: "Tweaks",
                key: "Reverb",
                defaultValue: true,
                new ConfigDescription("Add reverb SFX to some maps",
                null,
                new ConfigurationManagerAttributes { Order = -20 }
                ));
#if DEBUG
            Cfg_TestKey = Config.Bind(
                section: "SensibleH",
                key: "TestKey1",
                defaultValue: KeyboardShortcut.Empty,
                "Key to manually trigger test"
                );
#endif


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


