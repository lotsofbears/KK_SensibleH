using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using static KK_SensibleH.SensibleH;
using System.Collections;
using System.Linq;
using System;
using Illusion.Game;
using KK_SensibleH.Patches.DynamicPatches;
using VRGIN.Controls;
using KK_SensibleH.Caress;
using static KK_SensibleH.AutoMode.LoopProperties;
using Illusion.Extensions;
using System.Runtime.InteropServices;
using KK_SensibleH.EyeNeckControl;
using KKAPI;
using KKAPI.MainGame;
using ActionGame.Place;
using KKAPI.Utilities;

namespace KK_SensibleH.Caress
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
            public float speed;
            public float intensity;
            public float loopEndTime;
            public int peak;
            public int range;
            public bool hasPair;
            public bool leadsPair;
            public bool inPair;
            public bool startPair;
        }
        //class LickItem
        //{
        //    public string path;
        //    public float itemOffsetForward;
        //    public float itemOffsetUp;
        //    public float poiOffsetUp;
        //    public float directionUp;
        //    public float directionForward;
        //}
        public static string[] FakePrefix = new string[3];
        public static string[] FakePostfix = new string[3];
        public static bool ResetDrag { get; set; }
        public static bool FakeDrag { get; set; }
        public static Vector2 FakeDragLength { get; set; }
        public static bool FakeMouseButton { get; set; }
        public static MoMiController Instance;

        private bool _kissCo;
        internal bool _lickCo;
        private bool _moMiCo;

        private float[] _postfixTimers = new float[3];
        private bool _judgeCooldown;
        private bool _touchAnim;
        private float _inactiveTimestamp;
        private float _wait;
        private float _itemCountMultiplier;
        private float _itemCountTiny;
        private bool _drag;
        private Vector3 _targetScale;
        private Transform _pubicHair;
        //private Transform _eyes;
        //private Transform _head;
        //private Transform _neck;
        //private Transform _maleEyes;
        //private Transform _shoulders;


        private List<MoMiCircles> _circles = [];
        internal Coroutine[] _activeItems = new Coroutine[6];
        private Dictionary<int, ActiveItem> _items = [];
        internal bool IsTouchCrossFade => _touchAnim;
        private bool IsCrossFadeOver => _wait < Time.time;
        //private float GetFpsDelta => Time.deltaTime * 60f;
        private bool IsKiss => _handCtrl.IsKissAction(); // _kissCo ||
        private void Awake()
        {
            Instance = this;
            for (var i = 0; i < 3; i++)
            {
                _circles.Add(new MoMiCircles());
            }
            if (SensibleHController.IsVR)
            {
                this.gameObject.AddComponent<Kiss>();
            }
            if (SensibleH.HoldPubicHair.Value)
            {
                // Due to InsertAnimation acc pubic hair behaves.. wildly.
                // We help it to keep atleast some cohesion.
                var acc = lstFemale[0].objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cf_j_kokan/a_n_kokan")
                    .GetComponentsInChildren<Transform>(includeInactive: true)
                    .Where(t => t.name.StartsWith("ca_slot", StringComparison.Ordinal))
                    .FirstOrDefault();
                if (acc != null)
                {
                    _pubicHair = acc.Children().FirstOrDefault();
                    _targetScale = _pubicHair.transform.lossyScale;
                }
            }
            for (var i = 0; i < 3; i++)
            {
                FakePrefix[i] = null;
                FakePostfix[i] = null;
            }
        }

        private void OnDestroy()
        {
            Halt();
        }
        private void StartMoMi()
        {
            StartCoroutine(MoMiCo());
        }
        /// <summary>
        /// Hook for MainGameVR.
        /// </summary>
        public static void OnLickStart(HandCtrl.AibuColliderKind colliderKind)
        {
            if (Instance != null)
            {
                // First call before mouse clicks to stop our side if we are active.
                // Second to do.. stuff.
                if (colliderKind == HandCtrl.AibuColliderKind.none)
                {
                    if (Instance._moMiCo) Instance.Halt();
                }
                else
                {
                    Instance._lickCo = true;
                    Instance.JudgeProc(2, fakeIt: true);
                    Kiss.Instance.Cyu(colliderKind);
                }
            }
        }
        /// <summary>
        /// Hook for MainGameVR.
        /// </summary>
        public static void OnKissStart(HandCtrl.AibuColliderKind colliderKind)
        {
            if (Instance != null)
            {
                if (colliderKind == HandCtrl.AibuColliderKind.none)
                {
                    if (Instance._moMiCo) Instance.Halt();
                    headManipulators[0]._neckController.OnKissVrStart();
#if KK
                    // No patch helps to get the last bit of the lag in KK. Only this does.
                    IllusionFixes.ResourceUnloadOptimizations.DisableUnload.Value = true;
#endif
                }
                else
                {
                    Instance._kissCo = true;
                    Kiss.Instance.Cyu(HandCtrl.AibuColliderKind.mouth);
                }
            }
        }

        /// <summary>
        /// Hook for MainGameVR.
        /// </summary>
        public static void OnKissEnd()
        {
            if (Instance != null)
            {
                headManipulators[0]._neckController.OnKissVrEnd();
                Instance.Halt();
            }
        }

        /// <summary>
        /// Hook for MainGameVR.
        /// </summary>
        public static void ReleaseItem(HandCtrl.AibuColliderKind colliderKind)
        {
            if (Instance != null)
            {
                Instance.StopItemCo((int)colliderKind - 2);
            }
        }
        public static void MoMiJudgeProc(HandCtrl.AibuColliderKind colliderKind)
        {
            if (Instance != null)
            {
                Instance.JudgeProc(_handCtrl.useAreaItems[(int)colliderKind - 2].idUse);
            }
        }
        private void StopItemCo(int area)
        {
            if (_activeItems[area] != null)
            {
                StopCoroutine(_activeItems[area]);
            }
        }
        private void Halt()
        {
            StopAllCoroutines();
            _items.Clear();

            Cursor.visible = true;
            GameCursor.Instance.UnLockCursor();

            _moMiCo = false;
            _kissCo = false;
            _lickCo = false;
            MoMiActive = false;
            FakeDrag = false; 
            FakeMouseButton = false;
#if KK
            if (SensibleHController.IsVR)
            {
                IllusionFixes.ResourceUnloadOptimizations.DisableUnload.Value = false;
            }
#endif

        }
        private void Update()
        {
            if (_moMiCo)
            {
                _touchAnim = IsTouch && !IsCrossFadeOver;
                _drag = _handCtrl.ctrl == HandCtrl.Ctrl.drag;
                if (Input.GetMouseButtonDown(0) || (_handCtrl.actionUseItem == -1 && !_handCtrl.IsKissAction()))
                { 
                    Halt();
                }
                else if (_judgeCooldown)
                {
                    var count = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        // Fake conditions for animation restart of specific items.
                        if (_postfixTimers[i] < Time.time)
                        {
                            FakePostfix[i] = null;
                            count++;
                        }
                    }
                    if (count == 3)
                    {
                        _judgeCooldown = false;
                    }
                }
            }
            else if (GameCursor.isLock && (_handCtrl.actionUseItem != -1 || _handCtrl.IsKissAction()))
            {
                StartMoMi();
            }

            if (_pubicHair != null && _pubicHair.gameObject.activeSelf)
            {
                _pubicHair.localScale = Divide(Vector3.Scale(_targetScale, _pubicHair.localScale), _pubicHair.lossyScale);
            }
        }
        private Vector3 Divide(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        /// <summary>
        /// Initiates automatic movement of items, takes care of patching.
        /// </summary>
        private IEnumerator MoMiCo(bool skipWait = false)
        {
            _moMiCo = true;
            if (!skipWait)
            {
                // Update will be doing this from next frame for everybody.
                _touchAnim = IsTouch;

                yield return new WaitUntil(() => !_touchAnim);
                if (!SensibleHController.IsVR)
                {
                    yield return new WaitForSeconds(1f);
                }

                if (!IsKiss && _handCtrl.actionUseItem == -1)
                {
                    Halt();
                    yield break;
                }
            }
            _kissCo = IsKiss;
            MoMiActive = true;
            if (SensibleHController.IsVR)
            {
                if (!_lickCo && !_kissCo)
                {
                    FakeMouseButton = true;
                }
            }
            else
            {
                FakeMouseButton = true;
                Utils.Sound.Play(SystemSE.ok_l);
                if (UnityEngine.Input.GetMouseButton(0))
                {
                    yield return new WaitUntil(() => UnityEngine.Input.GetMouseButtonUp(0));
                }
                else
                {
                    //SensibleH.Logger.LogDebug($"MoMiCo:MouseButton:EarlyRelease");
                    _moMiCo = false;
                    yield break;
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
            
            var count = _items.Count;
            FakeDrag = !(mode == HFlag.EMode.sonyu && hFlag.nowAnimStateName.EndsWith("Loop", StringComparison.Ordinal));
            if (count != 0)
            {
                ResetDrag = true;
                FakeDragLength = Vector2.zero;

                _itemCountMultiplier = 1.5f / count;// + ((1 - count) * 0.1f);
                _itemCountTiny = _itemCountMultiplier * 0.1f;
                foreach (var item in _items)
                {
                    if (item.Value.area == 0 || item.Value.area == 4)
                    {
                        var otherItem = _items.Values
                            .Where(i => i.area - 1 == item.Value.area)
                            .FirstOrDefault();
                        if (otherItem != null)
                        {
                            // //SensibleH.Logger.LogDebug($"Found the pair[{item.Value.area}][{otherItem.area}] in _items");
                            item.Value.hasPair = true;
                            otherItem.hasPair = true;
                        }
                    }
                    _activeItems[item.Value.area] = StartCoroutine(ItemCo(_kissCo, item.Key));
                }
            }
            else
            {
                ResetDrag = false;
                FakeDragLength = Vector2.one;
            }
        }
        /// <summary>
        /// Moves attached items.
        /// </summary>
        private IEnumerator ItemCo(bool kiss, int itemId)
        {
            var judgeProc = !kiss;
            var item = _items[itemId];
            var midPos = new Vector2(0.5f, 0.5f);
            while (true)
            {
                // Variable judgeProc mainly keeps track of our current position (middle or anywhere but);
                if (item.pattern == -1)
                {
                    judgeProc = true;
                }

                if (item.startPair || Random.value < 0.5f)
                {
                    // //SensibleH.Logger.LogDebug($"Item:{itemId}[AttemptToJudge]");
                    if (!judgeProc)// && IsAdjustmentNeeded(itemId))
                    {
                        // //SensibleH.Logger.LogDebug($"Item:{itemId}[MoveToCenter]");
                        // Move item to the center.
                        var currentPos = _circles[itemId].GetPosition(item.pattern, item.deg, item.step, item.intensity, item.peak, item.range, out item.deg);
                        var deltaPos = midPos - currentPos;
                        var step = 0.6667f * Time.deltaTime;
                        var allSteps = deltaPos.magnitude / step;
                        var stepVec = deltaPos / allSteps;

                        while (allSteps-- > 1)
                        {
                            if (!_touchAnim)
                            {
                                // There are cases when the game "helps" us with the wrong vector.
                                hFlag.xy[item.area] = currentPos += stepVec;
                            }
                            yield return CoroutineUtils.WaitForEndOfFrame;
                        }
                        //yield return new WaitForSeconds(0.1f);
                    }

                    // If we want our judge procs after CaressAreaReaction() at aesthetic timings, there has to be atleast 2 of them.
                    // Otherwise we'll get a bad state and premature Halt().
                    // Proper wait after judgeProc is within the range 0.55f - 0.60f, 0.55f looks well and doesn't fall off all that often.
                    judgeProc = true;
                    //yield return CoroutineUtils.WaitForEndOfFrame;
                    if (JudgeProc(itemId))
                    {
                        var wait = 0f;
                        yield return new WaitForSeconds(0.4f);
                        if (Random.value < 0.4f)
                        {
                            headManipulators[0].Reaction();
                            //AddToDragLengthBoost(2f);

                            if (Random.value < 0.5f)
                            {
                                wait = CaressAreaReaction(itemId);
                                // TODO proper handle for special cameras, etc.
                                headManipulators[0]._neckController.LookAway();
                            }
                            else
                            {
                                headManipulators[0]._neckController.LookAtPoI();
                                yield return new WaitForSeconds(0.15f);
                            }
                        }
                        else
                        {
                            yield return new WaitForSeconds(0.15f);
                        }
                        var num = 0.5f;
                        while (Random.value < num || wait != 0f)
                        {
                            if (wait != 0f)
                            {
                                yield return new WaitForSeconds(wait);
                                wait = 0f;
                                //yield return CoroutineUtils.WaitForEndOfFrame;
                                if (JudgeProc(itemId))
                                {
                                    yield return new WaitForSeconds(0.55f);
                                }
                            }
                            //yield return CoroutineUtils.WaitForEndOfFrame;
                            JudgeProc(itemId);
                            if (Random.value < num * 0.5f)
                            {
                                yield return new WaitForSeconds(0.4f);
                                headManipulators[0].Reaction();
                                yield return new WaitForSeconds(0.15f);
                            }
                            else
                                yield return new WaitForSeconds(0.55f);
                            num -= 0.1f;
                        }
                        if (wait != 0f)
                        {
                            // No clue why 0.9f but it just werks.
                            yield return new WaitForSeconds(0.9f); // 0.9f
                        }
                        if (_touchAnim)
                        {
                            //var timestamp = Time.time;
                            yield return new WaitUntil(() => !_touchAnim);
                            // //SensibleH.Logger.LogDebug($"Item:{itemId}[ExtraWaitAfterCaressReaction:{Time.time - timestamp}]");
                        }
                        yield return CoroutineUtils.WaitForEndOfFrame;
                    }
                }

                OrganizeDictionary(itemId, judgeProc);

                if (judgeProc && item.pattern != -1) // IsAdjustmentNeeded(itemId) && 
                {
                    // //SensibleH.Logger.LogDebug($"Item:{itemId}[MoveToPos]");
                    // Move item from center to its initial position.
                    var targetPos = _circles[itemId].GetPosition(item.pattern, item.deg, item.step, item.intensity, item.peak, item.range, out item.deg);
                    var currentPos = midPos;
                    var deltaPos = targetPos - currentPos;
                    var step = 0.6667f * Time.deltaTime;
                    var allSteps = deltaPos.magnitude / step;
                    var stepVec = deltaPos / allSteps;

                    while (allSteps-- > 1)
                    {
                        if (!_touchAnim)
                        {
                            hFlag.xy[item.area] = currentPos += stepVec;
                        }
                        yield return CoroutineUtils.WaitForEndOfFrame;
                    }
                    judgeProc = false;
                }
                judgeProc = item.pattern == -1;
                while (item.loopEndTime > Time.time)
                {
                    if (judgeProc)
                    {
                        if (JudgeProc(itemId))
                        {
                            yield return new WaitForSeconds(0.55f);
                        }
                        else
                        {
                            // //SensibleH.Logger.LogDebug($"Item:{itemId}[CantDoJudgeLoop]");
                            yield return CoroutineUtils.WaitForEndOfFrame;
                            break;
                        }
                    }
                    else
                    {
                        if (!_touchAnim)
                        {
                            var vec = _circles[itemId].GetPosition(item.pattern, item.deg, item.step, item.intensity, item.peak, item.range, out item.deg);
                            hFlag.xy[item.area] = vec;
                            if (_drag)
                            {
                                AddToDragLength((_itemCountMultiplier + (item.speed + 2f - item.intensity) * _itemCountTiny) * Vector2.one); //  * (1f + hFlag.gaugeFemale * 0.002f) 
                            }
                        }
                        yield return CoroutineUtils.WaitForEndOfFrame;
                    }
                }
            }
        }
        //private float _boost;
        //private void AddToDragLengthBoost(float boost = 0f)
        //{
        //    if (boost != 0f)
        //    {
        //        _boost = boost;
        //    }
        //    if (_drag && _boost > 0f)
        //    {
        //        FakeDragLength += Vector2.one * _boost;
        //        _boost -= 0.05f;
        //    }
        //}
        private void AddToDragLength(Vector2 vector)
        {
            FakeDragLength += vector;
        }
        internal void SetCrossFadeWait(float time)
        {
            var crossFadePause = Time.time + time;
            if (_wait < crossFadePause)
            {
                _wait = crossFadePause;
            }
        }
        private void OrganizeDictionary(int idUse, bool midPos)
        {
            var item = _items[idUse];
            if (item.inPair && !item.leadsPair)
            {
                item.startPair = false;
                PairItems(idUse, midPos);
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
                // //SensibleH.Logger.LogDebug($"MomiItem[{idUse}]OrganizeDic:SetPair midPos[{midPos}] ptn[{item.pattern}]");
            }
            else if (!item.inPair || item.leadsPair)
            {
                var femGauge = hFlag.gaugeFemale;
                if (midPos)
                {
                    // Smaller value -> broader movements.
                    item.intensity = (int)(10f * Random.Range(1f, 3f - femGauge * 0.01f)) * 0.1f;//  (float)Math.Round(Random.Range(1f, 3f), 1);
                    item.deg = Random.Range(0, 360);
                }

                item.peak = Random.Range(0, 360);
                item.range = Random.Range(45, 120);
                item.pattern = PickPattern(item.area, item.obj);
                item.loopEndTime = Time.time + Random.Range(5f, 10f);
                item.startPair = false;

                var timeDelta = Time.deltaTime;
                if (timeDelta > 0.05f)
                {
                    // Very rarely we can catch a phantom stutter.
                    timeDelta = 0.0167f;
                }
                if (_lickCo && idUse == 2)
                {
                    var rand = Random.Range(1f + femGauge * 0.01f, 2f);
                    item.speed = rand;
                    item.step = (int)(10f * rand * (timeDelta * 60f)) * 0.1f;
                }
                else
                {
                    var rand = Random.Range(2f + femGauge * 0.02f, 5f);
                    item.speed = (rand - 3f);
                    item.step = (int)(10f * rand * (timeDelta * 60f)) * 0.1f;
                }
               
                // //SensibleH.Logger.LogDebug($"Item:{idUse}:[OrganizeDic]speed:{item.speed} int:{item.intensity}");
            }
        }
        private void PairItems(int idUse, bool midPos)
        {
            var item = _items[idUse];

            var leader = _items.Values
                .Where(i => i.hasPair && i.area != item.area)
                .FirstOrDefault();
            if (leader.pattern == -1)
            {
                // //SensibleH.Logger.LogDebug($"Item:{idUse}:PairItem[LeaderIsBad]");
                item.inPair = false;
                leader.inPair = false;
                leader.leadsPair = false;
                OrganizeDictionary(idUse, midPos);
                return;
            }

            if (midPos)
            {
                var link = LinkItems(leader.deg, leader.peak, out item.deg, out item.peak);
                item.intensity = leader.intensity;
                item.pattern = PickPattern(item.area, item.obj, link, leader.pattern);
                item.range = leader.range;
            }
            else
            {
                //item.deg = leader.deg;
                //item.peak = leader.peak;
                item.pattern = leader.pattern;
            }
            item.step = leader.step;
            item.loopEndTime = leader.loopEndTime + Time.deltaTime;

            if (Random.value < 0.25f)
            {
                item.inPair = false;
                leader.inPair = false;
                leader.leadsPair = false;
                // //SensibleH.Logger.LogDebug($"Item:{idUse}:Slave:PairItem[LastLoop]");
            }
            else
            {
                // //SensibleH.Logger.LogDebug($"ItemCo:{idUse}:Slave:PairItem");
            }
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
            // THE usual suspect if we broke the game.
            // It's way too much trouble to keep "JudgeProc()" during kiss, especially given that the player will hardly see/like it.
            if (!fakeIt && (IsKiss || (_judgeCooldown && FakePostfix[item] == null))) //(_kissCo || _lickCo || 
            {
                // //SensibleH.Logger.LogDebug($"JudgeProc[Attempt][{item}]");
                return false;
            }
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
                _judgeCooldown = true;
                // //SensibleH.Logger.LogDebug($"JudgeProc[Action][{item}][fakeIt:{fakeIt}]");
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

            //// //SensibleH.Logger.LogDebug($"PickPattern:Area[{area}] = [{result}]");
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
            if (IsTouch || hFlag.mode != HFlag.EMode.aibu)
            {
                return 0f;
            }

            HFlag.ClickKind click;
            float waitTime;

            var itemObj = _handCtrl.useItems[target].idObj;
            var activeArea = _handCtrl.useItems[target].kindTouch;
            // //SensibleH.Logger.LogDebug($"CaressAreaReaction area[{activeArea}], item[{itemObj}]");
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
            hFlag.click = click;
            for (var i = 0; i < 3; i++)
            {
                if (FakePostfix[i] != null)
                {
                    var suggestedTimer = Time.time + waitTime;
                    if (_postfixTimers[i] < suggestedTimer)
                    {
                        _postfixTimers[i] = suggestedTimer;
                    }
                }
            }
            headManipulators[0].SquirtHandler();

            // Test.
            SetCrossFadeWait(waitTime);
            //hFlag.SpeedUpClick(0.25f + Random.value * 0.25f, 1.5f);
            return waitTime;
        }
    }
}
