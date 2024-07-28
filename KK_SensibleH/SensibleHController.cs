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
using VRGIN.Core;
using Valve.VR;
using System;
using Manager;
using static SaveData;
using ActionGame.Chara;
using KK_SensibleH.Patches.StaticPatches;
using KK_SensibleH.AutoMode;
using KoikatuVR;
using static IllusionFixes.ResourceUnloadOptimizations;
using IllusionFixes;
using System.Reflection;

namespace KK_SensibleH
{
    /// <summary>
    /// Bugs:
    /// Cyu
    /// Kiss SFX reappeared on disengage phase of the kiss.
    /// </summary>
    public class SensibleHController : GameCustomFunctionController
    {
        public static SensibleHController Instance;

        private MoMiController _moMiController;
        private MaleController _maleController;
        private LoopController _loopController;
        private List<VoiceController> _voiceControllers;
        private List<Harmony> _persistentPatches = new List<Harmony>();
        //private readonly int[] voiceButton = { 3, 5, 6, 8 };
        //private AnimatorStateInfo getCurrentAnimatorStateInfo;
        private readonly int[] _clothes = { 1, 3, 5, 7, 8 };
        private bool _hEnd;
        internal bool _vr;
        private Scene _scene;
        private bool _patched;
        private bool IsConfigScene => _scene.AddSceneName.StartsWith("Con", StringComparison.Ordinal);
        private bool IsPointMoveScene => _scene.AddSceneName.StartsWith("HPo", StringComparison.Ordinal);
        private bool IsHProcScene => _scene.AddSceneName.Equals("HProc");

        //private GameObject _sphere;
        //private void Spawn()
        //{
        //    _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    var tang = FindObjectOfType<ChaControl>().objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/" +
        //        "cf_j_head/cf_s_head/cf_j_tang_01/cf_j_tang_02/cf_j_tang_03/cf_j_tang_04/cf_j_tang_05");
        //    _sphere.transform.SetParent(tang, false);
        //    _sphere.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        //    //sphere.transform.localPosition = new Vector3(0f, -0.015f, -0.06f);
        //    _sphere.GetComponent<Collider>().enabled = false;
        //    _sphere.GetComponent<Renderer>().material.color = new Color(0f, 0f, 1f, 1f);
        //}
        //private void MoveForward()
        //{
        //    _sphere.transform.localPosition += new Vector3(0f, 0f, 0.005f);
        //}
        //private void MoveUp()
        //{
        //    _sphere.transform.localPosition += new Vector3(0f, 0.005f, 0f);
        //}
        //private void Reset()
        //{
        //    _sphere.transform.localPosition = Vector3.zero;
        //}
        private Transform _testform;
        private Transform _eyes;
        private void Update()
        {
            if (_testform != null && _handCtrl.isKiss)
            {
                //SensibleH.Logger.LogDebug($"{vec.position.x} {vec.position.y} {vec.position.z}");
            }
            if (UnityEngine.Input.GetKeyDown(Cfg_TestKey.Value.MainKey) && Cfg_TestKey.Value.Modifiers.All(x => UnityEngine.Input.GetKey(x)))
            {
                //SensibleH.Logger.LogDebug($"Hotkey[1] {_scene.AddSceneName.StartsWith("Con") || _scene.AddSceneName.StartsWith("HPo")}");
            }
            else if (UnityEngine.Input.GetKeyDown(Cfg_TestKey2.Value.MainKey) && Cfg_TestKey2.Value.Modifiers.All(x => UnityEngine.Input.GetKey(x)))
            {
                SensibleH.Logger.LogDebug($"Hotkey[2] {_scene.AddSceneName}");
                //if (_primitiveCube == null)
                //    Spawn();
                //if (_forwardHelper)
                //{
                //    _forward += 0.01f;
                //    if (_forward > 0.1f)
                //        _forwardHelper = false;
                //    _primitiveCube.transform.localPosition = new Vector3(_primitiveCube.transform.localPosition.x, _primitiveCube.transform.localPosition.y, _forward);
                //}
                //else
                //{
                //    _forward -= 0.01f;
                //    if (_forward < -0.1f)
                //        _forwardHelper = true;
                //    _primitiveCube.transform.localPosition = new Vector3(_primitiveCube.transform.localPosition.x, _primitiveCube.transform.localPosition.y, _forward);
                //}
                //SensibleH.Logger.LogDebug($"Hotkey[2] _forward = {_forward}");
            }
            else if (UnityEngine.Input.GetKeyDown(Cfg_TestKey3.Value.MainKey) && Cfg_TestKey3.Value.Modifiers.All(x => UnityEngine.Input.GetKey(x)))
            {
                //if (_primitiveCube == null)
                //    Spawn();
                //if (_upHelper)
                //{
                //    _up += 0.01f;
                //    if (_up > 0.1f)
                //        _upHelper = false;
                //    _primitiveCube.transform.localPosition = new Vector3(_primitiveCube.transform.localPosition.x, _up, _primitiveCube.transform.localPosition.z);
                //else
                //{
                //    _up -= 0.01f;
                //    if (_up < -0.1f)
                //        _upHelper = true;
                //    _primitiveCube.transform.localPosition = new Vector3(_primitiveCube.transform.localPosition.x, _up, _primitiveCube.transform.localPosition.z);
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
        //private void Update()
        //{
        //    if (Input.GetKeyDown(Cfg_TestKey.Value.MainKey) && Cfg_TestKey.Value.Modifiers.All(x => Input.GetKey(x)))
        //    {
        //        if (_sphere == null)
        //            Spawn();
        //        else
        //            MoveForward();
        //        SensibleH.Logger.LogDebug($"SensibleH[Hotkey1] {_sphere.transform.localPosition.y} - {_sphere.transform.localPosition.z}");
        //    }
        //    else if (Input.GetKeyDown(Cfg_TestKey2.Value.MainKey) && Cfg_TestKey2.Value.Modifiers.All(x => Input.GetKey(x)))
        //    {
        //        MoveUp();
        //        SensibleH.Logger.LogDebug($"SensibleH[Hotkey2] {_sphere.transform.localPosition.y} - {_sphere.transform.localPosition.z}");

        //    }
        //    else if (Input.GetKeyDown(Cfg_TestKey3.Value.MainKey) && Cfg_TestKey3.Value.Modifiers.All(x => Input.GetKey(x)))
        //    {
        //        Reset();
        //        SensibleH.Logger.LogDebug($"SensibleH[Hotkey3] {_sphere.transform.localPosition.y} - {_sphere.transform.localPosition.z}");
        //    }

        //        //if (upHelper)
        //        //{
        //        //    itmUp -= 0.01f;
        //        //    if (itmUp <= -0.1f)
        //        //        upHelper = false;
        //        //}
        //        //else
        //        //{
        //        //    itmUp += 0.01f;
        //        //    if (itmUp > 0.1f)
        //        //        upHelper = true;
        //        //}

        //        //if (testLoop)
        //        //{
        //        //    if (!girlController[CurrentMain].IsVoiceActive)
        //        //    {
        //        //        var a = Mathf.Repeat(_chaControl[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
        //        //        if (a > 0.49f && a < 0.51f)
        //        //            girlController[CurrentMain].Reaction();
        //        //    }
        //        //    //if (a < 0.5f)
        //        //    //{
        //        //    //    if (_hFlag.speedCalc < 1f)
        //        //    //        _hFlag.speedCalc = Mathf.Lerp(0.5f, 1f, a * 5f);
        //        //    //}
        //        //    //else
        //        //    //{
        //        //    //    if (_hFlag.speedCalc > 0.55f)
        //        //    //        _hFlag.speedCalc = Mathf.Lerp(1f, 0.55f, a * 1.5f);
        //        //    //}
        //        //    ////if (a > 0.49f && a < 0.51f)
        //        //    ////{
        //        //    ////    SensibleH.Logger.LogDebug($"ProcAt = [{a}]");
        //        //    ////    girlController[CurrentMain].Reaction();
        //        //    ////}

        //        //}
        //        //if (!momi && _handCtrl.action == HandCtrl.HandAction.none && _handCtrl.useAreaItems[2] != null && _handCtrl.useAreaItems[2].obj.name == "p_fingerL")
        //        //{
        //        //    _handCtrl.DetachItemByUseAreaItem(2);
        //        //}
        //    }
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
            if (_vr)
            {
                ResourceUnloadOptimizations.DisableUnload.Value = false;
            }
        }

        //BetterSquirtController.RunSquirts(softSE: true, trigger: BetterSquirt.TriggerType.Touch, handCtrl: main == 0 ? _handCtrl : _handCtrl1);
        private void Start()
        {
            SensibleH.Logger.LogDebug($"Start");
            Instance = this;
            //_vrPov = POV.Instance != null;
        }
        protected override void OnStartH(MonoBehaviour proc, HFlag hFlag, bool vr)
        {
            SensibleH.Logger.LogDebug($"OnStartH");

            // Can't check/patch this on Awake/Start, too early to grab setting.
            _vr = UnityEngine.VR.VRSettings.enabled;
            if (!_patched)
            {
                SensibleH.Logger.LogDebug($"PersistentPatches");
                _patched = true;
                _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchLoop)));
                _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchMoMiAuxiliary)));
                if (_vr)
                {
                    SensibleH.Logger.LogDebug($"PersistentPatches[VR]");
                    _persistentPatches.Add(Harmony.CreateAndPatchAll(typeof(PatchMoMiAuxiliaryVR)));

                }
            }
            if (_vr)
            {
                // The easies way to disable stutters in H when you have 20gb of free RAM.
                // Does it look at VRAM too? Couldn't find it, and not much can be done about it in VR anyway.
                // With this and the patch, there is no more stutters on kiss.
                // And people who actually needs this in VR H scene.. I HIGLY doubt there are any.
                // Once H ends we re-enable it.
                ResourceUnloadOptimizations.DisableUnload.Value = true;
            }
            _scene = Scene.Instance;
            _hEnd = false;
            _handCtrl = Traverse.Create(proc).Field("hand").GetValue<HandCtrl>();
            _handCtrl1 = Traverse.Create(proc).Field("hand1").GetValue<HandCtrl>();
            _hVoiceCtrl = Traverse.Create(proc).Field("voice").GetValue<HVoiceCtrl>();

            // TODO This thingy isn't stock, add it.
            var colliderPlane = CommonLib.LoadAsset<GameObject>($"studio/pine_effect.unity3d", "ColliderPlane", clone: true);
            colliderPlane.GetComponent<Renderer>().enabled = false;
            colliderPlane.transform.position = new Vector3(0f, 0.1f, 0f);

            _chaControl = Traverse.Create(proc).Field("lstFemale").GetValue<List<ChaControl>>();
            _chaControlM = Traverse.Create(proc).Field("male").GetValue<ChaControl>();
            _hFlag = hFlag;
            if (LstHeroine == null)
                LstHeroine = new Dictionary<string, int>();
            _eyeneckFemale = Traverse.Create(proc).Field("eyeneckFemale").GetValue<HMotionEyeNeckFemale>();
            _eyeneckFemale1 = Traverse.Create(proc).Field("eyeneckFemale1").GetValue<HMotionEyeNeckFemale>();
            var charaCount = _chaControl.Count;
            _girlControllers = new List<GirlController>(charaCount);
            _voiceControllers = new List<VoiceController>(charaCount);
            FemalePoI = new GameObject[charaCount];

            _moMiController = this.gameObject.AddComponent<MoMiController>();
            _maleController = this.gameObject.AddComponent<MaleController>();
            _loopController = this.gameObject.AddComponent<LoopController>();
            _loopController.Initialize(proc);
            for (int i = 0; i < charaCount; i++)
            {
                _heroineList.Add(hFlag.lstHeroine[0]);
                var heroine = this.gameObject.AddComponent<GirlController>();
                heroine.Initialize(i, _loopController.GetFamiliarity(i));
                _girlControllers.Add(heroine);
                var voice = this.gameObject.AddComponent<VoiceController>();
                voice.Initialize(i);
                _voiceControllers.Add(voice);
            }
            foreach (var num in _clothes)
                _chaControlM.SetClothesStateNext(num);

            // Gameplay Enhancements by ManlyMarco attempts to change this too, but the value changed is irrelevant in practice.
            if (_hFlag.isInsertOK[0])
                _hFlag.isInsertOK[0] = Random.value < 0.75f;
            if (_hFlag.isAnalInsertOK)
                _hFlag.isAnalInsertOK = Random.value < 0.75f;

            StartCoroutine(OnceInAwhile());

            _eyes = _chaControl[0].objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            _testform = _chaControl[0].transform.parent.Find("CameraBase/Camera");
            //var kokan = _chaControl[0].objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cf_j_kokan/a_n_kokan");
            //kokan.SetParent(_chaControl[0].objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan"), true);
        }
        private IEnumerator OnceInAwhile()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (_hFlag == null)
                {
                    if (!_hEnd)
                    {
                        EndItAll();
                        if (!_scene.LoadSceneName.Equals("Action"))
                            yield break;
                    }
                    else if (!_scene.AddSceneName.StartsWith("H", StringComparison.Ordinal) && !_scene.IsNowLoadingFade)
                    {
                        ReDressAfter();
                        yield break;
                    }

                    yield return new WaitForSeconds(1f);
                    continue;
                }
                if (!IsHProcScene)
                {
                    yield return new WaitForSeconds(3f);
                    continue;
                }
                _loopController.Proc();
                for (var i = 0; i < _girlControllers.Count; i++)
                {
                    if (SensibleH.EyeNeckControl.Value)
                    {
                        _girlControllers[i].Proc();
                    }
                    _voiceControllers[i].Proc();
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
                    FirstTouch = !_handCtrl.IsItemTouch();
                yield return new WaitForSeconds(1f);
            }
        }


        public void DoVoiceProc(int main)
        {
            //if (!_voiceControllers[main].SayNickname())
            //{
            _voiceControllers[main].OnVoiceProc();
            //}
        }
        public void OnPositionChange(HSceneProc.AnimationListInfo nextAnimInfo)
        {
            if (_hFlag == null)
                return;

            CurrentMain = _hFlag.nowAnimationInfo.nameAnimation.Contains("Alt") ? 1 : 0;

            _loopController.OnPositionChange();
            _sprite.ForceCloseAllMenu();
            
            foreach (var girl in _girlControllers)
            {
                girl.OnPositionChange();
            }
        }
        public void DoFirstTouchProc()
        {
            List<int> voiceId = new List<int>();
            if (_handCtrl.useAreaItems[0] != null)
            {
                switch (_handCtrl.useAreaItems[0].idObj)
                {
                    case 0: // hand
                        voiceId.Add(112);
                        break;
                    case 1: // finger
                        voiceId.Add(124);
                        break;
                    case 2: // tongue
                        voiceId.Add(132);
                        break;
                    case 3: // baibu
                        voiceId.Add(138);
                        break;
                }
            }
            if (_handCtrl.useAreaItems[1] != null)
            {
                switch (_handCtrl.useAreaItems[1].idObj)
                {
                    case 0: // hand
                        voiceId.Add(112);
                        break;
                    case 1: // finger
                        voiceId.Add(124);
                        break;
                    case 2: // tongue
                        voiceId.Add(132);
                        break;
                    case 3: // baibu
                        voiceId.Add(138);
                        break;
                }
            }
            if (_handCtrl.useAreaItems[2] != null)
            {
                switch (_handCtrl.useAreaItems[2].idObj)
                {
                    case 0: // hand
                        voiceId.Add(114);
                        break;
                    case 1: // finger
                        voiceId.Add(120);
                        break;
                    case 2: // tongue
                        voiceId.Add(126);
                        break;
                    case 3: // baibu
                        voiceId.Add(134);
                        break;
                    case 4: // dildo
                        voiceId.Add(140);
                        break;
                }
            }
            if (_handCtrl.useAreaItems[3] != null)
            {
                switch (_handCtrl.useAreaItems[3].idObj)
                {
                    case 0: // hand
                        voiceId.Add(116);
                        break;
                    case 1: // finger
                        voiceId.Add(121);
                        voiceId.Add(122);
                        break;
                    case 2: // tongue
                        voiceId.Add(128);
                        break;
                }
            }
            if (_handCtrl.useAreaItems[4] != null)
            {
                switch (_handCtrl.useAreaItems[4].idObj)
                {
                    case 0: // hand
                        voiceId.Add(118);
                        break;
                    case 2: // tongue
                        voiceId.Add(130);
                        break;
                    case 3: // baibu
                        voiceId.Add(136);
                        break;
                }
            }
            if (_handCtrl.useAreaItems[5] != null)
            {
                switch (_handCtrl.useAreaItems[5].idObj)
                {
                    case 0: // hand
                        voiceId.Add(118);
                        break;
                    case 2: // tongue
                        voiceId.Add(130);
                        break;
                    case 3: // baibu
                        voiceId.Add(136);
                        break;
                }
            }
            // Check in case of MainGameVR shenanigans.
            if (voiceId.Count != 0)
                _hFlag.voice.playVoices[0] = voiceId.ElementAt(Random.Range(0, voiceId.Count));
        }
        protected override void OnDayChange(Cycle.Week day)
        {
            // Shuffle talk tempers.
            LstHeroine = null;
            MaleOrgCount = 0;
            var game = Game.Instance;
            foreach (var heroine in game.HeroineList)
            {
                for (int i = 0; i < heroine.talkTemper.Count(); i++)
                {
                    heroine.talkTemper[i] = (byte)Random.Range(0, 3);
                }
                if (heroine.isAnger)
                {
                    heroine.anger -= Random.Range(5, 15);
                    heroine.isAnger = heroine.anger < Random.Range(0, 20);
                }
            }
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
        private Dictionary<SaveData.Heroine, List<byte>> _redressTargets = new Dictionary<Heroine, List<byte>>();
        private List<SaveData.Heroine> _heroineList = new List<SaveData.Heroine>();
        //private Dictionary<SaveData.Heroine, int> _redressTargets = new Dictionary<SaveData.Heroine, int>();

        private void ReDress()
        {
            SensibleH.Logger.LogDebug($"ReDress");
            //for (var i = 0;  i < _chaControl.Count; i++)
            //{
            //    var heroine = _heroineList[i];
            //    var chara = _chaControl[i];
            //    SensibleH.Logger.LogDebug($"ReDress[Chara]");
            //    var list = new List<int>();
            //    for (var j = 0; j < chara.fileStatus.clothesState.Length; j++)
            //    {
            //        if (_auxClothesSlots.Contains(j) && chara.fileStatus.clothesState[j] > 1)
            //            chara.fileStatus.clothesState[j] = 3;
            //        else
            //            chara.fileStatus.clothesState[j] = 0;
            //    }
            //    chara.UpdateClothesStateAll();
            //    //foreach (var slot in _auxClothesSlots)
            //    //{
            //    //    if (chara.fileStatus.clothesState[slot] > 1)
            //    //        list.Add(slot);
            //    //}
            //    //chara.SetClothesStateAll(0);
            //    //foreach (var item in list)
            //    //{
            //    //    SensibleH.Logger.LogDebug($"UnDressBack[{item}]");
            //    //    chara.SetClothesState(item, 3, false);
            //    //}
            //    heroine.chaCtrl.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)chara.fileStatus.coordinateType);
            //    heroine.coordinates[0] = chara.fileStatus.coordinateType;
            //    heroine.isDresses[0] = false;
            //    //heroine.chaCtrl.AssignCoordinate((ChaFileDefine.CoordinateType)_chaControl[i].fileStatus.coordinateType);
            //    //heroine.chaCtrl.ChangeClothes(true);
            //}
            for (var i = 0; i < _chaControl.Count; i++)
            {
                var heroine = _heroineList[i];
                var chara = _chaControl[i];
                _redressTargets.Add(heroine, new List<byte>());
                for (var j = 0; j < chara.fileStatus.clothesState.Length; j++)
                {
                    if (_auxClothesSlots.Contains(j) && chara.fileStatus.clothesState[j] > 1)
                        _redressTargets[heroine].Add(3);
                    else
                        _redressTargets[heroine].Add(0);
                }
                heroine.coordinates[0] = chara.fileStatus.coordinateType;
                heroine.isDresses[0] = false;
            }
            _heroineList.Clear();
            //_heroineList.Clear();
            //var outsideCharas = FindObjectsOfType<ChaControl>()
            //        .Where(c => !c.objTop.activeSelf && nameList.Contains(c.fileParam.fullname))
            //        .ToList();
            //foreach (var chara in outsideCharas)
            //{
            //    SensibleH.Logger.LogDebug($"ReDress[outsideCharas]");
            //    var clone = _chaControl
            //        .Where(c => c.fileParam.fullname == chara.fileParam.fullname)
            //        .FirstOrDefault();
            //    chara.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)clone.fileStatus.coordinateType, false);
            //}


        }
        private void ReDressAfter()
        {
            //var _gameMgr = Game.Instance;
            SensibleH.Logger.LogDebug($"ReDressAfter");
            //foreach (var heroine in _heroineList)
            //{
            //    SensibleH.Logger.LogDebug($"ReDressAfter[Chara]");
            //    _gameMgr.actScene.actCtrl.SetDesire(0, heroine, 200);
            //}
            //_heroineList.Clear();
            //_chaControl[0].ChangeCoordinateTypeAndReload(ChaFileDefine.CoordinateType.School01, false);


            var _gameMgr = Game.Instance;
            foreach (var target in _redressTargets)
            {
                var chara = target.Key.chaCtrl;
                var clothesState = chara.fileStatus.clothesState;
                for (var i = 0; i < clothesState.Length; i++)
                {
                    clothesState[i] = target.Value[i];
                }
                chara.ChangeCoordinateTypeAndReload((ChaFileDefine.CoordinateType)target.Key.coordinates[0]);
                _gameMgr.actScene.actCtrl.SetDesire(0, target.Key, 200);
            }
            _redressTargets.Clear();
        }

        public void EndItAll()
        {
            SensibleH.Logger.LogDebug($"EndItAll");
            if (SceneApi.GetLoadSceneName().Equals("Action"))
                ReDress();
            if (_vr)
            {
                ResourceUnloadOptimizations.DisableUnload.Value = false;
            }
            _hEnd = true;
            //StopAllCoroutines();
            FemalePoI = null;
            MalePoI = null;

            _girlControllers = null;
            //_maleController = null;
            //_loopController = null;
            //_moMiController = null;
            _voiceControllers = null;

            Destroy(_moMiController);
            Destroy(_loopController);

            MoMiActive = false;
            OLoop = false;
            MoveNeckGlobal = false;
        }
    }
}