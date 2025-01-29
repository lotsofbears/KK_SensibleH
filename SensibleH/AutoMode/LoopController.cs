using KKAPI;
using KKAPI.MainGame;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using static KK_SensibleH.SensibleH;
using System.Collections;
using System.Linq;
using System;
using static KK_SensibleH.AutoMode.LoopProperties;
using UnityEngine.Assertions.Must;
using KK_SensibleH.Caress;
using KK_SensibleH.Patches.StaticPatches;
using static ChaCustom.CustomCheckWindow;
using static KK_SensibleH.Patches.StaticPatches.PatchLoop;

namespace KK_SensibleH.AutoMode
{
    /// <summary>.
    /// Recently Broken:
    /// edge voice is still a clutch
    /// </summary>
    public class LoopController : MonoBehaviour
    {
        private enum Loop
        {
            Random,
            Weak,
            Strong,
            Orgasm
        }

        private SensibleHController _master;
        public static LoopController Instance;
        private Coroutine _edgeRoutine;
        private HSceneProc.AnimationListInfo nextAnimation;
        private List<HActionBase> _lstProc;
        private List<HSceneProc.AnimationListInfo>[] lstUseAnimInfo;
        private GameObject fakeAnimButton;
        private int _actionTimer;
        private int _nextSpeedChange;
        private int _nextLoopChange;
        private int _actionPickTimer;
        private int _nextEdge;
        private int _climaxAt = Random.Range(75, 99);
        private bool _wasAnalPlay;
        private bool _edgeActive;
        private bool _busy;
        private bool _restart;
        private bool _hadClimax;
        private bool _finishLoop;
        private static HFlag.FinishKind _savedClick;

        internal void Initialize(MonoBehaviour _proc, SensibleHController master)
        {
            Instance = this;
            _master = master;
            var traverse = Traverse.Create(_proc);
            lstUseAnimInfo = traverse.Field("lstUseAnimInfo").GetValue<List<HSceneProc.AnimationListInfo>[]>();
            _lstProc = traverse.Field("lstProc").GetValue<List<HActionBase>>();
            _sprite = traverse.Field("sprite").GetValue<HSprite>();
            fakeAnimButton = Instantiate(_sprite.objMotionListNode, gameObject.transform, false);
            fakeAnimButton.AddComponent<HSprite.AnimationInfoComponent>();
            fakeAnimButton.SetActive(true);
            OnPositionChange();
            UpdateSettings();
            SetCeiling();
            var type = AccessTools.TypeByName("KK_MaleBreath.MaleBreathController");
            if (type != null)
            {
                PatchLoop.maleBreathDelegate = AccessTools.MethodDelegate<Func<int, bool>>(AccessTools.FirstMethod(type, m => m.Name.Equals("ButtonClick")));
            }
            SensibleH.ActionFrequency.SettingChanged += (_, _1) => UpdateSettings();
            SensibleH.GaugeSpeed.SettingChanged += (_, _1) => SensibleHController.UpdateSettings();
            SensibleH.ConfigBiasF.SettingChanged += (_, _1) => GetBias();
            SensibleH.ConfigBiasM.SettingChanged += (_, _1) => GetBias();
            SensibleH.ConfigMaleOrgCount.SettingChanged += (_, _1) => GetBias();
        }
        private void OnDestroy()
        {
            SensibleH.ActionFrequency.SettingChanged -= (_, _1) => UpdateSettings();
            SensibleH.GaugeSpeed.SettingChanged -= (_, _1) => SensibleHController.UpdateSettings();
            SensibleH.ConfigBiasF.SettingChanged -= (_, _1) => GetBias();
            SensibleH.ConfigBiasM.SettingChanged -= (_, _1) => GetBias();
            SensibleH.ConfigMaleOrgCount.SettingChanged -= (_, _1) => GetBias();
            Destroy(fakeAnimButton);
        }
        private Coroutine _speedChangerCo;
        private Coroutine runAfterCoroutine;
        private int GetNextTimer(float multiplier = 1f) => _actionTimer + (int)((5f + Random.value * 15f) * multiplier * ActionFrequency.Value);
        private float GetRandomRange(float multiplier = 1f) => (5f + Random.value * 15f) * multiplier * ActionFrequency.Value;
        private bool IsActionable
        {
            get
            {
#if DEBUG
                var a = GetAvailableActions();
                SensibleH.Logger.LogDebug($"Loop:IsActionable: count = {a.Count}");
                return a.Count > 0;
#else
                return GetAvailableActions().Count > 0;
#endif
            }
        }
        internal void UpdateSettings()
        {
            _actionPickTimer = Mathf.CeilToInt(10f * SensibleH.ActionFrequency.Value);    
        }

        private bool IsVoiceActive => _hVoiceCtrl.nowVoices[CurrentMain].state == HVoiceCtrl.VoiceKind.voice;

        /// <summary>
        /// If name if specified, will most likely appear at 0 index.
        /// Checks start of the string.
        /// </summary>
        private static List<Button> GetAvailableActions(string name = "")
        {
            List<Button> menu;
            switch (hFlag.mode)
            {
                case HFlag.EMode.houshi:
                    menu = _sprite.houshi.categoryActionButton.lstButton;
                    break;
                case HFlag.EMode.houshi3P:
                    menu = _sprite.houshi3P.categoryActionButton.lstButton;
                    break;
                case HFlag.EMode.sonyu:
                    menu = _sprite.sonyu.categoryActionButton.lstButton;
                    break;
                case HFlag.EMode.sonyu3P:
                    menu = _sprite.sonyu3P.categoryActionButton.lstButton;
                    break;
                case HFlag.EMode.houshi3PMMF:
                case HFlag.EMode.sonyu3PMMF:
                    menu = GetDarkSprite(hFlag.mode);
                    break;
                default:
                    return new List<Button>();
            }
            // StartsWith instead of Equal for Dark houshi.
            var choices = name == "" ? menu
                .Where(button => button.isActiveAndEnabled && button.interactable
                && !button.name.StartsWith("Fast", StringComparison.Ordinal) && !button.name.StartsWith("Slow", StringComparison.Ordinal))
                .ToList()

                : menu
                .Where(button => button.isActiveAndEnabled && button.interactable && button.name.StartsWith(name, StringComparison.Ordinal))
                .ToList();
            return choices;
        }
        private static List<Button> GetDarkSprite(HFlag.EMode mode)
        {
            return mode switch
            {
                HFlag.EMode.houshi3PMMF => _sprite.houshi3PDark.categoryActionButton.lstButton,
                HFlag.EMode.sonyu3PMMF => _sprite.sonyu3PDark.categoryActionButton.lstButton,
                _ => new List<Button>()
            };
        }
        public List<HSceneProc.AnimationListInfo> GetAvailableAnimations(int id = -2)
        {
            {
                if (lstUseAnimInfo == null)
                {
                    return new List<HSceneProc.AnimationListInfo>();
                }
                var mode =  HFlag.EMode.none;
                // If it was a delegate request from somewhere out there
                if (id != -2)
                {
                    // For all(-1 aka none) or mode specific one
                        mode = (HFlag.EMode)id;
                }
                else
                {
                    if (SensibleH.ConfigAutoPickPose.Value > AutoPoseType.OnlyIntercourse)
                    {
                        switch (SensibleH.ConfigAutoPickPose.Value)
                        {
                            case AutoPoseType.FemdomOnly:
                                return lstUseAnimInfo
                                    .SelectMany(e => e, (e, anim) => anim)
                                    .Where(anim => anim.mode != HFlag.EMode.aibu && anim.isFemaleInitiative == true)
                                    .ToList();
                            case AutoPoseType.AllPositions:
                                return lstUseAnimInfo
                                    .SelectMany(e => e, (e, anim) => anim)
                                    .Where(anim => anim.mode != HFlag.EMode.aibu)
                                    .ToList();
                        }
                    }
                    else
                    {
                        mode = (HFlag.EMode)SensibleH.ConfigAutoPickPose.Value;
                    }
                }
                return mode switch
                {
                    // -1 aka none for all animations
                    HFlag.EMode.none => lstUseAnimInfo
                        .SelectMany(e => e, (e, anim) => anim)
                        .Where(anim => anim != hFlag.nowAnimationInfo)
                        .ToList(),

                    HFlag.EMode.aibu => lstUseAnimInfo
                        .SelectMany(e => e, (e, anim) => anim)
                        .Where(anim => anim.mode == HFlag.EMode.aibu && anim != hFlag.nowAnimationInfo)
                        .ToList(),

                    HFlag.EMode.houshi => lstUseAnimInfo
                        .SelectMany(e => e, (e, anim) => anim)
                        .Where(anim => (anim.mode == HFlag.EMode.houshi || anim.mode == HFlag.EMode.houshi3P || anim.mode == HFlag.EMode.houshi3PMMF) && anim != hFlag.nowAnimationInfo)
                        .ToList(),

                    HFlag.EMode.sonyu => lstUseAnimInfo
                        .SelectMany(e => e, (e, anim) => anim)
                        .Where(anim => (anim.mode == HFlag.EMode.sonyu || anim.mode == HFlag.EMode.sonyu3P || anim.mode == HFlag.EMode.sonyu3PMMF) && anim != hFlag.nowAnimationInfo)
                        .ToList(),

                    // If didn't fit the criteria, go for the current hFlag.mode
                    _ => lstUseAnimInfo
                        .SelectMany(e => e, (e, anim) => anim)
                        .Where(anim => anim.mode == hFlag.mode && anim != hFlag.nowAnimationInfo)
                        .ToList(),
                };
            }
        }
        
        public static void Sleep()
        {
            Instance.Halt();
        }
        private void Halt()
        {
            if (!_busy)
            {
                _busy = true;
                if (_edgeActive)
                {
                    _edgeActive = false;
                    StopCoroutine(_edgeRoutine);
                }
                if (_speedChangerCo != null)
                {
                    StopCoroutine(_speedChangerCo);
                }
            }
        }
        public static void AlterLoop(int id)
        {
            switch (id)
            {
                case 0:
                    if (IsWeakLoop)
                    {
                        Instance.ChangeSpeed(request: Speed.Slow);
                    }
                    else
                    {
                        Instance.ChangeLoop(request: Loop.Weak);
                    }
                    break;
                case 1:
                    Instance.ChangeLoop(request: Loop.Strong);
                    break;
                case 2:
                    Instance.ChangeLoop(request: Loop.Orgasm);
                    break;
            }

        }
        public void Proc()
        {
#if DEBUG
            SensibleH.Logger.LogDebug($"LoopProc Busy[{_busy}] Restart[{_restart}] Climax[{_hadClimax}]");

#endif
            if (OLoop && !IsOrgasmLoop)
            {
                // Can be turned off manually if HSceneOptions is present.
                OLoop = false;
            }
            if (_edgeActive || (!IsHoushi && !IsSonyu) || _finishLoop)
            {
                return;
            }
            var timer = (int)Time.time;

            if (SensibleHController.IsVR && (MoMiController.Instance._lickCo || handCtrl.isKiss))
            {
#if DEBUG
                SensibleH.Logger.LogInfo($"Loop:Proc:Wait");
#endif
                if (IsStrongLoop || IsOrgasmLoop)
                {
                    ChangeLoop(request: Loop.Weak);
                }
                else if (hFlag.speedCalc > 0.25f)
                {
                    ChangeSpeed(request: Speed.Slow);
                }
            }
            else if (_busy && SensibleH.ConfigAutoMode.Value == AutoModeKind.Auto)
            {
                _busy = false;
                if (SensibleH.ConfigAutoMode.Value == AutoModeKind.Auto && IsHoushi)
                    SetHoushiAutoMode(true);
            }
            else if (SensibleH.ConfigAutoMode.Value != AutoModeKind.Disable && !_busy)
            {
                if (IsActionLoop)
                {
                    if (SensibleH.ConfigEdge.Value != EdgeType.Disable  && _nextEdge < _actionTimer)
                    {
                        if (hFlag.gaugeFemale > 90f || hFlag.gaugeMale > _climaxAt || Random.value < 0.5f)// 0.5f)
                        {
                            _nextEdge = GetNextTimer(3f * SensibleH.EdgeFrequency.Value);
                            return;
                        }
                        _edgeRoutine = StartCoroutine(Edge());
                        return;
                    }
                    _actionTimer += 1;

                    if (IsSonyu)
                    {
                        if (_nextLoopChange < _actionTimer && hFlag.gaugeMale > 10f)
                        {
                            ChangeLoop();
                        }
                        else if (_nextSpeedChange < _actionTimer && !OLoop)
                        {
                            ChangeSpeed();
                        }
                    }
                }
                else if (IsSonyu && IsIdleInside)
                {
                    ChangeMotion();
                }
                if (timer % _actionPickTimer == 0 && !IsVoiceActive && IsActionable)
                {
                    PickAction();
                }
            }
            if (timer % 30 == 0)
            {
                // Even though those are patched, voices at those "random" timings can be quite nice too. In aibu only perhaps.
                //if (hFlag.voice.isFemale70PercentageVoicePlay)
                //    hFlag.voice.isFemale70PercentageVoicePlay = false;
                //if (hFlag.voice.isMale70PercentageVoicePlay)
                //    hFlag.voice.isMale70PercentageVoicePlay = false;

                // No clue if this one patched.
                if (hFlag.voice.isAfterVoicePlay)
                    hFlag.voice.isAfterVoicePlay = false;

                //if (hFlag.gaugeFemale > 70 && Random.value < 0.25f)
                //    Convulsion(Random.value);
            }
        }
        private void SetCeiling()
        {
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:SetCeiling: upThere = {PatchLoop.FemaleUpThere}, top = {PatchLoop.FemaleCeiling}");
#endif
            PatchLoop.FemaleCeiling = Random.Range(70f, 100f);
            PatchLoop.FemaleUpThere = PatchLoop.FemaleCeiling - (10f + Random.value * 10f);
        }
        public void OnPositionChange(HSceneProc.AnimationListInfo nextAnimInfo = null)
        {
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:PositionChange");
#endif
            StopAllCoroutines();
            _nextEdge = GetNextTimer(6f * SensibleH.EdgeFrequency.Value);
            _edgeActive = false;
            OLoop = false;
            _restart = false;
            _hadClimax = false;
            GetBias();
            _busy = SensibleH.ConfigAutoMode.Value != AutoModeKind.Auto;
            if (hFlag.isCondom)
            {
                _sprite.CondomClick();
            }
            var mode = nextAnimInfo == null ? hFlag.mode : nextAnimInfo.mode;
            switch (mode)
            {
                case HFlag.EMode.houshi:
                    if (SensibleH.ConfigAutoMode.Value == AutoModeKind.Auto)
                    {
                        _sprite.houshi.tglRely.isOn = true;
                    }
                    else
                    {
                        _sprite.houshi.tglRely.isOn = false;
                    }
                    break;
                case HFlag.EMode.houshi3P:
                    if (SensibleH.ConfigAutoMode.Value == AutoModeKind.Auto)
                    {
                        _sprite.houshi3P.tglRely.isOn = true;
                    }
                    else
                    {
                        _sprite.houshi3P.tglRely.isOn = false;
                    }
                    break;
                case HFlag.EMode.sonyu:
#if KK
                    _sprite.sonyu.tglAutoFinish.isOn = false;
#else
                    _sprite.sonyu.btAutoFinishSpriteCtl.now = false;
#endif
                    break;
                case HFlag.EMode.sonyu3P:
#if KK
                    _sprite.sonyu3P.tglAutoFinish.isOn = false;
#else
                    _sprite.sonyu3P.btAutoFinishSpriteCtl.now = false;
#endif
                    break;
                case HFlag.EMode.houshi3PMMF:
                case HFlag.EMode.sonyu3PMMF:
                    ToggleAutoDark(mode);
                    break;
                default:
                    break;
            }
        }
        private void ToggleAutoDark(HFlag.EMode mode)
        {
            switch (mode)
            {
                case HFlag.EMode.houshi3PMMF:
                    _sprite.houshi3PDark.tglRely.isOn = SensibleH.ConfigAutoMode.Value == AutoModeKind.Auto;
                    break;
                case HFlag.EMode.sonyu3PMMF:
#if KK
                    _sprite.sonyu3PDark.tglAutoFinish.isOn = false;
#else
                    _sprite.sonyu3PDark.btAutoFinishSpriteCtl.now = false;
#endif
                    break;
            }

        }
        private IEnumerator Edge()
        {
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:Edge:Start");
#endif
            _edgeActive = true;
            //_girlController[CurrentMain].MoveNeckHalt();
            var setting = SensibleH.ConfigEdge.Value;
            var pullOut = IsHoushi || setting == EdgeType.Outside || (setting == EdgeType.Both && Random.value > 0.5f);
            if (ChangeLoop())
            {
                if (IsHoushi)
                    SetHoushiAutoMode(false);
                if (!IsOrgasmLoop)
                {
                    ChangeSpeed(request: Speed.Fast, urgent: true);
                }
                var timer = Time.time + 5f + (Random.value * 5f);
                while (timer > Time.time)
                {
                    yield return new WaitForSeconds(1f);
                }
                if (IsOrgasmLoop)
                {
                    ChangeMotion();
                }
                yield return null;
                ChangeSpeed(request: Speed.Halt, urgent: true);
                yield return new WaitForSeconds(1f);
            }
            if (!pullOut)
            {
                if (!IsVoiceActive)
                {
                    StartCoroutine(RunAfterTimer(() => headManipulators[CurrentMain].PlayVoice(309), Random.Range(1f, 3f)));
                }
                //hFlag.voice.playVoices[CurrentMain] = 309;
            }
            else
            {
                if (IsHoushi && !IsVoiceActive)
                {
                    StartCoroutine(RunAfterTimer(() => headManipulators[CurrentMain].PlayVoice(200), Random.Range(1f, 2f)));
                }
                _wasAnalPlay = hFlag.isAnalPlay;
            }
            var edgeTimer = Time.time + GetRandomRange();
            if (hFlag.nowAnimationInfo.isFemaleInitiative || hFlag.mode == HFlag.EMode.houshi)
            {
                while (edgeTimer > Time.time || IsVoiceActive)
                {
                    yield return new WaitForSeconds(0.2f);
                    ChangeEdge(pullOut);
                }
            }
            else
            {
                while (edgeTimer > Time.time)
                {
                    yield return new WaitForSeconds(0.2f);
                    ChangeEdge(pullOut);
                }
            }
            while (IsIdleOutside)
            {
                yield return new WaitForSeconds(0.2f);
                ChangeEdge();
            }
            if (IsSonyu)
            {
                while (!IsWeakLoop)
                {
                    ChangeMotion();
                    yield return new WaitForSeconds(0.2f);
                }
            }
            ChangeSpeed(request: Speed.Fast, urgent: true);
            _nextEdge = GetNextTimer(6f * SensibleH.EdgeFrequency.Value);
            if (IsHoushi)
            {
                SetHoushiAutoMode(true);
            }
            _edgeActive = false;
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:Edge:End");
#endif
        }
        public static void ClickButton(string name = "")
        {
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:ClickButton:name = {name}");
#endif
            if (name.Equals("None"))
            {
#if DEBUG
                SensibleH.Logger.LogInfo($"Loop:ClickButton:RetrieveClick:click = {_savedClick}");
#endif
                if (_savedClick != HFlag.FinishKind.none)
                {
                    hFlag.finish = _savedClick;
                    _savedClick = HFlag.FinishKind.none;
                }
                return;
            }
            var choices = GetAvailableActions(name);
            var count = choices.Count;
            if (count == 0)
                return;
            // Specific button should be at index 0;
            var number = name.Equals("") ? Random.Range(0, count) : 0;
            var nextAction = choices[number];

#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:ClickButton:nextAction = {nextAction.name}");
#endif
            PatchLoop.FakeButtonUp = true; // koikatsu actions check for left click mouse up
            nextAction.onClick.Invoke();
            PatchLoop.FakeButtonUp = false;
        }




        private void PickAction()
        {
            if (_hadClimax)
            {
                if (SensibleH.ConfigAutoRestart.Value != 0f && !_restart)
                {
#if DEBUG
                    SensibleH.Logger.LogInfo($"Loop:Action:Pick:Restart");
#endif
                    _restart = true;
                    if (SensibleH.ConfigAutoPickPose.Value == AutoPoseType.Disable || Random.value < SensibleH.ConfigAutoRestart.Value)
                    {
                        RestartAction();
                        return;
                    }
                }
                if (SensibleH.ConfigAutoPickPose.Value != AutoPoseType.Disable
                    && (IsIdleOutside || IsEndOutside || IsHoushiOutside))
                {
#if DEBUG
                    SensibleH.Logger.LogInfo($"Loop:Action:Pick:Animation");
#endif
                    PickNextAnimation();
                    return;
                }
            }
            if (hFlag.gaugeMale > _climaxAt && IsActionLoop)
            {
#if DEBUG
                SensibleH.Logger.LogInfo($"Loop:Action:Pick:Climax");
#endif
                if (IsSonyu && IsOrgasmLoop)
                {
                    ChangeLoop(request: Loop.Strong);
                }
                else if (Random.value < 0.25f)
                {
                    ToggleOutsideFinish();
                    StartCoroutine(RunAfterTimer(() => ClickButton(), timer: 0.5f));
                }
                else
                    ClickButton();
            }
            else if (IsEndInside || IsIdleInside || IsIdleOutside || (IsHoushi && IsHoushiOutside))
            {
#if DEBUG
                SensibleH.Logger.LogInfo($"Loop:Action:Pick:Desperate");
#endif
                ClickButton();
            }
        }

        /// <summary>
        /// We change the speed of excitement gain based on different factors.
        /// This results in virgins that can hardly climax and semen demons that defy logic, while making the dude to gravitate towards One-Shot-Joe side.
        /// And we keep stats for the day, so no more rooster-like actions (they apparently can do it for ~1k times a day, all with different partners thought). 
        /// </summary>
        private void GetBias()
        {
            /*
             * things that would modify bias for some time:
             * squirts, convulsions, pullout(as high number for fast decrease of the gauge in that little window)
             * 
             * procs of premature finish from squirts/extra moans/OLoop's/touches/convulsions
             */

            PickHStats(0);
            var familiarity = _master.GetFamiliarity(CurrentMain);
            var lewdness = hFlag.lstHeroine[CurrentMain].lewdness * 0.0033f;
            var numOfClimaxes = (float)LstHeroine[hFlag.lstHeroine[CurrentMain].Name];
            float coefficient;
            if (familiarity < 0.8f) // After 90 intimacy on 2nd Exp.stage, and after 60 on 3rd Exp.stage
                coefficient = SensibleH.ConfigFemaleOrgCount.Value == 0 ? 1f : ((float)SensibleH.ConfigFemaleOrgCount.Value / (SensibleH.ConfigFemaleOrgCount.Value + numOfClimaxes));  //5f / (5f + numOfClimaxes);
            else
                coefficient = 1 + (SensibleH.ConfigFemaleOrgProgression.Value ? (numOfClimaxes * numOfClimaxes * 0.1f) : ( numOfClimaxes * 0.1f));


            BiasF = ConfigBiasF.Value * (familiarity * (hFlag.isCondom ? 0.75f : 1f) * coefficient + lewdness);

            BiasM = ConfigBiasM.Value * ((hFlag.isCondom ? 0.75f : 1f) + lewdness - (ConfigMaleOrgCount.Value == 0 ? 0 : ((float)MaleOrgCount / ConfigMaleOrgCount.Value)));

        }
        /// <summary>
        /// Start/Stop motion.
        /// </summary>
        private void ChangeMotion()
        {
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:Change:Auto");
#endif
            GetBias();
            if (IsOrgasmLoop)
            {
                AnimSetPlay(hFlag.isAnalPlay ? "A_WLoop" : "WLoop");
                OLoop = false;
            }
            else
                hFlag.click = HFlag.ClickKind.modeChange;
        }
        public void OnPreClimax()
        {
            if (!_finishLoop)
            {
                _finishLoop = true;
                if (OLoop)
                {
                    ChangeLoop(request: Loop.Strong);
                }
                StopSpeedChangeCo();
                if (_edgeRoutine != null)
                {
                    StopCoroutine(_edgeRoutine);
                    _edgeActive = false;
                }
                if (maleBreathDelegate != null && maleBreathDelegate((int)ClickType.Inside))
                {
                    SensibleH.Logger.LogInfo($"Loop:SaveClick:hFlag.finish");
                    _savedClick = hFlag.finish;
                    hFlag.finish = HFlag.FinishKind.none;
                }
            }
        }
        private bool ChangeLoop(Loop request = Loop.Random)
        {
#if DEBUG
            SensibleH.Logger.LogInfo("Loop:Change:Motion");
#endif
            bool result = true;
            if (request == Loop.Random)
            {
                if (!IsOrgasmLoop && hFlag.speedCalc > 0.5f && hFlag.gaugeFemale < 85f)
                    request = (Loop)Random.Range(1, 4);
                else
                    request = (Loop)(Random.value > 0.5f ? 1 : 2);
            }
            switch (request)
            {
                case Loop.Weak:
                    if (IsOrgasmLoop)
                    {
                        AnimSetPlay(hFlag.isAnalPlay ? "A_WLoop" : "WLoop");
                        OLoop = false;
                        ChangeSpeed(request: Speed.Slow);
                    }
                    else if (IsStrongLoop)
                    {
                        hFlag.click = HFlag.ClickKind.motionchange;
                    }
                    else
                        result = false;
                    break;
                case Loop.Strong:
                    if (IsOrgasmLoop)
                    {
                        AnimSetPlay(hFlag.isAnalPlay ? "A_SLoop" : "SLoop");
                        OLoop = false;
                        ChangeSpeed(request: Speed.Slow);
                    }
                    else if (IsWeakLoop)
                    {
                        hFlag.click = HFlag.ClickKind.motionchange;
                    }
                    else
                        result = false;
                    break;
                case Loop.Orgasm:
                    if (IsStrongLoop || IsWeakLoop)
                    {
                        OLoop = true;
                        AnimSetPlay(hFlag.isAnalPlay ? "A_OLoop" : "OLoop");
                        ChangeSpeed(request: Speed.Max, urgent: true);
                        if (!IsVoiceActive && Random.value < 0.5f)
                        {
                            hFlag.voice.playVoices[CurrentMain] = 313;
                        }
                    }
                    else
                        result = false;
                    break;
            }
            _nextLoopChange = GetNextTimer(OLoop ? 0.5f : 1f);
            return result;
            //if (Random.value > 0.5f)
            //    ChangeSpeed();
        }
        internal void AnimSetPlay(string _animation)
        {
            _lstProc[(int)hFlag.mode].SetPlay(_animation, true);
        }
        private void ChangeEdge(bool _pullOut = false)
        {
            switch (hFlag.mode)
            {
                case HFlag.EMode.houshi:
                    if (!_pullOut && IsIdleOutside)
                    {
                        //SensibleH.Logger.LogInfo($"Loop:Change:Edge:1");
                        //_girlController[CurrentMain].PlayVoice()
                        hFlag.click = HFlag.ClickKind.speedup;
                        hFlag.rely = true;
                        GetBias();
                    }
                    else if (_pullOut && IsActionLoop)
                    {
                        //SensibleH.Logger.LogInfo($"Loop:Change:Edge:3");
                        //_girlController[CurrentMain].PlayVoice()
                        hFlag.rely = false;
                        hFlag.speedCalc = 0f;
                        _lstProc[(int)hFlag.mode].MotionChange(0);
                    }
                    break;
                case HFlag.EMode.sonyu:
                    if (!_pullOut && IsIdleOutside && IsActionable)
                    {
                        //SensibleH.Logger.LogInfo($"Loop:Change:Edge:1");

                        //if (hFlag.nowAnimationInfo.isFemaleInitiative)
                        //    ActionButton("Insert_novoice_female");
                        if (_wasAnalPlay || (hFlag.isAnalInsertOK && Random.value < 0.2f))
                            ClickButton("InsertAnal_novoice");
                        else
                            ClickButton("Insert_novoice");
                    }
                    else if (IsActionLoop)
                    {
                        //SensibleH.Logger.LogInfo($"Loop:Change:Edge:2");
                        hFlag.speedCalc = 0f;
                        ChangeMotion();
                    }
                    else if (_pullOut && IsIdleInside && IsActionable)
                    {
                        //if (hFlag.nowAnimationInfo.isFemaleInitiative)
                        //    _girlControllers[CurrentMain].SupressVoice(341);
                        //SensibleH.Logger.LogInfo($"Loop:Change:Edge:3");
                        ClickButton("Pull");
                    }
                    break;
            }
        }
        private enum Speed
        {
            Random,
            Slow,
            Fast,
            Max,
            Halt
        }
        private void ChangeSpeed(Speed request = Speed.Random, bool urgent = false)
        {
#if DEBUG
            SensibleH.Logger.LogInfo("Loop:Change:Speed");
#endif
            var speedCalc = hFlag.speedCalc;
            var speedOfChange = 1f;
            var excitementBias = hFlag.gaugeMale * 0.0033f;
            switch (request)
            {
                case Speed.Random:
                    if (speedCalc < 0.1f)
                        speedCalc += excitementBias + (Random.value * 0.5f);
                    else if (speedCalc > 0.9f)
                        speedCalc -= 0.33f - excitementBias + (Random.value * 0.5f);
                    else
                        speedCalc = Mathf.Clamp01(speedCalc + ((0.25f + Random.value * 0.5f) * (Random.value > 0.5f ? 1 : -1)));
                    break;
                case Speed.Slow:
                    speedCalc = Random.value * 0.25f;
                    break;
                case Speed.Fast:
                    speedCalc = 1f - Random.value * 0.25f;
                    break;
                case Speed.Max:
                    speedCalc = 1f;
                    break;
                default:
                    speedCalc = 0f;
                    break;
            }

            if (!urgent)
            {
                speedOfChange = Mathf.Abs(hFlag.speedCalc - speedCalc) * GetRandomRange();
            }
            _nextSpeedChange = GetNextTimer(1f);
            StopSpeedChangeCo();
            _speedChangerCo = StartCoroutine(SpeedChangeCo(hFlag.speedCalc, speedCalc, speedOfChange));
        }
        private void StopSpeedChangeCo()
        {
            if (_speedChangerCo != null)
            {
                StopCoroutine(_speedChangerCo);
            }
        }
        
        /// <summary>
        /// We change the speed of animation over time.
        /// </summary>
        private IEnumerator SpeedChangeCo(float start, float target, float speedOfChange)
        {
            //SensibleH.Logger.LogInfo($"SpeedManipulator {start} {target} {speedOfChange}");
            var step = (target - start) / speedOfChange * Time.deltaTime;
            var absStep = Mathf.Abs(step);
            while (Mathf.Abs(start - target) > absStep)
            {
                start += step;
                hFlag.speedCalc = start;
                yield return null;
            }
            hFlag.speedCalc = target;
        }

        private void RestartAction()
        {
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:Action:Restart");
#endif
            GetBias();
            _nextEdge = GetNextTimer(6f * SensibleH.EdgeFrequency.Value);
            _nextLoopChange = GetNextTimer();
            _restart = false;
            _hadClimax = false;
            _busy = false;
            //_userInput = true;
            if (IsHoushi)
            {
                hFlag.click = HFlag.ClickKind.again;
            }
            else if (IsSonyu)
            {
                if (IsEndOutside || IsIdleOutside)
                {
                    ClickButton();
                }
                else
                {
                    ChangeMotion();
                }
            }
        }
        public static void OnUserInput()
        {
            Instance.UserInput();
        }
        public void UserInput()
        {
#if DEBUG
            SensibleH.Logger.LogInfo($"Loop:UserInput:busy = {_busy}");
#endif
            if (_busy)
            {
                if (_hadClimax)
                {

                    if (IsActionLoop)
                    {
                        //SensibleH.Logger.LogInfo("Loop:UserInput:UnusualState");
                        // In case the user decides to do some actions after orgasm ahead of the plugin (or plugin was disabled and re-enabled).
                        _hadClimax = false;
                        _busy = false;
                    }
                    else if (SensibleH.ConfigAutoMode.Value == AutoModeKind.UserStartFinish && (IsEndLoop || IsIdleInside || IsIdleOutside))
                    {
                        _busy = false;
                        //SensibleH.Logger.LogInfo("Loop:UserInput:PostClimax");
                    }
                }
                else
                {
                    if (SensibleH.ConfigAutoMode.Value == AutoModeKind.UserStart || SensibleH.ConfigAutoMode.Value == AutoModeKind.UserStartFinish)
                    {
                        _busy = false;
                        //_userInput = true;
                        if (IsHoushi)
                            SetHoushiAutoMode(true);
                        //SensibleH.Logger.LogInfo("Loop:UserInput:PreClimax");
                    }
                }
            }
        }
        /// <summary>
        /// Toggle "climax outside".
        /// </summary>
        private void ToggleOutsideFinish()
        {
            //SensibleH.Logger.LogInfo($"Loop:Action:Climax:Toggle");
            switch (hFlag.mode)
            {
                case HFlag.EMode.houshi:
#if KK
                    if (_sprite.houshi.tglAutoFinish.isOn)
                        _sprite.houshi.tglAutoFinish.isOn = false;
                    else
                        _sprite.houshi.tglAutoFinish.isOn = true;
#else
                    if (_sprite.houshi.btAutoFinishSpriteCtl.now)
                        _sprite.houshi.btAutoFinishSpriteCtl.now = false;
                    else
                        _sprite.houshi.btAutoFinishSpriteCtl.now = true;
#endif
                    break;
                case HFlag.EMode.houshi3P:
#if KK
                    if (_sprite.houshi3P.tglAutoFinish.isOn)
                        _sprite.houshi3P.tglAutoFinish.isOn = false;
                    else
                        _sprite.houshi3P.tglAutoFinish.isOn = true;
#else
                    if (_sprite.houshi3P.btAutoFinishSpriteCtl.now)
                        _sprite.houshi3P.btAutoFinishSpriteCtl.now = false;
                    else
                        _sprite.houshi3P.btAutoFinishSpriteCtl.now = true;
#endif
                    break;
                case HFlag.EMode.sonyu:
#if KK
                    if (_sprite.sonyu.tglAutoFinish.isOn)
                        _sprite.sonyu.tglAutoFinish.isOn = false;
                    else
                        _sprite.sonyu.tglAutoFinish.isOn = true;
#else
                    if (_sprite.sonyu.btAutoFinishSpriteCtl.now)
                        _sprite.sonyu.btAutoFinishSpriteCtl.now = false;
                    else
                        _sprite.sonyu.btAutoFinishSpriteCtl.now = true;
#endif
                    break;
                case HFlag.EMode.sonyu3P:
#if KK
                    if (_sprite.sonyu3P.tglAutoFinish.isOn)
                        _sprite.sonyu3P.tglAutoFinish.isOn = false;
                    else
                        _sprite.sonyu3P.tglAutoFinish.isOn = true;
#else
                    if (_sprite.sonyu3P.btAutoFinishSpriteCtl.now)
                        _sprite.sonyu3P.btAutoFinishSpriteCtl.now = false;
                    else
                        _sprite.sonyu3P.btAutoFinishSpriteCtl.now = true;
#endif
                    break;

            }
        }
        /// <summary>
        /// We load chosen animation in fake button and "click" it.
        /// </summary>
        public static void PickAnimation(int id)
        {
            if (Instance != null)
            {
                Instance.PickNextAnimation(id);
            }
        }
        private void PickNextAnimation(int id = -2)
        {
            //SensibleH.Logger.LogInfo($"Loop:Action:Animation:Pick");
            var choices = GetAvailableAnimations(id);
            if (choices.Count == 0)
            {
                return;
            }
            nextAnimation = choices[Random.Range(0, choices.Count)];
            fakeAnimButton.GetComponent<HSprite.AnimationInfoComponent>().info = nextAnimation;
            fakeAnimButton.GetComponent<Toggle>().isOn = false;
            _sprite.OnChangePlaySelect(fakeAnimButton);
            fakeAnimButton.GetComponent<HSprite.AnimationInfoComponent>().info = null;
        }
        private IEnumerator RunAfterTimer(Action method, float timer = 1f, params object[] _args)
        {
            timer += Time.time;
            while (timer > Time.time)
            {
                yield return new WaitForSeconds(0.2f);
            }
            method.DynamicInvoke(_args);
        }
        private IEnumerator RunAfterPullout(Action _method, params object[] _args)
        {
            SuppressVoice = true;
            var timer = Time.time + 5f;
            while (!IsIdleOutside && !IsPull)
            {
                if (timer < Time.time)
                {
                    // Some bugs? and unexpected user input may leave us hanging.
                    yield break;
                }
                //SensibleH.Logger.LogInfo($"Loop:RunAfter:Pullout");
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            _method.DynamicInvoke(_args);
        }
        private IEnumerator RunAfterInsert(Action _method, params object[] _args)
        {
            //SensibleH.Logger.LogInfo($"Loop:RunAfter:Insert");
            yield return null;

            if (hFlag.isDenialvoiceWait)
            {
                _hadClimax = true;
                yield break;
            }
            SuppressVoice = true;
            while (IsVoiceActive)
            {
                // In case of voice interruption.
                yield return null;
            }
            var timer = Time.time + 5f;
            while (!IsInsert)
            {
                if (timer < Time.time)
                {
                    // Some bugs? and unexpected user input may leave us hanging.
                    yield break;
                }
                //SensibleH.Logger.LogInfo($"Loop:RunAfter:Insert");
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            _method.DynamicInvoke(_args);
        }
        /// <summary>
        ///  We count the amount of orgasms per day per girl and keep it for a day. No kPlug integration.
        /// </summary>
        private void PickHStats(int _addOrg)
        {
            var name = hFlag.lstHeroine[CurrentMain].Name;
            //SensibleH.Logger.LogInfo($"Loop:HStats:{name}");
            if (!LstHeroine.ContainsKey(name))
                LstHeroine.Add(name, 0);

            LstHeroine[name] += _addOrg;
        }
        /// <summary>
        /// We add extra HitReactionPlay()s to "No Voice Insert" and "Pullout".
        /// </summary>
        public void OnSonyuClick(bool pullOut)
        {
            // There is a case when we click the moment animation changes from climax finish to idle, this won't register click.
            if (runAfterCoroutine != null)
            {
                StopCoroutine(runAfterCoroutine);
            }
            //SensibleH.Logger.LogInfo($"OnSonyuClick");
            if (!pullOut)
            {
                runAfterCoroutine = StartCoroutine(RunAfterInsert(() => headManipulators[CurrentMain].StartConvulsion(time: Random.value, playVoiceAfter: true)));
            }
            else
            {
                var specificVoice = -1;
                if (!hFlag.nowAnimationInfo.isFemaleInitiative)
                {
                    if (_hadClimax)
                        specificVoice = 303;
                }
                else
                {
                    specificVoice = 347;
                }
                runAfterCoroutine = StartCoroutine(RunAfterPullout(() => headManipulators[CurrentMain].StartConvulsion(time: Random.value, playVoiceAfter: true, specificVoice)));
            }
        }
        //private void OnInsert()
        //{
        //    _girlControllers[CurrentMain].StartConvulsion(Random.value, true);
        //}
        //private void OnPullout()
        //{

        //}
        public void DoAnalClick()
        {
            if (hFlag.isAnalInsertOK)
                OnSonyuClick(pullOut: false);
        }
        public void OnOrgasmF()
        {
            //SensibleH.Logger.LogInfo($"Loop:Orgasm:F");
            PickHStats(1);
            if (SensibleH.ConfigAutoMode.Value == AutoModeKind.UserStartFinish)
            {
                _busy = true;
            }
            _hadClimax = true;
            _finishLoop = false;
            SetCeiling();
        }
        public void OnOrgasmM()
        {
            //SensibleH.Logger.LogInfo($"Loop:Orgasm:M");
            if (SensibleH.ConfigAutoMode.Value == AutoModeKind.UserStartFinish)
            {
                _busy = true;
            }

            _climaxAt = Random.Range(75, 100);
            _hadClimax = true;
            _finishLoop = false;
            MaleOrgCount += 1;
            SetHoushiAutoMode(state: false);
        }
        private void SetHoushiAutoMode(bool state)
        {
            switch (hFlag.mode)
            {
                case HFlag.EMode.houshi:
                    _sprite.houshi.tglRely.Set(state);
                    break;
                case HFlag.EMode.houshi3P:
                    _sprite.houshi3P.tglRely.Set(state);
                    break;
                case HFlag.EMode.houshi3PMMF:
                    SetHoushiAutoModeDark(state);
                    break;
            }
        }
        private void SetHoushiAutoModeDark(bool state)
        {
            _sprite.houshi3PDark.tglRely.Set(state);
        }
    }
}