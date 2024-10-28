using KKAPI;
using KKAPI.MainGame;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using static KK_SensibleH.SensibleH;
using System.Collections;
using System.Linq;
using Illusion.Extensions;
using ActionGame;
using System;
using Manager;
using KK_SensibleH.Patches.StaticPatches;
using KK_SensibleH.AutoMode;
using VRGIN.Helpers;
using KK_SensibleH.Caress;
using ADV;
using RootMotion.FinalIK;
using Studio;
using VRGIN.Core;
using static Illusion.Utils;
using KK.RootMotion.FinalIK;
namespace KK_SensibleH
{
    /// <summary>
    /// Recently broken:
    /// Kiss SFX reappeared on disengage phase of the kiss.
    /// </summary>
    public class SensibleHController : GameCustomFunctionController
    {
        public static SensibleHController Instance;

        private MoMiController _moMiController;
        private MaleController _maleController;
        private LoopController _loopController;
        private readonly List<Harmony> _persistentPatches = new List<Harmony>();
        //private AnimatorStateInfo getCurrentAnimatorStateInfo;
        private readonly int[] _clothes = { 1, 3, 5};
        private bool _hEnd;
        internal static bool _vr;
#if KK
        private Transform[] _ref = new Transform[22];
        enum Refs
        {
            // 22 entry.
            root,
            pelvis,
            spine,
            chest,
            neck,
            head,

            leftShoulder,
            leftUpperArm,
            leftForearm,
            leftHand,
            
            rightShoulder,
            rightUpperArm,
            rightForearm,
            rightHand,

            leftThigh,
            leftCalf,
            leftFoot,
            leftToes,

            rightThigh,
            rightCalf,
            rightFoot,
            rightToes
        }
        // cf_j_spine03 brings rather weird behaviour.
        //private Transform[] GetFullSpine(RootMotion.FinalIK.FullBodyBipedIK fbbik)
        //{
        //    var result = new Transform[4];
        //    result[0] = fbbik.references.spine[0];
        //    result[1] = fbbik.references.spine[1];
        //    result[2] = result[1].Find("cf_j_spine03");
        //    result[3] = fbbik.references.spine[2];
        //    return result;
        //}
        private Transform _spine03;
        private Transform _camera;
        private LookAtController _lookAtController;
        internal void SetupLookAtIK(ChaControl chara)
        {
            var fbbik = chara.objAnim.GetComponent<RootMotion.FinalIK.FullBodyBipedIK>();
            if (fbbik == null) return;
            var lookAt = chara.objAnim.AddComponent<KK.RootMotion.FinalIK.LookAtIK>();
            lookAt.solver.SetChain(fbbik.references.spine, fbbik.references.head, null, fbbik.references.root);
            lookAt.solver.bodyWeight = 0.7f;
            lookAt.solver.headWeight = 0.7f;
            _lookAtController = chara.objAnim.AddComponent<LookAtController>();
            _lookAtController.ik = lookAt;
            _lookAtController.weightSmoothTime = 1f;
            _lookAtController.targetSwitchSmoothTime = 1f;
            _lookAtController.maxRadiansDelta = 0.25f;
            _lookAtController.maxMagnitudeDelta = 0.25f;
            _lookAtController.slerpSpeed = 1f;
            _lookAtController.maxRootAngle = 180f;
            _camera = hFlag.ctrlCamera.transform;
            //lookController.target = VR.Camera.Head;
            _spine03 = fbbik.references.spine[1].Find("cf_j_spine03");
        }
        private void FollowTarget()
        {
            if (_lookAtController.target == null)
            {
                if (Vector3.Angle(_camera.position - _spine03.position, _spine03.forward) < 45f)
                {
                    _lookAtController.target = _camera;
                }
            }
            else
            {
                if (Vector3.Angle(_camera.position - _spine03.position, _spine03.forward) > 90f)
                {
                    _lookAtController.target = null;
                }
            }
        }


#endif
        private void Update()   
        {
            if (Input.GetKeyDown(Cfg_TestKey.Value.MainKey) && Cfg_TestKey.Value.Modifiers.All(x => Input.GetKey(x)))
            {
#if KK
                SetupLookAtIK(_chaControl[0]);
                //_chaControlM.objTop.SetActive(true);
                //_chaControlM.visibleAll = true;
                //_chaControlM.fileStatus.visibleHeadAlways = false;
                //KK_VR.Features.VRIKHelper.TestRun(_chaControlM);
#endif
            }
            else if (Input.GetKeyDown(Cfg_TestKey2.Value.MainKey) && Cfg_TestKey2.Value.Modifiers.All(x => Input.GetKey(x)))
            {
#if KK
#endif
            }
            else if (Input.GetKeyDown(Cfg_TestKey3.Value.MainKey) && Cfg_TestKey3.Value.Modifiers.All(x => Input.GetKey(x)))
            {
            }
        }
        private void Start()
        {
            Instance = this;
            _vr = SteamVRDetector.IsRunning;
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchClickAction)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchDragAction)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchEyeNeck)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchGame)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchH)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchHandCtrl)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchLoop)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(TestGame)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(TestH)));
#if KKS
                if (SensibleH.ProlongObi.Value)
                {
                    _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchObi)));
                }
#endif
            if (_vr)
            {
                _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchHandCtrlVR)));
            }

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;

        }
        // Did i port it in KK_VR?
        // private static int[] _reverbMaps = new int[] { 14, 15, 16, 18, 37, 45, 51, 52, 7501, 7550 };
        private string[] _reverbMaps = new string[]
        {
            "Pool",
            "ShawerRoom",
            "1FToilet",
            "2FToilet",
            "3FToilet",
            "ToiletMale"
        };

        private void OnActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            if (_reverbMaps.Contains(to.name))
            {
                var map = GameObject.Find("Map");
                if (map != null && map.GetComponent<AudioReverbZone>() == null)
                {
                    map.AddComponent<AudioReverbZone>();
                    //SensibleH.Logger.LogDebug($"Added:{typeof(AudioReverbZone)}:to:{map.name}");
                }
            }
        }
        protected override void OnStartH(MonoBehaviour proc, HFlag flag, bool vr)
        {
            SensibleH.Logger.LogDebug($"OnStartH");
            StopAllCoroutines();
            _hEnd = false;
            var traverse = Traverse.Create(proc);
            _handCtrl = traverse.Field("hand").GetValue<HandCtrl>();
            _handCtrl1 = traverse.Field("hand1").GetValue<HandCtrl>();
            _hVoiceCtrl = traverse.Field("voice").GetValue<HVoiceCtrl>();

            _chaControl = traverse.Field("lstFemale").GetValue<List<ChaControl>>();
            _chaControlM = traverse.Field("male").GetValue<ChaControl>();
            
            hFlag = flag;
            if (LstHeroine == null)
            {
                LstHeroine = new Dictionary<string, int>();
            }
            _eyeneckFemale = traverse.Field("eyeneckFemale").GetValue<HMotionEyeNeckFemale>();
            _eyeneckFemale1 = traverse.Field("eyeneckFemale1").GetValue<HMotionEyeNeckFemale>();
            var charaCount = _chaControl.Count;
            _girlControllers = new List<GirlController>(charaCount);
            FemalePoI = new GameObject[charaCount];

            _moMiController = this.gameObject.AddComponent<MoMiController>();
            _maleController = this.gameObject.AddComponent<MaleController>();
            _loopController = this.gameObject.AddComponent<LoopController>();
            _loopController.Initialize(proc, this);
            for (int i = 0; i < charaCount; i++)
            {
                //if (!_redressTargets.ContainsKey(hFlag.lstHeroine[i]))
                //{
                //    _redressTargets.Add(hFlag.lstHeroine[i], new List<byte>());
                //}
                var heroine = this.gameObject.AddComponent<GirlController>();
                heroine.Initialize(i, GetFamiliarity(i));
                _girlControllers.Add(heroine);
            }
            DressDudeForAction();

            // Gameplay Enhancements by ManlyMarco attempts to change this too, but the value changed is irrelevant in practice.
            if (hFlag.isInsertOK[0])
            {
                hFlag.isInsertOK[0] = Random.value < 0.75f;
            }
            if (hFlag.isAnalInsertOK)
            {
                hFlag.isAnalInsertOK = Random.value < 0.75f;
            }

            var pipi = _chaControlM.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cm_J_dan_top/cm_J_dan100_00");
            TestH.size = (pipi.localScale.x + pipi.localScale.y) * 0.5f;

            UpdateSettings();
            //_repositionLight = true;
            StartCoroutine(OnceInAwhile());
        }

        private IEnumerator DoLater()
        {
            yield return new WaitForSeconds(1f);
        }
        private void DressDudeForAction()
        {
            // Not-only-socks edition.
            var states = _chaControlM.fileStatus.clothesState;
            for (int i = 0; i < states.Length; i++)
            {
                if (_clothes.Contains(i))
                    states[i] = 1;
                else
                    states[i] = 0;
            }
            _chaControlM.UpdateClothesStateAll();
        }

        private IEnumerator OnceInAwhile()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (_hEnd || hFlag == null)
                {
                    SensibleH.Logger.LogDebug($"HEnd");
                    if (!_hEnd)
                    {
                        EndItAll();
                        if (!SceneApi.GetLoadSceneName().Equals("Action"))
                        {
                            yield break;
                        }
                    }
                    else if (!SceneApi.GetAddSceneName().StartsWith("H", StringComparison.Ordinal))// && !SceneApi.GetIsNowLoadingFade())
                    {
                        ReDressAfter();
                        yield break;
                    }

                    yield return new WaitForSeconds(1f);
                    continue;
                }
#if KK
                if (!Scene.Instance.AddSceneName.Equals("HProc")) continue;
#else
                if (SceneApi.GetIsOverlap()) continue;
#endif

                _loopController.Proc();
                //_moMiController.Proc();
                foreach (var girl in _girlControllers)
                {
                    girl.Proc();
                }
                if (MoveNeckGlobal && (!SensibleH.EyeNeckControl.Value || (EyeNeckPtn[0] == -1 && EyeNeckPtn[1] == -1)))
                {
                    SensibleH.Logger.LogDebug($"MoveNeckGlobal[Stop]");
                    MoveNeckGlobal = false;
                }
                //if (_chaControlM != null && _chaControlM.visibleAll)
                //    maleController.LookLessDead();
                //SensibleH.Logger.LogDebug($"OnceInAWhile[{_scene.AddSceneName}");
                //    $"poi[{FemalePoI[0]}] [{moveNeckGlobal}]");
                if (!FirstTouch)
                {
                    FirstTouch = !_handCtrl.IsItemTouch();
                }

                //if (_repositionLight)
                //{
                //    RepositionDirLight();
                //}
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Returns (0.25 - 1.0) value based on, well familiarity.
        /// </summary>
        internal float GetFamiliarity(int main)
        {
            var heroine = hFlag.lstHeroine[main];
            var hExp = 0.55f + ((int)heroine.HExperience * 0.15f);
            if (!hFlag.isFreeH)
            {
                if (hFlag.mode != HFlag.EMode.lesbian && hFlag.mode != HFlag.EMode.masturbation)
                {
#if KK
                    hExp *= 0.5f + heroine.intimacy * 0.005f;
#else
                    hExp *= 0.5f + Mathf.Clamp(heroine.hCount, 0f, 10f) * 0.05f;//   (heroine.lewdness * 0.005f);
#endif
                }
                else
                {
                    hExp *= 0.75f + (heroine.lewdness * 0.0025f);
                }
            }
            else
            {
                hExp *= 0.75f;
            }
            SensibleH.Logger.LogDebug($"Familiarity:{hExp}");
            return hExp;
        }
        public void OnVoiceProc(int main)
        {
            if (hFlag != null)
            {
                if (SuppressVoice)
                {
                    _girlControllers[main]._lastVoice = hFlag.voice.playVoices[main];
                    hFlag.voice.playVoices[main] = -1;
                }
                else
                {
                    _girlControllers[main].OnVoiceProc();
                }
            }
        }
        public void OnPositionChange(HSceneProc.AnimationListInfo nextAnimInfo)
        {
            SensibleH.Logger.LogDebug($"NewPosition[{nextAnimInfo.mode}]");
            if (hFlag != null)
            {
                CurrentMain = hFlag.nowAnimationInfo.nameAnimation.Contains("Alt") ? 1 : 0;

                _loopController.OnPositionChange(nextAnimInfo);
                _moMiController.OnPositionChange(nextAnimInfo);
                _sprite.ForceCloseAllMenu();

                foreach (var girl in _girlControllers)
                {
                    girl.OnPositionChange();
                }
                foreach (var obj in _sprite.menuActionSub.lstObj)
                {
                    obj.SetActive(false);
                }
                UpdateSettings();
                SetTouchAvailability();
                //_repositionLight = true;
            }

        }
        internal static void UpdateSettings()
        {
            hFlag.rateClickGauge = 2f / (float)GaugeSpeed.Value;
        }
        public void DoFirstTouchProc()
        {
            SensibleH.Logger.LogDebug($"ExtraVoices:FirstTouch");
            List<int> voiceId = new List<int>();
            foreach (var item in _handCtrl.useItems)
            {
                if (item != null)
                {
                    voiceId.Add(dragVoices[item.idObj, item.kindTouch - HandCtrl.AibuColliderKind.muneL]);
                }
            }
            if (voiceId.Count != 0)
            {
                // Click voices have IDs of dragID - 1.
                hFlag.voice.playVoices[0] = voiceId[Random.Range(0, voiceId.Count)] - (Random.value > 0.5f ? 1 : 0);

            }
        }
        public void OnTouch(int item = -1)
        {
            if (hFlag != null)
            {
                SensibleH.Logger.LogDebug($"ExtraTriggers:Touch");
                _girlControllers[0]._neckController.LookAtPoI(item);
            }
        }

        public void SetTouchAvailability()
        {
            foreach (var anim in _handCtrl.dicMES.Values)
            {
                for (var i = 0; i < anim.isTouchAreas.Length; i++)
                {
                    anim.isTouchAreas[i] = true;
                }
            }
        }

        private readonly int[,] dragVoices = new int[,]
        {
            // hand (0)
            { 112, 112, 114, 116, 118, 118 },

            // finger (1)
            { 124, 124, 120, 122, -1, -1 },
            
            // tongue (2)
            { 132, 132, 126, 128, 130, 130 },
            
            // massager (3)
            { 138, 138, 134, -1, 136, 136 },
            
            // vibrator (4)
            { -1, -1, 140, -1, -1, -1 },

            // dildo (5)
            { -1, -1, 147, -1, -1, -1 },

            // rotor (6)
            { 151, 151, 149, -1, -1, -1 }
        };
#if KK
        //protected override void OnPeriodChange(Cycle.Type period)
        //{
        //    // Implemented by default in KKS.
        //    foreach (var heroine in Game.Instance.HeroineList)
        //    {
        //        //for (int i = 0; i < heroine.talkTemper.Count(); i++)
        //        //{
        //        //    // 2 - denial
        //        //    //heroine.talkTemper[i] = (byte)Random.Range(0, 3);
        //        //    //heroine.talkTemper[i] = (byte)2;
        //        //}
        //        ShuffleTemper(heroine);
        //    }
        //}
        public static void ShuffleTemper(SaveData.Heroine heroine)
        {
            var temper = heroine.m_TalkTemper;
            var bias = 1f - Mathf.Clamp01(0.3f - heroine.favor * 0.001f - heroine.intimacy * 0.001f - (heroine.isGirlfriend ? 0.1f : 0f));
            //SensibleH.Logger.LogDebug($"ShuffleTemper:{heroine.Name}:{bias}");
            var part = bias * 0.5f;
            for (int i = 0; i < temper.Length; i++)
            {
                temper[i] = GetBiasedByte(bias, part);
            }
        }
        private static byte GetBiasedByte(float bias, float part)
        {
            var value = Random.value;
            if (value > bias) return 2;
            if (value < part) return 1;
            return 0;
        }
#endif
        protected override void OnDayChange(Cycle.Week day)
        {
#if KK
            foreach (var heroine in Game.Instance.HeroineList)
            {
                ShuffleTemper(heroine);
            }
#endif
            LstHeroine = null;
            MaleOrgCount = 0;
        }
        protected override void OnEndH(MonoBehaviour _proc, HFlag _hFlag, bool _vr)
        {
            SensibleH.Logger.LogDebug($"OnEndH");
            EndItAll();
        }
        private readonly int[] _auxClothesSlots = { 2, 3, 5, 6 };
        //private List<SaveData.Heroine> _heroineList = new List<SaveData.Heroine>();

        //internal void RepositionDirLight()
        //{
        //    if (hFlag == null)
        //        return;
        //    _repositionLight = false;
        //    var chara = _chaControl[0];
        //    var hScene = chara.transform.parent;
        //    var dirLight = hScene.Find("CameraBase/Camera/Directional Light");
        //    if (dirLight == null)
        //    {
        //        dirLight = hScene.Find("Directional Light");
        //        if (dirLight == null)
        //        {
        //            return;
        //        }
        //    }
        //    // We find rotation between vector from the center of the scene (0,0,0), and base of the chara.
        //    // Then we create rotation towards it from the chara for random degrees, and elevate it a bit.
        //    // And place our camera there. Consistent, doesn't defy logic too often, and much better then in vr then directional light.
        //    // TODO port to KK_VR.

        //    var lowHeight = (chara.objHeadBone.transform.position.y - chara.transform.position.y) < 0.5f;
        //    var yDeviation = Random.Range(30f, 60f);
        //    var xDeviation = Random.Range(15f, lowHeight ? 60f : 30f);
        //    var lookRot = Quaternion.LookRotation(new Vector3(0f, chara.transform.position.y, 0f) - chara.transform.position);
        //    dirLight.transform.SetParent(hScene, worldPositionStays: false); // 
        //    dirLight.position = chara.objHeadBone.transform.position + (Quaternion.RotateTowards(chara.transform.rotation, lookRot, yDeviation) * Quaternion.Euler(-xDeviation, 0f, 0f) * Vector3.forward);
        //    dirLight.rotation = Quaternion.LookRotation((lowHeight ? chara.objBody : chara.objHeadBone).transform.position - dirLight.position);
        //    SensibleH.Logger.LogDebug($"{chara.objHeadBone.transform.position}");
        //}

        private Dictionary<string, List<byte>> _redressTargets = new Dictionary<string, List<byte>>();
        private void ReDress()
        {

#if KK
            SensibleH.Logger.LogDebug($"ReDress:{_chaControl.Count}");
            foreach (var chara in _chaControl)
            {
                //var clone = Game.Instance.actScene.GetComponentsInChildren<ChaControl>()
                //    .Where(c => c.fileParam.fullname.Equals(chara.fileParam.fullname) && c.fileParam.personality == chara.fileParam.personality)
                //    .FirstOrDefault();
                //var cloneSaveData = Game.Instance.HeroineList
                //    .Where(h => h.chaCtrl == chara)
                //    .FirstOrDefault();
                _redressTargets.Add(chara.fileParam.fullname, new List<byte>());
                var target = _redressTargets[chara.fileParam.fullname];
                for (var i = 0; i < chara.fileStatus.clothesState.Length; i++)
                {
                    if (_auxClothesSlots.Contains(i) && chara.fileStatus.clothesState[i] > 1)
                    {
                        target.Add(3);
                        //cloneSaveData.charFile.status.clothesState[j] = 3;
                        //clone.fileStatus.clothesState[j] = 3;
                    }
                    else
                    {
                        target.Add(0);
                        //cloneSaveData.charFile.status.clothesState[j] = 0;
                        //clone.fileStatus.clothesState[j] = 0;
                    }
                }
                //clone.chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)chara.fileStatus.coordinateType);
                target.Add((byte)chara.fileStatus.coordinateType);
                target.Add((byte)chara.fileParam.personality);

                ////var heroine = _heroineList[i];
                //var chara = _chaControl[i].chaFile;
                //var heroine = _redressTargets.ElementAt(i);
                //for (var j = 0; j < chara.status.clothesState.Length; j++)
                //{
                //    if (_auxClothesSlots.Contains(j) && chara.status.clothesState[j] > 1)
                //    {
                //        heroine.Value.Add(3);
                //    }
                //    else
                //    {
                //        heroine.Value.Add(0);
                //    }
                //}
                //heroine.Key.coordinates[0] = chara.status.coordinateType;
                //heroine.Key.isDresses[0] = false;
            }

#endif
        }
        private void ReDressAfter()
        {
            // Proper redressing has to be done after H if we want changed outfit to stay put (atleast for a while).
            // Not in KKS. Also we prompt a girl to put on a different outfit, even if this period it is already done.
            //
            // There are a lot of null checks in console and sometimes failed to load outfits around the school, but pretty sure, I contribute none to that,
            // As it keeps on happening even without any of my edits/plugins, and the plugin in question is quite important.. 
            SensibleH.Logger.LogDebug($"ReDressAfter");
#if KK
            var _gameMgr = Game.Instance;
            foreach (var target in _redressTargets)
            {
                var saveData = Game.Instance.HeroineList
                    .Where(h => h.Name.Equals(target.Key) && h.personality == target.Value[target.Value.Count - 1])
                    .FirstOrDefault();

                var chara = saveData.chaCtrl;
                var state = chara.fileStatus.clothesState;
                for (var i = 0; i < state.Length; i++)
                {
                    state[i] = target.Value[i];
                    saveData.charFile.status.clothesState[i] = target.Value[i];

                    //clothesState[i] = 0;
                }
                // KKS H Clone has messed up coord index? Coord plugin interference?

                chara.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)target.Value[target.Value.Count - 2]);

                saveData.coordinates[0] = target.Value[target.Value.Count - 2];
                saveData.isDresses[0] = false;
                _gameMgr.actScene.actCtrl.SetDesire(0, saveData, 200);
        
            }
            _redressTargets.Clear();

#endif
        }
        public void EndItAll()
        {
            SensibleH.Logger.LogDebug($"EndItAll");
            if (SceneApi.GetLoadSceneName().Equals("Action"))
            {
                // We are in the main game.
                ReDress();
            }
            else
            {
                StopAllCoroutines();
            }
            _hEnd = true;
            FemalePoI = null;
            MalePoI = null;

            Destroy(_moMiController);
            Destroy(_loopController);
            foreach (var controller in _girlControllers)
            {
                Destroy(controller);
            }

            OLoop = false;
            MoveNeckGlobal = false;
        }
    }
}