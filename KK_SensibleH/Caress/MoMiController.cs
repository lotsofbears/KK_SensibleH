using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using static KK_SensibleH.SensibleH;
using static KK_SensibleH.GirlController;
using System.Collections;
using System.Linq;
using System;
using Illusion.Game;
using KK_SensibleH.Patches.DynamicPatches;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Controls;
using KoikatuVR;
using KoikatuVR.Caress;
using static SteamVR_Controller;
using KK_SensibleH.Caress;
using static KK_SensibleH.AutoMode.LoopProperties;
using KK_SensibleH.AutoMode;
using KK_SensibleH.EyeNeckControl;
using NodeCanvas.Tasks.Actions;
using static Illusion.Game.Utils.Scene;
using UnityEngine.VR;

namespace KK_SensibleH
{
    /*
     * TODO:
     * 
     * Squirts:  - while inside, change Pitch/Yaw, and make it Y / W shaped across the Yaw
     *
     */
    /// <summary>
    /// 
    /// </summary>
    public class MoMiController : MonoBehaviour
    {
        class ActiveItem
        {
            public int area;
            public int obj;
            public int pattern;
            public float deg;
            public float step;
            public float intensity;
            public float loopEndTime;
            public int peak;
            public int range;
            public bool hasPair;
            public bool leadsPair;
            public bool inPair;
            public bool startPair;
        }
        class LickItem
        {
            public string path;
            public float itemOffsetForward;
            public float itemOffsetUp;
            public float poiOffsetUp;
            public float directionUp;
            public float directionForward;
        }
        public static string[] FakePrefix = new string[3];
        public static string[] FakePostfix = new string[3];
        public static Vector2 FakeDragLength;
        public static MoMiController Instance;

        internal static bool _kissCo;
        internal static bool _endKissCo;
        internal static bool _lickCo;
        internal static bool _moMiCo;

        private float[] _postfixTimers = new float[3];
        private bool _endLickCo;
        //private bool _moMiCo;
        private bool _fakeTag;
        private bool _vr;
        private bool _judgeCooldown;
        private bool _touchAnim;
        private bool _mousePressDown;
        internal bool _aibu;
        internal bool _houshi;
        internal bool _sonyu;

        private Transform _eyes;
        private Transform _head;
        private Transform _neck;
        private Transform _maleEyes;
        private Transform _shoulders;
        private SteamVR_Controller.Device _device;
        private SteamVR_Controller.Device _device1;
        private Controller _controller;
        private Controller _controller1;
        private List<MoMiCircles> _circles = new List<MoMiCircles>();
        private Thread _thread;
        private GameCursor _gameCursor;
        internal List<Harmony> _activePatches = new List<Harmony>();
        internal List<Coroutine> _activeCoroutines = new List<Coroutine>();
        private Dictionary<int, ActiveItem> _items = new Dictionary<int, ActiveItem>();
        private float GetFpsDelta => Time.deltaTime * 60f;
        private void Awake()
        {
            Instance = this;
            _gameCursor = GameCursor.Instance;
            _vr = UnityEngine.VR.VRSettings.enabled;
            for (var i = 0; i < 3; i++)
            {
                _circles.Add(new MoMiCircles());
            }
            if (_vr)
            {
                this.gameObject.AddComponent<Kiss>();
                _thread = this.gameObject.AddComponent<Thread>();
                var chara = _chaControl[0];
                _eyes = chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
                _head = chara.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head");
                _neck = chara.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck");
                _maleEyes = chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz");
                _shoulders = chara.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_backsk_00");
            }
        }

        /*
         * KissEnd check for upright target
         * Cyu Propper attach point and it's variability
         * Cyu for lick.
         * 
         * 
         */
        private void OnDestroy()
        {
            //SensibleH.Logger.LogDebug("MoMi[OnDestroy]");
            foreach (var activePatch in _activePatches)
                activePatch?.UnpatchSelf();
        }
        private void StartMoMi()
        {
            _activeCoroutines.Add(StartCoroutine(MoMiCo()));
        }
        internal void Halt(bool disengage = true)
        {
            SensibleH.Logger.LogDebug($"MoMi[HaltReason][Button = {UnityEngine.Input.GetMouseButtonDown(0)}] [Item = {_handCtrl.actionUseItem != -1}] [Kiss = {_handCtrl.isKiss}]");
            foreach (var coroutine in _activeCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            foreach (var patch in _activePatches)
            {
                patch.UnpatchSelf();
            }

            _activeCoroutines.Clear();
            _activePatches.Clear();
            _items.Clear();

            Cursor.visible = true;
            _gameCursor.UnLockCursor();

            if (_vr)
            {
                if (_kissCo)
                {
                    _girlControllers[0]._eyeNeckController.OnKissVrEnd();
                }
                if (disengage && (_kissCo || _lickCo))
                {
                    _activeCoroutines.Add(StartCoroutine(EndKissCo()));
                }
                if (_mousePressDown)
                {
                    HandCtrlHooks.InjectMouseButtonUp(0);
                    //_injectMouseButtonUp(0);
                    _mousePressDown = false;
                }
            }
            _moMiCo = false;
            _kissCo = false;
            _lickCo = false;
            MoMiActive = false;
        }
        //private void RangeTest()
        //{
        //    var origin = VR.Camera.Origin;
        //    var head = VR.Camera.Head;
        //    var item = _handCtrl.useItems[2].obj.transform.Find("cf_j_tangroot");
        //    var poi = _chaControl[0].objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_s_waist02/k_f_kosi02_02");
        //    var moveTo = item.position + (item.forward * itmFrwrd) + (item.up * itmUp);
        //    var lookAt = Quaternion.LookRotation(poi.position + poi.up * 0.05f - moveTo, -poi.forward);
        //    origin.rotation = lookAt;
        //    origin.position += moveTo - head.position;
        //}
        //private bool frwrdHelper;
        //private bool upHelper;
        //private float itmFrwrd = 0f;
        //private float itmUp = 0f;
       
        //private void SetDelegate()
        //{
        //    _vrMouth = FindObjectOfType<VRMouth>();
        //    //object a = AccessTools.FieldRefAccess<bool?>(typeof(VRMouth), "_lickCoShouldEnd"); //AccessTools.FieldRefAccess<VRMouth,bool?>(_vrMouth, "_lickCoShouldEnd");
        //    var type = AccessTools.TypeByName("KoikatuVR.Caress.HandCtrlHooks");
        //    var method = AccessTools.FirstMethod(type, m => m.Name.Contains("InjectMouseButtonUp"));
        //    _injectMouseButtonUp = AccessTools.MethodDelegate<InjectMouseButtonUp>(method);
        //}
        private void Update()
        {
            // We properly manage it now.
            //if (_hFlag == null)
            //    return;

            // There is a little to no point to run moMi on the kiss outside of VR. So we don't, saves ADHD VR users from quite a few bugs.
            if (!_moMiCo && GameCursor.isLock && _handCtrl.actionUseItem != -1)// || _handCtrl.isKiss))  || _lickCo)
            {
                StartMoMi();
            }
            else if (_moMiCo)
            {
                _touchAnim = IsTouch;
                if (UnityEngine.Input.GetMouseButtonDown(0) || (_handCtrl.actionUseItem == -1 && !_handCtrl.isKiss))
                {
                    Halt();
                }
                else if (_fakeTag)
                {
                    var count = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        // Fake conditions for animation restarts of specific items.
                        if (_postfixTimers[i] < Time.time)
                        {
                            FakePostfix[i] = null;
                            count++;
                        }
                    }
                    if (count == 3)
                    {
                        _fakeTag = false;
                        _judgeCooldown = false;
                    }
                }
            }
            if (UnityEngine.Input.GetKeyDown(Cfg_TestKey.Value.MainKey) && Cfg_TestKey.Value.Modifiers.All(x => UnityEngine.Input.GetKey(x)))
            {
                //RangeTest();
                //SensibleH.Logger.LogDebug($"Hotkey[1] {Vector3.Distance(_eyes.position, VR.Camera.transform.position)} {Vector3.SqrMagnitude(_eyes.position - VR.Camera.transform.position)}");
            }
            else if (UnityEngine.Input.GetKeyDown(Cfg_TestKey2.Value.MainKey) && Cfg_TestKey2.Value.Modifiers.All(x => UnityEngine.Input.GetKey(x)))
            {
                //if (frwrdHelper)
                //{
                //    itmFrwrd -= 0.01f;
                //    if (itmFrwrd <= -0.1f)
                //        frwrdHelper = false;
                //}
                //else
                //{
                //    itmFrwrd += 0.01f;
                //    if (itmFrwrd > 0.1f)
                //        frwrdHelper = true;
                //}
                //SensibleH.Logger.LogDebug($"SensibleH[Hotkey2] frwrdHelper[{itmFrwrd}]");
            }
            else if (UnityEngine.Input.GetKeyDown(Cfg_TestKey3.Value.MainKey) && Cfg_TestKey3.Value.Modifiers.All(x => UnityEngine.Input.GetKey(x)))
            {

                //if (upHelper)
                //{
                //    itmUp -= 0.01f;
                //    if (itmUp <= -0.1f)
                //        upHelper = false;
                //}
                //else
                //{
                //    itmUp += 0.01f;
                //    if (itmUp > 0.1f)
                //        upHelper = true;
                //}
                //SensibleH.Logger.LogDebug($"SensibleH[Hotkey3] upHelper[{itmUp}]");
            }
        }
        /// <summary>
        /// Initiates automatic movement of items, looks what to patch.
        /// </summary>
        private IEnumerator MoMiCo(bool skipWait = false)
        {
            SensibleH.Logger.LogDebug($"MoMiCo[StartReason] item[{_handCtrl.actionUseItem != -1}] kiss[{_handCtrl.isKiss}]");
            _moMiCo = true;
            if (!skipWait)
            {
                // Update will be doing this from next frame for everybody.
                _touchAnim = IsTouch;

                yield return new WaitUntil(() => !_touchAnim);
                yield return new WaitForSeconds(1f); 

                if (!_handCtrl.isKiss && _handCtrl.actionUseItem == -1)
                {
                    Halt();
                    yield break;
                }
            }
            var kiss = _handCtrl.isKiss;
            SensibleH.Logger.LogDebug($"MoMiCo[Start] kiss[{kiss}] vr[{_vr}]");
            MoMiActive = true;
            if (!kiss)
            {
                SensibleH.Logger.LogDebug($"MoMiCo[PatchMoMi]");
                _activePatches.Add(Harmony.CreateAndPatchAll(typeof(PatchHandCtrl)));
                if (!_lickCo)
                {
                    Utils.Sound.Play(SystemSE.ok_l);
                    if (_vr)
                    {
                        UpdateDevices();
                        _activePatches.Add(Harmony.CreateAndPatchAll(typeof(PatchKoikatuVR)));

                        if (_device.GetPress(ButtonMask.Trigger))
                            yield return new WaitUntil(() => _device.GetPressUp(ButtonMask.Trigger));
                        else if (_device1.GetPress(ButtonMask.Trigger))
                            yield return new WaitUntil(() => _device1.GetPressUp(ButtonMask.Trigger));
                        _mousePressDown = true;
                        //vrPatch.UnpatchSelf();
                    }
                    else
                    {
                        if (UnityEngine.Input.GetMouseButton(0))
                        {
                            yield return new WaitUntil(() => UnityEngine.Input.GetMouseButtonUp(0));
                        }
                        else
                        {
                            //SensibleH.Logger.LogDebug($"MoMiCo[Halt] Mouse button was released too soon");
                            _moMiCo = false;
                            yield break;
                        }
                    }
                }
            }
            foreach (var item in _handCtrl.useItems)
            {
                if (item != null)
                    _items.Add(
                        item.idUse, new ActiveItem
                        {
                            area = (int)item.kindTouch - 2,
                            obj = item.idObj,
                            pattern = -1
                        });
            }
            //SensibleH.Logger.LogDebug($"MoMiCo[AddItem] {_items.Count} _items added");
            foreach (var item in _items)
            {
                if (item.Value.area == 0 || item.Value.area == 4)
                {
                    var otherItem = _items.Values
                        .Where(i => i.area - 1 == item.Value.area)
                        .FirstOrDefault();
                    if (otherItem != null)
                    {
                        SensibleH.Logger.LogDebug($"Found the pair[{item.Value.area}][{otherItem.area}] in _items");
                        item.Value.hasPair = true;
                        otherItem.hasPair = true;
                    }
                }
                _activeCoroutines.Add(StartCoroutine(ItemCo(kiss, item.Key)));
            }
            SensibleH.Logger.LogDebug($"MoMiCo[Finish]");
        }
        /// <summary>
        /// Moves attached items.
        /// </summary>
        private IEnumerator ItemCo(bool kiss, int itemId)
        {
            SensibleH.Logger.LogDebug($"ItemCo[{itemId}][Online]");
            var judgeProc = !kiss;
            var item = _items[itemId];
            var midPos = new Vector2(0.5f, 0.5f);
            while (true)
            {
                // Variable judgeProc mainly keeps track of our current position (middle or anywhere but);
                if (item.pattern == -1 || item.startPair)
                    judgeProc = true;

                if (item.startPair || Random.value < 0.5f)
                {
                    if (!judgeProc && IsAdjustmentNeeded(itemId))
                    {
                        //SensibleH.Logger.LogDebug($"ItemCo[{idUse}][MoveToCenter]");
                        // Move item to the center.
                        var currentPos = _circles[itemId].GetPosition(item.pattern, item.deg,  item.step, item.intensity, item.peak, item.range, out item.deg);
                        var deltaPos = midPos - currentPos;
                        var step = 0.6667f * Time.deltaTime;
                        var allSteps = deltaPos.magnitude / step;
                        var stepVec = deltaPos / allSteps;

                        while (allSteps-- > 1)
                        {
                            if (_touchAnim)
                            {
                                yield return new WaitUntil(() => !_touchAnim);
                                //yield return new WaitForEndOfFrame();
                            }
                            // There are cases when the game "helps" us with wrong vector.
                            _hFlag.xy[item.area] = currentPos += stepVec;
                            yield return new WaitForEndOfFrame();
                        }
                        yield return new WaitForSeconds(0.1f);
                    }

                    // If we want our judge procs after CaressAreaReaction() at aesthetic timings, there has to be atleast 2 of them.
                    // Otherwise we'll get a bad state and premature Halt().
                    // Proper wait after judgeProc is within the range 0.55f - 0.60f, 0.55f looks well and doesn't fall off all that often.
                    judgeProc = true;
                    if (JudgeProc(itemId))
                    {
                        yield return new WaitForSeconds(0.4f);

                        var wait = 0f;
                        if (Random.value < 0.4f)
                        {
                            _girlControllers[0].Reaction();

                            if (Random.value < 0.5f)
                            {
                                //_girlController[0].PlayShort();
                                wait = CaressAreaReaction(itemId);
                                _girlControllers[0]._eyeNeckController.LookAway();
                            }
                            else
                            {
                                _girlControllers[0]._eyeNeckController.LookAtPoI(5f);
                                yield return new WaitForSeconds(0.15f);
                            }
                        }
                        else
                            yield return new WaitForSeconds(0.15f);
                        var num = 0.5f;
                        while (Random.value < num)
                        {
                            if (wait != 0f)
                            {
                                yield return new WaitForSeconds(wait);
                                wait = 0f;
                                if (JudgeProc(itemId))
                                    yield return new WaitForSeconds(0.55f);
                                else
                                    break;
                            }

                            JudgeProc(itemId);
                            if (Random.value < num / 2f)
                            {
                                yield return new WaitForSeconds(0.4f);
                                _girlControllers[0].Reaction();
                                yield return new WaitForSeconds(0.15f);
                            }
                            else
                                yield return new WaitForSeconds(0.55f);
                            num -= 0.1f;
                        }
                        // The time that CrossFader needs to return the body to an active state after CaressAreaReaction().
                        if (wait != 0f)
                            yield return new WaitForSeconds(1f); // 0.9f
                    }
                    
                }

                OrganizeDictionary(itemId, judgeProc);

                if (judgeProc && IsAdjustmentNeeded(itemId) && item.pattern != -1)
                {
                    //SensibleH.Logger.LogDebug($"ItemCo[{idUse}][MoveToPos]");
                    // Move item from center to its initial position.
                    var targetPos = _circles[itemId].GetPosition(item.pattern, item.deg, item.step, item.intensity, item.peak, item.range, out item.deg);
                    var currentPos = midPos;
                    var deltaPos = targetPos - currentPos;
                    var step = 0.6667f * Time.deltaTime;
                    var allSteps = deltaPos.magnitude / step;
                    var stepVec = deltaPos / allSteps;

                    while (allSteps-- > 1)
                    {
                        if (_touchAnim)
                            yield return new WaitUntil(() => !_touchAnim);
                        _hFlag.xy[item.area] = currentPos += stepVec;
                        yield return new WaitForEndOfFrame();
                    }
                    judgeProc = false;
                }
                judgeProc = item.pattern == -1 ? true : false;
                while (item.loopEndTime > Time.time)
                {
                    if (judgeProc)
                    {
                        if (JudgeProc(itemId))
                            yield return new WaitForSeconds(0.55f);
                        else
                            break;
                    }
                    else
                    {
                        if (!_touchAnim)
                        {
                            _hFlag.xy[item.area] = _circles[itemId].GetPosition(item.pattern, item.deg, item.step, item.intensity, item.peak, item.range, out item.deg);
                            if (FakeDragLength == Vector2.zero)
                                FakeDragLength = _hFlag.xy[item.area];
                        }
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }
        private void OrganizeDictionary(int idUse, bool midPos)
        {
            var item = _items[idUse];
            if (item.inPair && !item.leadsPair)
            {
                PairItems(idUse, midPos);
                if (item.startPair)
                    item.startPair = false;
            }
            else if (!_lickCo && !item.inPair && item.hasPair && Random.value < 0.4f)
            {
                var otherItem = _items.Values
                    .Where(v => v.hasPair && v.area != item.area)
                    .FirstOrDefault();

                item.inPair = true;
                item.loopEndTime = otherItem.loopEndTime - Time.deltaTime;
                item.leadsPair = true;

                otherItem.inPair = true;
                item.startPair = true;
                otherItem.startPair = true;
                SensibleH.Logger.LogDebug($"MomiItem[{idUse}]OrganizeDic:SetPair midPos[{midPos}] ptn[{item.pattern}]");
            }
            else if (!item.inPair || (item.inPair && item.leadsPair))
            {
                if (midPos)
                {
                    item.intensity = (float)Math.Round(Random.Range(1f, 3f), 1);
                    item.deg = Random.Range(0, 360);
                }

                item.peak = Random.Range(0, 360);
                item.range = Random.Range(45, 120);
                item.pattern = PickPattern(item.area, item.obj);
                item.loopEndTime = Time.time + Random.Range(2f, 12f);
                if (_lickCo && idUse == 2)
                    item.step = (float)Math.Round(Random.Range(1f, 2f) * (Time.deltaTime * 60f), 1);
                else
                    item.step = (float)Math.Round(Random.Range(2f, 5f) * (Time.deltaTime * 60f), 1);

                if (item.startPair)
                    item.startPair = false;
                SensibleH.Logger.LogDebug($"ItemCo[{idUse}][OrganizeDic] Master[{item.leadsPair}] midPos[{midPos}] ptn[{item.pattern}] step[{item.step}] int[{item.intensity}] deg[{item.deg}]");
            }
        }
        private void PairItems(int idUse, bool midPos)
        {
            var item = _items[idUse];
            //var fpsDelta = GetFpsDelta;

            var leaderItem = _items.Values
                .Where(i => i.hasPair && i.area != item.area)
                .FirstOrDefault();

            if (midPos)
            {
                var link = LinkItems(leaderItem.deg, leaderItem.peak, out item.deg, out item.peak);
                item.intensity = leaderItem.intensity;
                item.pattern = PickPattern(item.area, item.obj, link, leaderItem.pattern);
            }
            else
            {
                item.deg = leaderItem.deg;
                item.peak = leaderItem.peak;
                item.pattern = leaderItem.pattern;
            }
            item.step = leaderItem.step;
            item.range = leaderItem.range;
            item.loopEndTime = leaderItem.loopEndTime + Time.deltaTime;

            if (Random.value < 0.25f)
            {
                item.inPair = false;
                leaderItem.inPair = false;
                leaderItem.leadsPair = false;
                SensibleH.Logger.LogDebug($"ItemCo[{idUse}][PairItems] Slave [TurnOffPairing]");
            }
            SensibleH.Logger.LogDebug($"ItemCo[{idUse}][PairItems]Slave midPos[{midPos}] ptn[{item.pattern}] step[{item.step}] int[{item.intensity}] deg[{item.deg}]");
        }
        private bool IsAdjustmentNeeded(int idUse)
        {
            return true;
            //if (idUse == 2 && _lickCo)
            //{
            //    return true;
            //}
            //else
            //{
            //    var area = _handCtrl.useItems[idUse].kindTouch;
            //    return area != HandCtrl.AibuColliderKind.kokan && area != HandCtrl.AibuColliderKind.anal;
            //}
        }

        private bool JudgeProc(int item, bool fakeIt = false)
        {
            // It's way too much trouble to keep "JudgeProc()" during lick/kiss action, especially given that the player will hardly see/like it at all.
            if (!fakeIt && (_kissCo || (_judgeCooldown && FakePostfix[item] == null))) //(_kissCo || _lickCo || 
                return false;
            else
            {
                if (!fakeIt)
                {
                    FakePrefix[item] = String.Empty;
                    _handCtrl.JudgeProc();
                    FakePrefix[item] = null;
                }
                FakePostfix[item] = String.Empty;

                // Ideally timing will be between 0.57f and 0.59f, but on random stutter it can go a bit higher.
                _postfixTimers[item] = Time.time + 0.65f;
                _fakeTag = true;
                _judgeCooldown = true;
                SensibleH.Logger.LogDebug($"JudgeProc[{item}][fakeIt: {fakeIt}]");
                return true;
            }
        }
        private Link LinkItems(float deg, int peak, out float outDeg, out int outPeak)
        {
            var link = (Link)Random.Range(0, 4);
            var number = Random.Range(1, 3) * (Random.value < 0.33f ? -90 : 90);
            
            if (link == Link.Asynchronous || link == Link.Asymmetrical)
            {
                outDeg = deg + number;
                outPeak = peak + number;
            }
            else
            {
                outDeg = deg;
                outPeak = peak;
            }
            return link;
        }
        private int PickPattern(int area, int obj, Link link = Link.None, int linkPtn = 0)
        {
            int result;
            if (link != Link.None)
            {
                if (link == Link.Symmetrical || link == Link.Asymmetrical)
                {
                    if (linkPtn < 12)
                    {
                        var index = linkPtn % 4;
                        var sectionStart = linkPtn / 4 * 4;
                        if (index / 2 == 0)
                        {
                            // first half
                            result = sectionStart + 2 + index;
                        }
                        else
                            result = sectionStart + index;
                    }
                    else
                    {
                        // Half circle.
                        result = 12;
                    }
                }
                else
                {
                    result = linkPtn;
                }
            }
            else
            {
                if (area == 2 || area == 3)
                    result = Random.value < 0.25 ? -1 : Random.Range(13, 18);
                else
                    result = Random.value < 0.1 ? -1 : Random.Range(0, 13);
            }
              
            //SensibleH.Logger.LogDebug($"PickPattern:Area[{area}] = [{result}]");
            return result;
        }
        private enum Link
        {
            Synchronous, // All in one direction.
            Asynchronous, // All in one direction but flipped? start/peak.
            Symmetrical, // Mirroring each other.
            Asymmetrical, // Mirroring each other but flipped? start/peak. 
            None
        }
        private float CaressAreaReaction(int target)
        {
            // Timings are crossbreed of aesthetic and breakability.
            // Rarely falls off and Halt()s, but nothing breaks.
            if (IsTouch || _hFlag.mode != HFlag.EMode.aibu)
                return 0f;

            HFlag.ClickKind click;
            float waitTime;

            var itemObj = _handCtrl.useItems[target].idObj;
            var activeArea = _handCtrl.useItems[target].kindTouch;
            SensibleH.Logger.LogDebug($"CaressAreaReaction area[{activeArea}], item[{itemObj}]");
            if (activeArea == HandCtrl.AibuColliderKind.muneL || activeArea == HandCtrl.AibuColliderKind.muneR)
            {
                if (itemObj == 0 || itemObj == 3)
                    waitTime = 0.25f;
                else
                    waitTime = 0.65f;
                click = (HFlag.ClickKind)(14 + activeArea);
            }
            else if (activeArea == HandCtrl.AibuColliderKind.kokan)
            {
                waitTime = Random.Range(0.5f, 0.7f);
                click = HFlag.ClickKind.kokan;
            }
            else if (activeArea == HandCtrl.AibuColliderKind.anal)
            {
                if (itemObj == 2)
                    waitTime = 0.55f;
                else
                    waitTime = 0.25f;
                click = HFlag.ClickKind.anal;
            }
            else
            {
                if (itemObj == 2)
                    waitTime = Random.Range(0.25f, 1f);
                else
                    waitTime = 0.25f;
                click = (HFlag.ClickKind)(14 + activeArea);
            }
            _hFlag.click = click;
            for (var i = 0; i < 3; i++)
            {
                if (FakePostfix[i] != null)
                {
                    var suggestedTimer = Time.time + waitTime;
                    if (_postfixTimers[i] < suggestedTimer)
                        _postfixTimers[i] = suggestedTimer;
                }
            }
            _girlControllers[0].SquirtHandler();
            return waitTime;
        }
        internal void OnPositionChange(HSceneProc.AnimationListInfo nextAnimInfo = null)
        {
            var mode = nextAnimInfo == null ? _hFlag.mode : nextAnimInfo.mode;
            switch (mode)
            {
                case HFlag.EMode.aibu:
                    _aibu = true;
                    _houshi = false;
                    _sonyu = false;
                    break;
                case HFlag.EMode.houshi:
                case HFlag.EMode.houshi3P:
                case HFlag.EMode.houshi3PMMF:
                    _aibu = false;
                    _houshi = true;
                    _sonyu = false;
                    break;
                case HFlag.EMode.sonyu:
                case HFlag.EMode.sonyu3P:
                case HFlag.EMode.sonyu3PMMF:
                    _aibu = false;
                    _houshi = false;
                    _sonyu = true;
                    break;
                default:
                    _aibu = false;
                    _houshi = false;
                    _sonyu = false;
                    break;
            }
            if (nextAnimInfo.mode == HFlag.EMode.aibu)
                _aibu = true;
            else
                _aibu = false;
        }
        private void UpdateDevices()
        {
            _device = SteamVR_Controller.Input((int)VR.Mode.Right.Tracking.index);
            _device1 = SteamVR_Controller.Input((int)VR.Mode.Left.Tracking.index);
        }

        /// <summary>
        /// Purely VR feature, allows us to actually kiss.. stock of MainGameVR / KK_VR is a joke.
        /// </summary>
        /// <returns></returns>
        private IEnumerator KissCo()
        {
            SensibleH.Logger.LogDebug($"KissCo[Start]");
            yield return new WaitForEndOfFrame();
            _kissCo = true;
            _activePatches.Add(Harmony.CreateAndPatchAll(typeof(PatchHandCtrlKiss)));
            _activePatches.Add(Harmony.CreateAndPatchAll(typeof(PatchSteamVR)));
            var origin = VR.Camera.Origin;
            var head = VR.Camera.Head;
            //var neck = GetKissStartPosition();

            _girlControllers[0]._eyeNeckController.OnKissVrStart();
            Kiss.Instance.Cyu();

            // In light of recent rework, whole "FindRoll" function is busted.
            var rollDelta = FindRollDelta();
            if (Math.Abs(rollDelta) < 5f)
            {
                //var signedAngle = SignedAngle(head.position - _eyes.position, _eyes.forward, _eyes.up);
                var signedAngle = SignedAngle(head.position - _shoulders.position, _shoulders.forward, _shoulders.up);
                SensibleH.Logger.LogDebug($"KissCo[signedAngle] = {signedAngle}]");
                if (Math.Abs(signedAngle) < 10f)
                {
                    rollDelta = 25f * (Random.value > 0.5f ? 1 : -1);
                    if (_sonyu)
                        rollDelta *= Random.value * 2f;

                    SensibleH.Logger.LogDebug($"KissCo[RandomRoll] Everything else is too small to consider it {rollDelta}");
                }
                else
                    rollDelta = signedAngle;
            }
            var angleModRight = rollDelta * 0.0111f;// 90f;
            var absModRight = Mathf.Abs(angleModRight);
            var angleModUp = 1f - absModRight;
            if (absModRight > 1f)
                angleModRight = absModRight - (angleModRight - absModRight);


            var offsetRight = angleModRight * 0.0667f; // 15f; // 25f
            var offsetForward = 0.09f;
            var offsetUp = -0.04f - (Math.Abs(offsetRight) * 0.5f);
            var startDistance = Vector3.Distance(_eyes.position, head.position) - offsetForward;
            //var steps = 0f;
            var timer = Time.time + 3f;

            // Placeholder.
            // Change this one to something more interesting.
            MoMiController.FakeDragLength = Vector2.one * 0.5f;

            //var newPosUpMod = _shoulders.up * 0.1f;
            //var newXZpos = new Vector3(head.position.x, _shoulders.position.y + newPosUpMod.y, head.position.z);
            //var newZYpos = new Vector3(_shoulders.position.x + newPosUpMod.x, head.position.y, head.position.z);

            //var yaw = SignedAngle(newXZpos - (_shoulders.position + newPosUpMod), _shoulders.forward + newPosUpMod, _shoulders.up + newPosUpMod);
            //var pitch = SignedAngle(newZYpos - (_shoulders.position + newPosUpMod), _shoulders.forward + newPosUpMod, _shoulders.right + newPosUpMod);
            //var posDelta = head.position - _shoulders.position;
            //var angleDeltaUp = Vector3.Angle(posDelta, _shoulders.up);
            //var signedDeltaForward = SignedAngle(posDelta, _shoulders.forward, _shoulders.up);
            //while (timer > Time.time)
            //{
            //    // God knows what method.
            //    var adjustedEyes = _eyes.position + (_eyes.up * offsetUp) + (_eyes.right * offsetRight);
            //    Vector3 moveTowards;
            //    var fpsDelta = GetFpsDelta;
            //    var angle = Vector3.Angle(VR.Camera.Head.position - adjustedEyes, _eyes.forward);
            //    //var curDist = (adjustedEyes - head.position).magnitude;
            //    if (angle < 30f)
            //    {
            //        SensibleH.Logger.LogDebug($"KissCo[MoveTo] LowAngle {angle}");
            //        //We move directly to the ~lips.
            //        moveTowards = Vector3.MoveTowards(VR.Camera.Head.position, adjustedEyes + _eyes.forward * offsetForward, 0.0025f * fpsDelta);
            //        steps += 0.00125f * fpsDelta;
            //    }
            //    else
            //    {
            //        SensibleH.Logger.LogDebug($"KissCo[MoveTo] HighAngle {angle}");
            //        //We move to the Forward Vector of girl's face.
            //        //moveTowards = adjustedEyes + (_eyes.forward * (offsetForward + Mathf.Clamp01(startDist - steps)));
            //        moveTowards = Vector3.MoveTowards(VR.Camera.Head.position, adjustedEyes + _eyes.forward * (offsetForward + Mathf.Clamp01(startDist - steps)), 0.0025f * fpsDelta);
            //    }
            //    //if (moveTowards.y < adjustedEyes.y)
            //    //    moveTowards.y = adjustedEyes.y;

            //    var lookRotation = Quaternion.LookRotation(_eyes.position + (_eyes.right * offsetRight) - moveTowards, (_eyes.up * angleModUp) + (_eyes.right * angleModRight));
            //    origin.rotation = Quaternion.RotateTowards(origin.rotation, lookRotation, fpsDelta);
            //    origin.position += moveTowards - head.position;
            //    yield return new WaitForEndOfFrame();
            //}
            //var inPosition = false;
            var oldEyePos = _eyes.position;
            //while (true)//(timer > Time.time)
            //{
            //    // Glue Method.
            //    // Voice interrupt will be in Cyu.
            //    var adjustedEyes = _eyes.position + (_eyes.up * offsetUp) + (_eyes.right * offsetRight);
            //    var targetPos = adjustedEyes + _eyes.forward * (offsetForward + Mathf.Clamp01(startDistance - steps));
            //    Vector3 moveTowards;
            //    var deltaEyesPos = _eyes.position - oldEyePos;
            //    //camera.MoveFixMoveCam(deltaEyesPos);
            //    oldEyePos = _eyes.position;
            //    if (!inPosition)
            //    {
            //        moveTowards = Vector3.MoveTowards(head.position, deltaEyesPos + targetPos, Time.deltaTime * 0.13f);
            //        steps += Time.deltaTime * 0.13f;
            //        if (Vector3.Distance(moveTowards, targetPos) < 0.005f)
            //            inPosition = true;
            //    }
            //    else
            //    {
            //        moveTowards = targetPos;
            //        steps += Time.deltaTime * step;
            //        if (!IsTouch && (steps > startDistance || timer < Time.time))
            //        {
            //            break;
            //        }
            //    }

            //    var lookRotation = Quaternion.LookRotation(_eyes.position + (_eyes.right * offsetRight) - moveTowards, (_eyes.up * angleModUp) + (_eyes.right * angleModRight)); // + _eyes.forward * -0.1f);
            //    origin.rotation = Quaternion.RotateTowards(origin.rotation, lookRotation, Time.deltaTime * 90f);
            //    origin.position += moveTowards - head.position;
            //    yield return new WaitForEndOfFrame();
            //}
            while (timer > Time.time)
            {
                // Simple MoveTowards + added head movement.
                // With newest neck looks very good with added (due to girls' sway) eye's deltaPos.
                // Requires more testing.
                var adjustedEyes = _eyes.position + (_eyes.up * offsetUp) + (_eyes.right * offsetRight);
                var targetPos = adjustedEyes + _eyes.forward * offsetForward;

                var deltaEyesPos = _eyes.position - oldEyePos;
                oldEyePos = _eyes.position;
                //camera.MoveFixMoveCam(deltaEyesPos);
                var moveTowards = Vector3.MoveTowards(head.position, targetPos, Time.deltaTime * 0.07f);
                var lookRotation = Quaternion.LookRotation(_eyes.position + (_eyes.right * offsetRight) - moveTowards, (_eyes.up * angleModUp) + (_eyes.right * angleModRight)); // + _eyes.forward * -0.1f);
                origin.rotation = Quaternion.RotateTowards(origin.rotation, lookRotation, Time.deltaTime * 90f);
                origin.position += moveTowards + deltaEyesPos - head.position;
                yield return new WaitForEndOfFrame();
            }
            //SensibleH.Logger.LogDebug($"KissCo[UnPatch]");
            var lastElement = _activePatches.Count - 1;
            _activePatches[lastElement].UnpatchSelf();
            _activePatches.RemoveAt(lastElement);
            UpdateDevices();
            //if (!_moMiCo)
            //{
            //    // Pretty sure it's online for a while at this point.
            //    _moMiCo = true;
            //    _activeCoroutines.Add(StartCoroutine(MoMiCo()));
            //}
            while (true)
            {
                if (_device.GetPress(ButtonMask.Grip) || _device1.GetPress(ButtonMask.Grip))
                {
                    if (Vector3.Distance(_eyes.position, head.position) > 0.25f)
                    {
                        Halt();
                        yield break;
                    }
                }
                else if (_device.GetPressUp(ButtonMask.Trigger) || _device1.GetPressUp(ButtonMask.Trigger))
                {
                    Halt();
                    yield break;
                }
                else
                {
                    var deltaEyesPos = _eyes.position - oldEyePos;
                    oldEyePos = _eyes.position;
                    var targetPos = _eyes.position + (_eyes.right * offsetRight) + (_eyes.forward * offsetForward) + (_eyes.up * offsetUp);
                    var moveTowards = Vector3.MoveTowards(head.position, targetPos, Time.deltaTime * 0.05f);
                    var lookRotation = Quaternion.LookRotation(_eyes.position + (_eyes.right * offsetRight) - moveTowards, (_eyes.up * angleModUp) + (_eyes.right * angleModRight)); // + _eyes.forward * -0.1f);
                    origin.rotation = Quaternion.RotateTowards(origin.rotation, lookRotation, Time.deltaTime * 15f);
                    origin.position += moveTowards + deltaEyesPos - head.position;
                }
                yield return new WaitForEndOfFrame();
            }
        }
        /// <summary>
        /// Properly disengages the player from VR actions. Leaves the player not familiar with "Grip Move" hanging, that is being in weird X-axis rotation.
        /// </summary>
        internal IEnumerator EndKissCo()
        {
            SensibleH.Logger.LogDebug($"EndKissCo[Start]");
            _endKissCo = true;
            var origin = VR.Camera.Origin;
            var head = VR.Camera.Head;
            var pov = POV.Instance != null && POV.Active;
            UpdateDevices();
            if (_device.GetPress(ButtonMask.Grip) || _device1.GetPress(ButtonMask.Grip))
            {
                yield return new WaitUntil(() => !_device.GetPress(ButtonMask.Grip) && !_device1.GetPress(ButtonMask.Grip));
                yield return new WaitForEndOfFrame();
            }
            if (Vector3.Distance(_eyes.position, head.position) < 0.25f)
            {
                // Get away first if we are too close. Different for active pov.
                //SensibleH.Logger.LogDebug($"EndKissCo[MoveCameraAway][pov = {pov}]");
                var step = Time.deltaTime * 0.12f; //0.0034f * delta;
                if (pov && _maleEyes != null)
                {
                    //SensibleH.Logger.LogDebug($"EndKissCo[PoV]");
                    var upVec = _maleEyes.position.y - _eyes.position.y > 0.3f ? (Vector3.up * (step * 3f)) : Vector3.zero;
                    while (_handCtrl.isKiss || _handCtrl.actionUseItem != -1) // _handCtrl.isKiss
                    {
                        var newPos = head.position + (head.forward * -step) + upVec;
                        origin.rotation = Quaternion.RotateTowards(origin.rotation, Quaternion.Euler(origin.eulerAngles.x, origin.eulerAngles.y, 0f), GetFpsDelta);
                        origin.position += newPos - head.position;
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    while (Vector3.Distance(_eyes.position, head.position) < 0.3f)
                    {
                        var newPos = head.position + (head.forward * -step);
                        origin.rotation = Quaternion.RotateTowards(origin.rotation, Quaternion.Euler(origin.eulerAngles.x, origin.eulerAngles.y, 0f), GetFpsDelta);
                        origin.position += newPos - head.position;
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
            if (!pov)
            {
                //var newYZpos = new Vector3(_shoulders.position.x, head.position.y, head.position.z);
                //var angleDelta = Vector3.Angle(newYZpos - _shoulders.position, _shoulders.forward);
                if (true)  //(Math.Abs(Mathf.DeltaAngle(origin.eulerAngles.x, 0f)) < 50f)
                {
                    while (true)
                    {
                        if (!_device.GetPress(ButtonMask.Grip) || !_device1.GetPress(ButtonMask.Grip))
                        {
                            var oldHeadPos = head.position;
                            var lookAt = Quaternion.LookRotation(_eyes.position - head.position);
                            origin.rotation = Quaternion.RotateTowards(origin.rotation, Quaternion.Euler(lookAt.eulerAngles.x, origin.eulerAngles.y, 0f), GetFpsDelta);
                            origin.position += oldHeadPos - head.position;
                            if ((int)origin.eulerAngles.z == 0 && (int)origin.eulerAngles.x == (int)lookAt.eulerAngles.x)
                                break;
                        }
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    while ((int)origin.eulerAngles.z != 0)
                    {
                        if (!_device.GetPress(ButtonMask.Grip) || !_device1.GetPress(ButtonMask.Grip))
                        {
                            var oldHeadPos = head.position;
                            origin.rotation = Quaternion.RotateTowards(origin.rotation, Quaternion.Euler(origin.eulerAngles.x, origin.eulerAngles.y, 0f), GetFpsDelta);
                            origin.position += oldHeadPos - head.position;
                        }
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
            _endKissCo = false;
            _handCtrl.DetachAllItem();
            _hFlag.click = HFlag.ClickKind.de_muneL;
            SensibleH.Logger.LogDebug($"EndKissCo[End]");
        }
        public static void StartVrAction(HandCtrl.AibuColliderKind colliderKind)
        {
            if (_endKissCo)
            {
                SensibleH.Logger.LogDebug($"StartVrAction[EndKissCo]");
                Instance.Halt(disengage: false);
                _endKissCo = false;
            }
            if (!_kissCo)
            {
                if (colliderKind == HandCtrl.AibuColliderKind.mouth)
                {
                    if (_moMiCo || _lickCo)
                    {
                        Instance.Halt(disengage: false);
                    }
                    SensibleH.Logger.LogDebug($"StartVrAction[KissCo]");
                    Instance._activeCoroutines.Add(Instance.StartCoroutine(Instance.KissCo()));

                }
                else if (!_lickCo)
                {
                    SensibleH.Logger.LogDebug($"StartVrAction[LickCo]");
                    if (_moMiCo)//if (_kissCo)
                    {
                        Instance.Halt(disengage: false);
                    }
                    if (_hFlag.mode == HFlag.EMode.aibu)
                        _hFlag.click = (HFlag.ClickKind)colliderKind; //(_handCtrl.selectKindTouch + 14);
                    Instance._activeCoroutines.Add(Instance.StartCoroutine(Instance.LickCo(colliderKind)));
                }
            }
        }

        /// <summary>
        /// Purely VR feature, helps immensely with Licking of MainGameVR.
        /// TODO Centering of camera in sonyu, so it looks more plausible.
        /// </summary>
        private IEnumerator LickCo(HandCtrl.AibuColliderKind colliderKind)
        {
            SensibleH.Logger.LogDebug($"LickCo[Start]");
            _lickCo = true;
            _activeCoroutines.Add(StartCoroutine(AttachCo(colliderKind)));
            yield return CaressUtil.ClickCo();
            yield return new WaitUntil(() => !IsTouch);
            //yield return new WaitUntil(() => _handCtrl.actionUseItem == -1);

            _mousePressDown = true;
            HandCtrlHooks.InjectMouseButtonDown(0);

            // Is this really necessary ?
            MoMiActive = true;

            yield return new WaitUntil(() => GameCursor.isLock);

            // Most likely somebody along the line wants this Wait badly.
            yield return new WaitForEndOfFrame();
            if (!_moMiCo)
            {
                SensibleH.Logger.LogDebug($"LickCo[AddMoMiCo]");
                _moMiCo = true;
                _activeCoroutines.Add(StartCoroutine(MoMiCo(skipWait: true)));
            }
            JudgeProc(2, fakeIt: true);
        }
        /// <summary>
        /// Partner in crime of LickCo.
        /// </summary>
        private IEnumerator AttachCo(HandCtrl.AibuColliderKind colliderKind)
        {
            SensibleH.Logger.LogDebug($"AttachCo[Start]");
            var dic = PoI[colliderKind];
            var poi = _chaControl[0].objBodyBone.transform.Find(dic.path);

            var origin = VR.Camera.Origin;
            var head = VR.Camera.Head;
            var prevPoiPosition = poi.position;
            // We check parameter as the update doesn't run centralized check just yet.
            while (IsTouch || _handCtrl.useItems[2] == null)
            {
                // We move together with the point of interest during "Touch" animation
                origin.position += poi.position - prevPoiPosition;
                prevPoiPosition = poi.position;
                yield return new WaitForEndOfFrame();
            }
            var item = _handCtrl.useItems[2].obj.transform.Find("cf_j_tangroot");
            //SensibleH.Logger.LogDebug($"AttachCo[Start] {poi.rotation.eulerAngles.x}");
            if (poi.rotation.eulerAngles.x > 30f && poi.rotation.eulerAngles.x < 90f)
            {
                // Check if the girl is on all fours.
                dic = PoI[HandCtrl.AibuColliderKind.none];
            }
            UpdateDevices();
            while (true)
            {
                if (_handCtrl.useItems[2] == null || _device.GetPressDown(ButtonMask.Trigger) || _device1.GetPressDown(ButtonMask.Trigger))
                {
                    SensibleH.Logger.LogDebug($"AttachCo[PrematureEnd] no transform/triggers");
                    Halt();
                }
                //SensibleH.Logger.LogDebug($"AttachCo[MoveToItem]");
                var adjustedItem = item.position + item.forward * dic.itemOffsetForward + item.up * dic.itemOffsetUp;
                var moveTo = Vector3.MoveTowards(head.position, adjustedItem, Time.deltaTime * 0.2f);
                var lookAt = Quaternion.LookRotation(poi.position + poi.up * dic.poiOffsetUp - moveTo, poi.up * dic.directionUp + poi.forward * dic.directionForward);
                origin.rotation = Quaternion.RotateTowards(origin.rotation, lookAt, Time.deltaTime * 60f);
                origin.position += moveTo - head.position;
                if (Vector3.Distance(adjustedItem, head.position) < 0.002f)
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
            while (true)
            {
                if (_device.GetPressDown(ButtonMask.Trigger) || _device1.GetPressDown(ButtonMask.Trigger)) //_handCtrl.useItems[2] == null || 
                {
                    break;
                }
                else if (_device.GetPress(ButtonMask.Grip) || _device1.GetPress(ButtonMask.Grip))
                {
                    if (Vector3.Distance(poi.position, head.position) > 0.15f)
                        break;
                }
                else
                {
                    var targetPos = item.position + (item.forward * dic.itemOffsetForward) + (item.up * dic.itemOffsetUp);
                    var moveTo = Vector3.MoveTowards(head.position, targetPos, Time.deltaTime * 0.05f);
                    var lookAt = Quaternion.LookRotation(poi.position + poi.up * dic.poiOffsetUp - moveTo, poi.up * dic.directionUp + poi.forward * dic.directionForward);
                    origin.rotation = Quaternion.RotateTowards(origin.rotation, lookAt, Time.deltaTime * 15f);
                    origin.position += moveTo - head.position;
                }
                yield return new WaitForEndOfFrame();
            }
            Halt();
            SensibleH.Logger.LogDebug($"AttachCo[End]");
        }
        /*
         * cf_j_tangroot.transform.
         *     forward+ (All subsequent measurements are done relative to the girl)
         *         boobs - vec.up
         *         ass - vec.down
         *         vag - vec.up
         *         anal - vec.forward 
         *     up+ (All subsequent measurements are done relative to the girl)
         *         boobs - vec.forward
         *         ass - vec.backward
         *         vag - vec.forward
         *         anal - vec.down
         */
        private Dictionary<HandCtrl.AibuColliderKind, LickItem> PoI = new Dictionary<HandCtrl.AibuColliderKind, LickItem>()
        {
            // There are inconsistencies depending on the pose. Not fixed: ass, anal.
            {
                HandCtrl.AibuColliderKind.muneL, new LickItem {
                path = "cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/cf_s_bust00_L/cf_d_bust01_L" +
                    "/cf_j_bust01_L/cf_d_bust02_L/cf_j_bust02_L/cf_d_bust03_L/cf_j_bust03_L/cf_s_bust03_L/k_f_mune03L_02",
                itemOffsetForward = 0.08f,
                itemOffsetUp = 0f,//-0.04f, 
                poiOffsetUp = 0.05f,
                directionUp = 1f,
                directionForward = 0f
                }
            },
            {
                HandCtrl.AibuColliderKind.muneR, new LickItem {
                path = "cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/cf_s_bust00_R/cf_d_bust01_R" +
                    "/cf_j_bust01_R/cf_d_bust02_R/cf_j_bust02_R/cf_d_bust03_R/cf_j_bust03_R/cf_s_bust03_R/k_f_mune03R_02",
                itemOffsetForward = 0.08f,
                itemOffsetUp = 0f,
                poiOffsetUp = 0.05f,
                directionUp = 1f,
                directionForward = 0f
                }
            },
            {
                HandCtrl.AibuColliderKind.kokan, new LickItem {
                path = "cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_s_waist02/k_f_kosi02_02",
                itemOffsetForward = 0.06f,
                itemOffsetUp = 0.03f,
                poiOffsetUp = 0f,
                directionUp = 0.5f,
                directionForward = 0.5f
                }
            },
            {
                HandCtrl.AibuColliderKind.anal, new LickItem {
                path = "cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_s_waist02/k_f_kosi02_02",
                itemOffsetForward = -0.05f,//-0.06f, 
                itemOffsetUp = -0.08f, // -0.06f
                poiOffsetUp = 0f,
                directionUp = 1f,
                directionForward = 0f
                }
            },
            {
                HandCtrl.AibuColliderKind.siriL, new LickItem {
                path = "cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/aibu_hit_siri_L",
                itemOffsetForward = -0.04f, // -0.06f
                itemOffsetUp = 0.04f,
                poiOffsetUp = 0.2f,
                directionUp = 1f,
                directionForward = 0f
                }
            },
            {
                HandCtrl.AibuColliderKind.siriR, new LickItem {
                path = "cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/aibu_hit_siri_R",
                itemOffsetForward = -0.04f, // -0.06f
                itemOffsetUp = 0.04f,
                poiOffsetUp = 0.2f,
                directionUp = 1f,
                directionForward = 0f
                }
            },
            {
                HandCtrl.AibuColliderKind.none, new LickItem {
                path = "cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_s_waist02/k_f_kosi02_02",
                itemOffsetForward = -0.07f, // -0.01
                itemOffsetUp = -0.01f,
                poiOffsetUp = 0f,
                directionUp = 0f,
                directionForward = -1f
                }
            },


        };
        private float FindRollDelta()
        {
            var headsetRoll = Mathf.DeltaAngle(VR.Camera.Head.eulerAngles.z, 0f);
            var headRoll = Mathf.DeltaAngle(_neck.localRotation.eulerAngles.z, 0f) + Mathf.DeltaAngle(_head.localRotation.eulerAngles.z, 0f);

            return Mathf.DeltaAngle(headsetRoll, headRoll);
        }
        private float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            // This one brings little to no benefit with current neck states of the kiss.
            // After recent rework became a local detractor.
            float unsignedAngle = Vector3.Angle(from, to);

            float cross_x = from.y * to.z - from.z * to.y;
            float cross_y = from.z * to.x - from.x * to.z;
            float cross_z = from.x * to.y - from.y * to.x;
            float sign = Mathf.Sign(axis.x * cross_x + axis.y * cross_y + axis.z * cross_z);
            return unsignedAngle * sign;
        }
    }
}
