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
using VRGIN.Core;

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
        private List<Harmony> _persistentPatches = new List<Harmony>();
        //private readonly int[] voiceButton = { 3, 5, 6, 8 };
        //private AnimatorStateInfo getCurrentAnimatorStateInfo;
        private readonly int[] _clothes = { 1, 3, 5};
        private bool _hEnd;
        internal bool _vr;
        private void Update()   
        {
            if (Input.GetKeyDown(Cfg_TestKey.Value.MainKey) && Cfg_TestKey.Value.Modifiers.All(x => Input.GetKey(x)))
            {
#if KK
                SensibleH.Logger.LogDebug($"{VR.Camera.Head.position.y - Game.Instance.actScene.Player.chaCtrl.transform.position.y}");
#endif
                //var skinnedMeshes = _chaControl[0].GetComponentsInChildren<SkinnedMeshRenderer>();
                //foreach(var skinnedMesh in skinnedMeshes)
                //{
                //    SensibleH.Logger.LogDebug($"{skinnedMesh.name}");
                //    if (skinnedMesh.name.Equals("cf_O_face", StringComparison.Ordinal))
                //    {
                //        for (int i = 0; i < skinnedMesh.sharedMesh.blendShapeCount; i++)
                //        {
                //            SensibleH.Logger.LogDebug($"{skinnedMesh.sharedMesh.GetBlendShapeName(i)} " +
                //                $"{skinnedMesh.GetBlendShapeWeight(i)}");
                //        }
                //    }
                //}
            }
            else if (Input.GetKeyDown(Cfg_TestKey2.Value.MainKey) && Cfg_TestKey2.Value.Modifiers.All(x => Input.GetKey(x)))
            {
                //var cameras = FindObjectsOfType<Camera>();
                //foreach (var cam in cameras)
                //{
                //    SensibleH.Logger.LogDebug($"{cam.name} DepthMode[{cam.depthTextureMode}] Depth[{cam.depth}] [{cam.cullingMask}]");
                //    SensibleH.Logger.LogDebug($"{string.Join(", ", UnityHelper.GetLayerNames(cam.cullingMask))}");
                //}
                //var vrginCam = FindObjectsOfType<Camera>().Where(c => c.name.Contains("VRGIN_Camera"));
                //var currentObiRend = FindObjectOfType<ObiFluidRenderer>();
                //VR.Camera.GetOrAddComponent<ObiFluidRenderer>();
                //var newObiRend = VR.Camera.GetComponent<ObiFluidRenderer>();
                //SensibleH.Logger.LogDebug($"VrginCamera[{VR.Camera.name}]");

                //var hScene = FindObjectOfType<HSceneProc>();
                //var fluidRend = FindObjectOfType<ObiFluidRenderer>();

                //SensibleH.Logger.LogDebug($"{hScene.gameObject.name}");
                //SensibleH.Logger.LogDebug($"{fluidRend.gameObject.name}");
                //if (_forwardHelper)
                //{
                //    _forward += 0.01f;
                //    if (_forward > 0.1f)
                //        _forwardHelper = false;
                //    _testForm.transform.localPosition = new Vector3(_testForm.transform.localPosition.x, _testForm.transform.localPosition.y, _forward);
                //}
                //else
                //{
                //    _forward -= 0.01f;
                //    if (_forward < -0.1f)
                //        _forwardHelper = true;
                //    _testForm.transform.localPosition = new Vector3(_testForm.transform.localPosition.x, _testForm.transform.localPosition.y, _forward);
                //}
                //SensibleH.Logger.LogDebug($"Hotkey[2] _forward = {_forward}");
            }
            else if (Input.GetKeyDown(Cfg_TestKey3.Value.MainKey) && Cfg_TestKey3.Value.Modifiers.All(x => Input.GetKey(x)))
            {
                //if (_upHelper)
                //{
                //    _up += 0.01f;
                //    if (_up > 0.1f)
                //        _upHelper = false;
                //    _testForm.transform.localPosition = new Vector3(_testForm.transform.localPosition.x, _up, _testForm.transform.localPosition.z);
                //}
                //else
                //{
                //    _up -= 0.01f;
                //    if (_up < -0.1f)
                //        _upHelper = true;
                //    _testForm.transform.localPosition = new Vector3(_testForm.transform.localPosition.x, _up, _testForm.transform.localPosition.z);
                //}
                //SensibleH.Logger.LogDebug($"Hotkey[3] _up = {_up}");
            }
        }
        //private float _forward;
        //private float _up;
        //private bool _forwardHelper;
        //private bool _upHelper;
        //private GameObject _primitiveCube;
        //private void Spawn()
        //{
        //    _primitiveCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    _primitiveCube.transform.SetParent(_chaControl[0].objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceLow_tz/a_n_mouth"), false);
        //    _primitiveCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        //    _primitiveCube.GetComponent<Collider>().enabled = false;
        //    _primitiveCube.GetComponent<Renderer>().material.color = new Color(1, 0, 1, 1);
        //    _primitiveCube.GetComponent<Renderer>().enabled = true;
        //}

        //private void Test()
        //{
        //    // source of sound attachment
        //    Transform transform = _chaControls[0].objBody.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cf_j_kokan"));
        //    Transform soundReference = transform.transform.parent;
        //    GameObject asset = CommonLib.LoadAsset<GameObject>($"studio/{ASSETBUNDLE}", ASSETNAME, clone: true);
        //    Utils.Sound.Setting setting = new Utils.Sound.Setting
        //    {
        //        type = Manager.Sound.Type.GameSE3D,
        //        assetBundleName = softSE ? @"sound/data/se/h/00/00_00.unity3d" : @"sound/data/se/h/12/12_00.unity3d",
        //        assetName = "hse_siofuki",
        //    };
        //    Transform soundsource = Utils.Sound.Play(setting).transform;
        //    if (soundsource != null)
        //    {
        //        soundsource.transform.SetParent(soundReference, false);
        //    }
        //}

        // _handCtrl.IsItemtouch - presence of attached hand regardless of action
        /*_hFlag.speedupclickaibu make girl sway from intensity
         * hAibu StartDislikes to cut it out 
         */
        //public void Test9()
        //{
        //    voiceController.PlayVoice(tempCounter);
        //    SensibleH.Logger.LogDebug($"Played voice with id call {tempCounter}");
        //    tempCounter += 1;
        //}
        private void OnDestroy()
        {
            //foreach (var patch in _persistentPatches)
            //    patch?.UnpatchSelf();
            //if (_vr)
            //{
            //    ResourceUnloadOptimizations.DisableUnload.Value = false;
            //}
        }
        private void Start()
        {
            Instance = this;
            _vr = SteamVRDetector.IsRunning;
            //SensibleH.Logger.LogDebug($"PersistentPatches");
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchEyeNeck)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchH)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchLoop)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchGame)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(TestH)));
            _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(TestGame)));
#if KKS
                if (SensibleH.ProlongObi.Value)
                {
                    _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchObi)));
                }
#endif
            if (_vr)
            {
                //SensibleH.Logger.LogDebug($"PersistentPatches[VR]");
                _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchHandCtrlVR)));
            }

        }
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            SensibleH.Logger.LogDebug($"OnStartH");
            StopAllCoroutines();
            //if (_vr)
            //{
            //    // This thing is evil.

            //    // The easies way to disable stutters in H when you have 20gb of free RAM.
            //    // Does it look at VRAM too? Couldn't find it, and not much can be done about it in VR anyway.
            //    // With this and the patch, there is no more stutters on kiss.
            //    // And people who actually need this in VR H scene.. I HIGLY doubt there are any.
            //    // Once H ends/OnDestroy we re-enable it.
            //    ResourceUnloadOptimizations.DisableUnload.Value = true;
            //}
            _hEnd = false;
            _handCtrl = Traverse.Create(proc).Field("hand").GetValue<HandCtrl>();
            _handCtrl1 = Traverse.Create(proc).Field("hand1").GetValue<HandCtrl>();
            _hVoiceCtrl = Traverse.Create(proc).Field("voice").GetValue<HVoiceCtrl>();

            // TODO This thingy isn't stock, add it.
            //_colliderPlane = CommonLib.LoadAsset<GameObject>($"studio/pine_effect.unity3d", "ColliderPlane", clone: true);
            //_colliderPlane.GetComponent<Renderer>().enabled = false;
            //_colliderPlane.transform.position = new Vector3(0f, 0.1f, 0f);;

            _chaControl = Traverse.Create(proc).Field("lstFemale").GetValue<List<ChaControl>>();
            _chaControlM = Traverse.Create(proc).Field("male").GetValue<ChaControl>();
            _hFlag = hFlag;
            if (LstHeroine == null)
                LstHeroine = new Dictionary<string, int>();
            _eyeneckFemale = Traverse.Create(proc).Field("eyeneckFemale").GetValue<HMotionEyeNeckFemale>();
            _eyeneckFemale1 = Traverse.Create(proc).Field("eyeneckFemale1").GetValue<HMotionEyeNeckFemale>();
            var charaCount = _chaControl.Count;
            _girlControllers = new List<GirlController>(charaCount);
            FemalePoI = new GameObject[charaCount];

            _moMiController = this.gameObject.AddComponent<MoMiController>();
            _maleController = this.gameObject.AddComponent<MaleController>();
            _loopController = this.gameObject.AddComponent<LoopController>();
            _loopController.Initialize(proc, this);
            for (int i = 0; i < charaCount; i++)
            {
                if (!_redressTargets.ContainsKey(hFlag.lstHeroine[i]))
                {
                    // Can occur on consecutive H scenes.
                    _redressTargets.Add(hFlag.lstHeroine[i], new List<byte>());
                }
                //_heroineList.Add(hFlag.lstHeroine[i]);
                var heroine = this.gameObject.AddComponent<GirlController>();
                heroine.Initialize(i, GetFamiliarity(i));
                _girlControllers.Add(heroine);
            }
            DressDudeForAction();

            // Gameplay Enhancements by ManlyMarco attempts to change this too, but the value changed is irrelevant in practice.
            if (_hFlag.isInsertOK[0])
            {
                _hFlag.isInsertOK[0] = Random.value < 0.75f;
            }
            if (_hFlag.isAnalInsertOK)
            {
                _hFlag.isAnalInsertOK = Random.value < 0.75f;
            }

            var pipi = _chaControlM.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cm_J_dan_top/cm_J_dan100_00");
            TestH.size = (pipi.localScale.x + pipi.localScale.y) * 0.5f;

            UpdateSettings();
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
                if (_hFlag == null)
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
                    else if (!SceneApi.GetAddSceneName().StartsWith("H", StringComparison.Ordinal) && !SceneApi.GetIsNowLoadingFade())
                    {
                        ReDressAfter();
                        yield break;
                    }

                    yield return new WaitForSeconds(1f);
                    continue;
                }
                if (SceneApi.GetIsOverlap())// Scene.IsOverlap)//!IsHProcScene
                {
                    yield return new WaitForSeconds(3f);
                    continue;
                }
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
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Returns (0.25 - 1.0) value based on, well familiarity.
        /// </summary>
        internal float GetFamiliarity(int main)
        {
            var heroine = _hFlag.lstHeroine[main];
            var hExp = 0.55f + ((int)heroine.HExperience * 0.15f);
            if (!_hFlag.isFreeH)
            {
                if (_hFlag.mode != HFlag.EMode.lesbian && _hFlag.mode != HFlag.EMode.masturbation)
                {
#if KK
                    hExp *= 0.5f + heroine.intimacy * 0.05f;
#else
                    hExp *= 0.5f + Mathf.Clamp(heroine.hCount, 0f, 10f) * 0.05f;//   (heroine.lewdness * 0.005f);
#endif
                }
                else
                {
                    hExp *= 1f + (heroine.lewdness * 0.005f);
                }
            }
            else if (_hFlag.mode == HFlag.EMode.lesbian)
            {
                hExp = 1f;
            }
            return hExp;
        }
        public void OnVoiceProc(int main)
        {
            if (_hFlag != null)
            {
                if (SuppressVoice)
                {
                    _girlControllers[main]._lastVoice = _hFlag.voice.playVoices[main];
                    _hFlag.voice.playVoices[main] = -1;
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
            if (_hFlag != null)
            {
                CurrentMain = _hFlag.nowAnimationInfo.nameAnimation.Contains("Alt") ? 1 : 0;

                _loopController.OnPositionChange(nextAnimInfo);
                _moMiController.OnPositionChange(nextAnimInfo);
                _sprite.ForceCloseAllMenu();

                foreach (var girl in _girlControllers)
                {
                    girl.OnPositionChange();
                }
            }

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
                _hFlag.voice.playVoices[0] = voiceId[Random.Range(0, voiceId.Count)] - (Random.value > 0.5f ? 1 : 0);

            }
        }
        public void OnTouch(int item = -1)
        {
            if (_hFlag != null)
            {
                SensibleH.Logger.LogDebug($"ExtraTriggers:Touch");
                _girlControllers[0]._neckController.LookAtPoI(item);
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
        protected override void OnPeriodChange(Cycle.Type period)
        {
            // Implemented by default in KKS.
            foreach (var heroine in Game.Instance.HeroineList)
            {
                for (int i = 0; i < heroine.talkTemper.Count(); i++)
                {
                    heroine.talkTemper[i] = (byte)Random.Range(0, 3);
                }
            }
        }
#endif
        protected override void OnDayChange(Cycle.Week day)
        {
            // Shuffle talk tempers.
            LstHeroine = null;
            MaleOrgCount = 0;
        }
        //protected override void OnEndH(MonoBehaviour _proc, HFlag __hFlag, bool _vr)
        //{
        //    SensibleH.Logger.LogDebug($"OnEndH");
        //    //if (SceneApi.GetLoadSceneName().Equals("Action"))
        //    //{
        //    //    // Lets redress whole school because why not.
        //    //    var chaControls = FindObjectsOfType<ChaControl>().ToList();
        //    //    //.Where(c => c.objTop.activeSelf)

        //    //    foreach (var chara in chaControls)
        //    //    {
        //    //        chara.SetClothesStateAll(0);
        //    //    }
        //    //}
        //}
        private readonly int[] _auxClothesSlots = { 2, 3, 5, 6 };
        private Dictionary<SaveData.Heroine, List<byte>> _redressTargets = new Dictionary<SaveData.Heroine, List<byte>>();
        //private List<SaveData.Heroine> _heroineList = new List<SaveData.Heroine>();

        private void ReDress()
        {
#if KK
            SensibleH.Logger.LogDebug($"ReDress");
            for (var i = 0; i < _chaControl.Count; i++)
            {
                //var heroine = _heroineList[i];
                var chara = _chaControl[i];
                var heroine = _redressTargets.ElementAt(i);
                //_redressTargets.Add(heroine, new List<byte>());
                for (var j = 0; j < chara.fileStatus.clothesState.Length; j++)
                {
                    if (_auxClothesSlots.Contains(j) && chara.fileStatus.clothesState[j] > 1)
                    {
                        heroine.Value.Add(3);
                    }
                    else
                        heroine.Value.Add(0);
                }
                heroine.Key.coordinates[0] = chara.fileStatus.coordinateType;
                heroine.Key.isDresses[0] = false;
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
#endif
            foreach (var target in _redressTargets)
            {
                var chara = target.Key.chaCtrl;
                var clothesState = chara.fileStatus.clothesState;
                for (var i = 0; i < clothesState.Length; i++)
                {
#if KK
                    clothesState[i] = target.Value[i];
#else
                    clothesState[i] = 0;
#endif
                }
                // KKS H Clone has messed up coord index? Coord plugin interferes?

#if KK
                chara.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)target.Key.coordinates[0]);
                _gameMgr.actScene.actCtrl.SetDesire(0, target.Key, 200);
#endif
            }
            _redressTargets.Clear();
        }

        public void EndItAll()
        {
            SensibleH.Logger.LogDebug($"EndItAll");
            if (SceneApi.GetLoadSceneName().Equals("Action"))
            {
                // We are in the main game.
                ReDress();
            }
            _hEnd = true;
            //StopAllCoroutines();
            FemalePoI = null;
            MalePoI = null;

            //_maleController = null;
            //_loopController = null;
            //_moMiController = null;

            Destroy(_moMiController);
            Destroy(_loopController);
            foreach (var controller in _girlControllers)
            {
                Destroy(controller);
            }
            //_girlControllers = null;

            MoMiActive = false;
            OLoop = false;
            MoveNeckGlobal = false;
        }
    }
}