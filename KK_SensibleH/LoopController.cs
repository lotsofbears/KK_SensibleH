using BepInEx.Unity;
//using InputSimulator = BepInEx.Unity.InputSimulator;
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
using static KK_SensibleH.LoopParameters;

namespace KK_SensibleH
{
    /// <summary>
    /// TODO Catch and suppress voice PROPERLY.
    /// </summary>
    public class LoopController : GameCustomFunctionController
    {
        public static LoopController Instance;
        private HSceneProc.AnimationListInfo nextAnimation;
        private List<HActionBase> lstProc;
        private Traverse<List<HSceneProc.AnimationListInfo>[]> lstUseAnimInfo;
        private GameObject fakeAnimButton;
        private int _actionTimer;
        private int _nextSpeedChange;
        private int _nextLoopChange;
        private int _nextEdge;
        private int _climaxAt = Random.Range(75, 99);
        private bool _wasAnalPlay;
        private bool _edgeActive;
        private bool _busy;
        private bool _userInput;
        private bool _restart;
        private bool _hadClimax;
        internal void Initialize(MonoBehaviour _proc)
        {
            Instance = this;
            OnPositionChange();
            lstUseAnimInfo = Traverse.Create(_proc).Field<List<HSceneProc.AnimationListInfo>[]>("lstUseAnimInfo");
            lstProc = Traverse.Create(_proc).Field("lstProc").GetValue<List<HActionBase>>();
            _sprite = Traverse.Create(_proc).Field("sprite").GetValue<HSprite>();
            fakeAnimButton = Instantiate(_sprite.objMotionListNode, gameObject.transform, false);
            fakeAnimButton.AddComponent<HSprite.AnimationInfoComponent>();
            fakeAnimButton.SetActive(true);
        }
        private Coroutine _speedChangerCo;
        private Coroutine runAfterCoroutine; // It really is necessary due to crossFader, other way is a mess;
        private int GetNextTimer(float multiplier = 1f) => _actionTimer + (int)((5f + Random.value * 15f) * multiplier * ActionFrequency.Value);
        private float GetRandomRange(float multiplier = 1f) => (5f + Random.value * 15f) * multiplier * ActionFrequency.Value;
        private bool IsActionable => GetAvailableActions().Count > 0;
        
        internal bool IsVoiceActive => _hVoiceCtrl.nowVoices[CurrentMain].state == HVoiceCtrl.VoiceKind.voice;

        private List<Button> GetAvailableActions(string _name = "")
        {
            List<Button> menu;
            switch (_hFlag.mode)
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
                default:
                    return new List<Button>();
            }
            var choices = _name == String.Empty ? menu
                .Where(button => button.isActiveAndEnabled && button.interactable)
                .ToList() 
                : menu
                .Where(button => button.isActiveAndEnabled && button.interactable && button.name.Contains(_name))
                .ToList();
            return choices;
        }
        public List<HSceneProc.AnimationListInfo> GetAvailableAnimations
        {
            get
            {

                if (lstUseAnimInfo == null)
                {
                    return new List<HSceneProc.AnimationListInfo>();
                }
                if (SensibleH.AutoPickPosition.Value == AutoPosMode.AllPositions)
                {
                    return lstUseAnimInfo.Value
                        .SelectMany(e => e, (e, anim) => anim)
                        .Where(anim => anim.mode != HFlag.EMode.aibu)
                        .ToList();
                }
                else
                {
                    return lstUseAnimInfo.Value
                        .SelectMany(e => e, (e, anim) => anim)
                        .Where(anim => anim.mode != HFlag.EMode.aibu && anim.isFemaleInitiative == true)
                        .ToList();
                }
            }
        }

        public void Proc()
        {
            //SensibleH.Logger.LogDebug($"LoopProc Busy[{_busy}] Restart[{_restart}] UserInput[{_userInput}] Climax[{_hadClimax}]");

            if (_edgeActive)
                return; 
            var timer = (int)Time.time;
            if (MoMiController._lickCo || _handCtrl.isKiss)
            {
                SensibleH.Logger.LogDebug($"LoopProc LickKiss wait");
                if (IsStrongLoop || IsOrgasmLoop)
                {
                    ChangeLoop(request: Loop.Weak);
                }
                else if (_hFlag.speedCalc > 0.2f)
                {
                    ChangeSpeed(request: Speed.Slow);
                }
            }
            else if (_busy && (_userInput || SensibleH.AutoMode.Value == AutoModeKind.Automatic))
            {
                _busy = false;
                _userInput = false;
                if (SensibleH.AutoMode.Value == AutoModeKind.Automatic && IsHoushi)
                    ToggleHoushiAutoMode(true);
            }
            else if (SensibleH.AutoMode.Value != AutoModeKind.Disabled && !_busy)
            {
                if (IsActionLoop && !IsFinishLoop)
                {
                    if (SensibleH.Edge.Value && _nextEdge < _actionTimer)
                    {
                        if (_hFlag.gaugeFemale > 90f || _hFlag.gaugeMale > _climaxAt || Random.value < 0.5f)// 0.5f)
                        {
                            _nextEdge = GetNextTimer(3f);
                            return;
                        }
                        StartCoroutine(Edge());
                        return;
                    }
                    _actionTimer += 1;
                    if (IsSonyu)
                    {
                        if (_hFlag.gaugeFemale > 90f && IsOrgasmLoop)
                        {
                            ChangeLoop(request: Loop.Strong);
                        }
                        else if (_nextLoopChange < _actionTimer && _hFlag.gaugeMale > 10f && _hFlag.gaugeFemale < 90f)
                            ChangeLoop();
                        else if (_nextSpeedChange < _actionTimer)
                            ChangeSpeed();
                    }
                }
                if (IsSonyu && IsIdleInside)
                    ChangeMotion();

                if (timer % 5 == 0 && !IsVoiceActive && IsActionable)
                {
                    PickAction();
                }
            }
            if (timer % 30 == 0)
            {
                if (_hFlag.voice.isFemale70PercentageVoicePlay)
                    _hFlag.voice.isFemale70PercentageVoicePlay = false;
                if (_hFlag.voice.isMale70PercentageVoicePlay)
                    _hFlag.voice.isMale70PercentageVoicePlay = false;
                if (_hFlag.voice.isAfterVoicePlay)
                    _hFlag.voice.isAfterVoicePlay = false;
                //if (_hFlag.gaugeFemale > 70 && Random.value < 0.25f)
                //    Convulsion(Random.value);
            }
        }
        public void OnPositionChange(HSceneProc.AnimationListInfo nextAnimInfo = null)
        {
            SensibleH.Logger.LogDebug("OnPositionChange");
            StopAllCoroutines();
            _nextEdge = GetNextTimer(6f * SensibleH.EdgeFrequency.Value);
            _edgeActive = false;
            if (AutoMode.Value != AutoModeKind.Automatic)
            {
                _busy = true;
                _userInput = false;
            }
            else
            {
                _userInput = true;
                _busy = false;
            }
            if (_hFlag.isCondom)
            {
                _sprite.CondomClick();
            }
            _restart = false;
            _hadClimax = false;
            _hFlag.finish = HFlag.FinishKind.none;
            GetBias(); 
            if (nextAnimInfo != null)
            {
                switch (nextAnimInfo.mode)
                {
                    case HFlag.EMode.houshi:
                    case HFlag.EMode.houshi3P:
                        if (SensibleH.AutoMode.Value == AutoModeKind.Automatic)
                            _sprite.houshi.tglRely.isOn = true;
                        else
                            _sprite.houshi.tglRely.isOn = false;
                        //_hFlag.rely = true;
                        //Sprite.rely.InitTimer();
                        break;
                    case HFlag.EMode.sonyu:
                        _sprite.sonyu.tglAutoFinish.isOn = false;
                        break;
                }
            }
        }
        private IEnumerator Edge()
        {
            SensibleH.Logger.LogDebug($"Edge[Start]");
            _edgeActive = true;
            //_girlController[CurrentMain].MoveNeckHalt();
            var pullOut = IsHoushi || Random.value < 0.5f;
            if (ChangeLoop())
            {
                if (IsHoushi)
                    ToggleHoushiAutoMode(false);
                ChangeSpeed(request: Speed.Fast, urgent: true);
                var timer = Time.time + 5f + (Random.value * 5f);
                while (timer > Time.time)
                {
                    yield return new WaitForSeconds(1f);
                }
                if (IsOrgasmLoop)
                    ChangeMotion(); 

                ChangeSpeed(request: Speed.Halt, urgent: true);
                yield return new WaitForSeconds(1f);
            }
            if (!pullOut)
                StartCoroutine(RunAfterTimer(() => _girlController[CurrentMain].PlayVoice(309), Random.Range(1f, 3f)));
            //_hFlag.voice.playVoices[CurrentMain] = 309;
            else
            {
                if (IsHoushi && !IsVoiceActive)
                    StartCoroutine(RunAfterTimer(() => _girlController[CurrentMain].PlayVoice(200), Random.Range(1f, 2f)));
                //_hFlag.voice.playVoices[CurrentMain] = 200;
                _wasAnalPlay = _hFlag.isAnalPlay;
            }
            var edgeTimer = Time.time + 5f + (Random.value * 10f);
            while (edgeTimer > Time.time)
            {
                yield return new WaitForSeconds(0.2f);
                ChangeEdge(pullOut);
            }
            while (IsIdleOutside)
            {
                yield return new WaitForSeconds(0.2f);
                ChangeEdge();
            }
            if (Random.value < 0.5f)
            {
                ChangeLoop(request: Loop.Strong);
                ChangeSpeed(urgent: true);
            }    
            _nextEdge = GetNextTimer(6f * SensibleH.EdgeFrequency.Value);
            _edgeActive = false;
            if (IsHoushi)
                ToggleHoushiAutoMode(true);
            SensibleH.Logger.LogDebug($"Edge[End]");
        }
        private void ActionButton(string name = "")
        {
            SensibleH.Logger.LogDebug($"LoopController[ActionButton]");
            List<Button> choices;

            if (name == String.Empty)
                choices = GetAvailableActions();
            else
                choices = GetAvailableActions(name);

            if (choices.Count == 0)
                return;

            var nextAction = choices[Random.Range(0, choices.Count)];
            SensibleH.Logger.LogDebug($"LoopController[ActionButton] Pick[{nextAction}]");
            BepInEx.Unity.InputSimulator.MouseButtonUp(0); // koikatsu actions check for left click mouse up
            nextAction.onClick.Invoke();
            BepInEx.Unity.InputSimulator.UnsetMouseButton(0);
            CatchDenial();
        }
        private void CatchDenial()
        {
            if (_hFlag.isDenialvoiceWait)
            {
                //    SensibleH.Logger.LogDebug($"CatchDenial proc");
                //    if ((SensibleH.Asshole.Value == CondomMode.DontLikeIt && (Random.value < 0.3f)) || SensibleH.Asshole.Value == CondomMode.NeverHeardOf)
                //    {
                //        PullInit(true);
                //        SensibleH.Logger.LogDebug($"CatchDenial _button true FULL SPEED AHEAD");
                //    }
                //    else if (SensibleH.CondomTease.Value && (Random.value < 0.5f))
                //    {
                //        StartCoroutine(RunAfter(() => _sprite.CondomClick(), true));
                //    }
                //}
            }
        }
        private void PickAction()
        {
            SensibleH.Logger.LogDebug($"PickAction");

            
            if (SensibleH.AutoRestartAction.Value && _hadClimax && !_restart)
            {
                _restart = true;
                SensibleH.Logger.LogDebug($"PickAction[AutoRestartAction]");
                if (SensibleH.AutoPickPosition.Value == AutoPosMode.Disabled)
                {
                    RestartAction();
                    return;
                }
                else if (Random.value < 0f)//0.3f)
                {
                    RestartAction();
                    return;
                }
            }
            if (SensibleH.AutoPickPosition.Value != AutoPosMode.Disabled && _hadClimax && (IsIdleOutside || IsEndOutside))
            {
                SensibleH.Logger.LogDebug($"PickAction[NextAnimation]");
                PickNextAnimation();
            }
            else if (IsActionLoop && _hFlag.gaugeMale > _climaxAt)
            {
                SensibleH.Logger.LogDebug($"PickAction[AutoClimax]");
                if (IsOrgasmLoop)
                {
                    ChangeLoop(request: Loop.Strong);
                }
                else if (Random.value < 0.25f)
                {
                    ToggleOutsideFinish();
                    StartCoroutine(RunAfterTimer(() => ActionButton(), timer: 0.5f));
                }
                else
                    ActionButton();
            }
            else if (IsEndInside || IsIdleInside || IsIdleOutside)          
            {
                SensibleH.Logger.LogDebug($"PickAction[PullOut] just press something");
                ActionButton();
            }
        }

        internal float GetFamiliarity(int main)
        {
            var hExp = 0.2f + ((int)_hFlag.lstHeroine[main].HExperience * 0.2f);
            if (!_hFlag.isFreeH)
            {
                if (_hFlag.mode != HFlag.EMode.lesbian && _hFlag.mode != HFlag.EMode.masturbation)
                {
                    hExp *= (0.5f + (_hFlag.lstHeroine[main].intimacy * 0.01f));
                }
                else
                    hExp *= (0.5f + (_hFlag.lstHeroine[main].lewdness * 0.01f));
            }
            return hExp;
        }
        /// <summary>
        /// We change the speed of excitement gain based on different factors.
        /// This results in virgins that can hardly climax and semen demons that defy logic, while making the dude to gravitate towards One-Shot-Joe side.
        /// </summary>
        private void GetBias()
        {
            /*
             * Run this one every 10/30 sec and on extra occasions.
             * 
             * things that would modify bias for some time:
             * squirts, convulsions, pullout(as high number for fast decrease of the gauge in that little window)
             * 
             * procs of premature finish from squirts/extra moans/OLoop's/touches/convulsions
             */

            PickHStats(0);
            var familiarity = GetFamiliarity(CurrentMain);
            var lewdness = _hFlag.lstHeroine[CurrentMain].lewdness * 0.005f;
            var numOfClimaxes = LstHeroine[_hFlag.lstHeroine[CurrentMain].Name];
            float coefficient;
            if (familiarity < 0.55f)
                coefficient = 2f / (2f + numOfClimaxes);
            else
                coefficient = 1 + (numOfClimaxes * 0.1f);

            BiasF = familiarity * (_hFlag.isCondom ? 0.75f : 1f) * coefficient + lewdness;

            BiasM = (_hFlag.isCondom ? 0.5f : 1f) - (MaleOrgCount * 0.2f);

            SensibleH.Logger.LogDebug($"BiasF = {BiasF}, BiasM = {BiasM}");
        }
        private void ChangeMotion()
        {
            SensibleH.Logger.LogDebug($"LoopController[ChangeMotion]");
            GetBias();
            if (IsOrgasmLoop)
            {
                AnimSetPlay(_hFlag.isAnalPlay ? "A_WLoop" : "WLoop");
                OLoop = false;
            }
            else 
                _hFlag.click = HFlag.ClickKind.modeChange;
        }
        private enum Loop
        {
            Random,
            Weak,
            Strong,
            Orgasm
        }
        private bool ChangeLoop(Loop request = Loop.Random)
        {
            SensibleH.Logger.LogDebug("ChangeLoop");
            bool result = true;
            if (request == Loop.Random)
            {
                if (!IsOrgasmLoop && _hFlag.speedCalc > 0.5f)
                    request = (Loop)Random.Range(1, 4);
                else
                    request = (Loop)(Random.value > 0.5f ? 1 : 2);
            }
            switch (request)
            {
                case Loop.Weak:
                    if (IsOrgasmLoop)
                    {
                        AnimSetPlay(_hFlag.isAnalPlay ? "A_WLoop" : "WLoop");
                        OLoop = false;
                    }
                    else if (IsStrongLoop)
                    {
                        _hFlag.click = HFlag.ClickKind.motionchange;
                    }
                    else
                        result = false;
                    break;
                case Loop.Strong:
                    if (IsOrgasmLoop)
                    {
                        AnimSetPlay(_hFlag.isAnalPlay ? "A_SLoop" : "SLoop");
                        OLoop = false;
                    }
                    else if (IsWeakLoop)
                    {
                        _hFlag.click = HFlag.ClickKind.motionchange;
                    }
                    else
                        result = false;
                    break;
                case Loop.Orgasm:
                    if (_hFlag.gaugeMale < _climaxAt && (IsStrongLoop || IsWeakLoop))
                    {
                        OLoop = true;
                        AnimSetPlay(_hFlag.isAnalPlay ? "A_OLoop" : "OLoop");
                        if (_speedChangerCo != null)
                            StopCoroutine(_speedChangerCo);
                        _hFlag.speedCalc = 1f;
                        if (!IsVoiceActive && Random.value < 0.5f)
                            _hFlag.voice.playVoices[CurrentMain] = 313;
                    }
                    else
                        result = false;
                    break;
            }
            _nextLoopChange = GetNextTimer();
            return result;
            //if (Random.value > 0.5f)
            //    ChangeSpeed();

        }
        //private void ChangeLoop(bool _OLoop = false)
        //{
        //    SensibleH.Logger.LogDebug($"ChangeLoop");
        //    if (_OLoop || (!OLoop && _hFlag.speedCalc > 0.6f && _hFlag.gaugeMale > 20f && _hFlag.gaugeMale < 80f && _hFlag.gaugeFemale < 80f && Random.value < 0.2f))
        //    {
        //        AnimSetPlay(_hFlag.isAnalPlay ? "A_OLoop" : "OLoop");
        //        if (_speedChangerCo != null)
        //            StopCoroutine(_speedChangerCo);
        //        _hFlag.speedCalc = 1f;
        //        if (IsSonyu && !_girlController[CurrentMain].IsVoiceActive && Random.value < 0.5f)
        //        {
        //            _girlController[CurrentMain].PlayVoice(313);
        //            //else

        //        }
        //        _girlController[CurrentMain].MoveNeckHalt();
        //        OLoop = true;
        //        loopWait = speedWait = GetRandomRange;
        //        return;
        //    }
        //    else if (OLoop)
        //    {
        //        if (Random.value < 0.5f)
        //            AnimSetPlay(_hFlag.isAnalPlay ? "A_SLoop" : "SLoop");
        //        else
        //            AnimSetPlay(_hFlag.isAnalPlay ? "A_WLoop" : "WLoop");
        //        OLoop = false;
        //    }
        //    else if (Random.value < 0.33f)
        //    {
        //        loopWait = GetRandomRange;
        //        return;
        //    }
        //    else
        //        _hFlag.click = HFlag.ClickKind.motionchange;
        //    ChangeSpeed(forced: true, fast: true);
        //    loopWait = GetRandomRange;
        //}

        internal void AnimSetPlay(string _animation)
        {
            lstProc[(int)_hFlag.mode].SetPlay(_animation, true);
        }
        private void ChangeEdge(bool _pullOut = false)
        {
            switch (_hFlag.mode)
            {
                case HFlag.EMode.houshi:
                    if (!_pullOut && IsIdleOutside)
                    {
                        SensibleH.Logger.LogDebug($"ChangeEdge[1]");
                        //_girlController[CurrentMain].PlayVoice()
                        _hFlag.click = HFlag.ClickKind.speedup;
                        _hFlag.rely = true;
                        GetBias();
                    }
                    else if (_pullOut && IsActionLoop)
                    {
                        SensibleH.Logger.LogDebug($"ChangeEdge[3]");
                        //_girlController[CurrentMain].PlayVoice()
                        _hFlag.rely = false;
                        _hFlag.speedCalc = 0f;
                        lstProc[(int)_hFlag.mode].MotionChange(0);
                    }
                    break;
                case HFlag.EMode.sonyu:
                    if (!_pullOut && IsIdleOutside && IsActionable)
                    {
                        SensibleH.Logger.LogDebug($"ChangeEdge[1]");

                        //if (_hFlag.nowAnimationInfo.isFemaleInitiative)
                        //    ActionButton("Insert_novoice_female");
                        if (_wasAnalPlay || (_hFlag.isAnalInsertOK && Random.value < 0.2f))
                            ActionButton("InsertAnal_novoice");
                        else
                            ActionButton("Insert_novoice");
                    }
                    else if (IsActionLoop)
                    {
                        SensibleH.Logger.LogDebug($"ChangeEdge[2]");
                        _hFlag.speedCalc = 0f;
                        ChangeMotion();
                    }
                    else if (_pullOut && IsIdleInside && IsActionable)
                    {
                        if (_hFlag.nowAnimationInfo.isFemaleInitiative)
                            _girlController[CurrentMain].SupressVoice(341);
                        SensibleH.Logger.LogDebug($"LoopController[ChangeEdge] 3");
                        ActionButton("Pull");
                    }
                    break;
            }
        }
        private enum Speed
        {
            Random,
            Slow,
            Fast,
            Halt
        }
        private void ChangeSpeed(Speed request = Speed.Random, bool urgent = false)
        {
            SensibleH.Logger.LogDebug("ChangeSpeed");
            var speedCalc = _hFlag.speedCalc;
            var speedOfChange = 1f;
            if (request == Speed.Random)
            {
                if (speedCalc < 0.1f)
                    speedCalc += Mathf.Clamp(Random.value, 0f, 0.75f);
                else if (speedCalc > 0.9f)
                    speedCalc -= Mathf.Clamp(Random.value, 0f, 0.75f);
                else
                    speedCalc = Random.value;
            }
            else if (request == Speed.Slow)
            {
                speedCalc = Random.value * 0.2f;
            }
            else if (request == Speed.Fast)
            {
                speedCalc = 1f - Random.value * 0.2f;
            }
            else
                speedCalc = 0f;

            if (!urgent)
            {
                speedOfChange = GetRandomRange(0.25f);
            }
            _nextSpeedChange = GetNextTimer();

            if (_speedChangerCo != null)
                StopCoroutine(_speedChangerCo);
            _speedChangerCo = StartCoroutine(SpeedChangerCo(_hFlag.speedCalc, speedCalc, speedOfChange));
        }
        //private void ChangeSpeed(bool forced = false, bool fast = false, bool slow = false)
        //{
        //    SensibleH.Logger.LogDebug($"ChangeSpeed");
        //    if (!forced && Random.value < 0.25f)
        //    {
        //        speedWait = GetRandomRange / 2;
        //        return;
        //    }
        //    float targetSpeed;
        //    float speedOfChange;
        //    if (fast)
        //    {
        //        targetSpeed = Random.Range(0.5f, 1f);
        //        speedOfChange = 1f;
        //    }
        //    else if (slow)
        //    {
        //        targetSpeed = 0f;
        //        speedOfChange = Random.Range(1f, 3f);
        //    }
        //    else
        //    {
        //        if (_hFlag.gaugeMale < 20f)
        //            targetSpeed = Random.Range(0f, 0.75f);
        //        else if (_hFlag.gaugeMale > 80f)
        //            targetSpeed = Random.Range(0.25f, 1f);
        //        else
        //            targetSpeed = Random.Range(0f, 1f);

        //        speedOfChange = Random.Range(1f, 8f);
        //    }
        //    if (speedManipulatorCoroutine != null)
        //        StopCoroutine(speedManipulatorCoroutine);

        //    speedManipulatorCoroutine = StartCoroutine(SpeedChangerCo(_hFlag.speedCalc, targetSpeed, speedOfChange));
        //    speedWait = GetRandomRange / 2 + (int)speedOfChange;
        //}

        private IEnumerator SpeedChangerCo(float start, float target, float speedOfChange)
        {
            SensibleH.Logger.LogDebug($"SpeedManipulator {start} {target} {speedOfChange}");
            for (var t = 0f; t < speedOfChange; t += Time.deltaTime)
            {
                if (_hFlag.finish != HFlag.FinishKind.none)
                    yield break;

                _hFlag.speedCalc = Mathf.Lerp(start, target, t / speedOfChange);
                yield return null;
            }
            _hFlag.speedCalc = target;
        }

        private void RestartAction()
        {
            SensibleH.Logger.LogDebug($"RestartAction");
            GetBias();
            _restart = false;
            _hadClimax = false;
            _userInput = true;
            switch (_hFlag.mode)
            {
                case HFlag.EMode.houshi:
                    _hFlag.click = HFlag.ClickKind.again;
                    break;
                case HFlag.EMode.sonyu:
                    if (IsEndOutside)
                        ActionButton();
                    else
                        _hFlag.click = HFlag.ClickKind.speedup;
                    break;
            }

        }
        public void OnUserInput()
        {
            if (_hadClimax && _hFlag.gaugeFemale > 10f && _hFlag.gaugeMale > 10f)
            {
                // In case the user decides to do some actions after orgasm ahead of plugin (or disable it and then re-enable it).
                _hadClimax = false;
            }
            if (_busy && 
                (AutoMode.Value == AutoModeKind.BeginWithPrompt || AutoMode.Value == AutoModeKind.BeginAndProceedWithPrompt))
            {
                SensibleH.Logger.LogDebug("OnUserInput");
                _userInput = true;
                if (IsHoushi)
                    ToggleHoushiAutoMode(true);

            }
        }

        private void ToggleOutsideFinish()
        {
            SensibleH.Logger.LogDebug($"ToggleOutsideFinish");
            switch (_hFlag.mode)
            {
                case HFlag.EMode.houshi:
                    if (_sprite.houshi.tglAutoFinish.isOn)
                        _sprite.houshi.tglAutoFinish.isOn = false;
                    else
                        _sprite.houshi.tglAutoFinish.isOn = true;
                    break;
                case HFlag.EMode.houshi3P:
                    if (_sprite.houshi3P.tglAutoFinish.isOn)
                        _sprite.houshi3P.tglAutoFinish.isOn = false;
                    else
                        _sprite.houshi3P.tglAutoFinish.isOn = true;
                    break;
                case HFlag.EMode.sonyu:
                    if (_sprite.sonyu.tglAutoFinish.isOn)
                        _sprite.sonyu.tglAutoFinish.isOn = false;
                    else
                        _sprite.sonyu.tglAutoFinish.isOn = true;
                    break;
                case HFlag.EMode.sonyu3P:
                    if (_sprite.sonyu3P.tglAutoFinish.isOn)
                        _sprite.sonyu3P.tglAutoFinish.isOn = false;
                    else
                        _sprite.sonyu3P.tglAutoFinish.isOn = true;
                    break;

            }
        }
        private void PickNextAnimation()
        {
            SensibleH.Logger.LogDebug($"PickNextAnimation");
            var choices = GetAvailableAnimations;
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
            yield return null;
            if (_hFlag.isDenialvoiceWait)
                yield break;
            while (!IsIdleOutside)
            {
                SensibleH.Logger.LogDebug($"LoopController[RunAfterPullout]");
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            _method.DynamicInvoke(_args);
        }
        private IEnumerator RunAfterInsert(Action _method, params object[] _args)
        {
            SensibleH.Logger.LogDebug($"RunAfterInsert");
            yield return null;
            if (_hFlag.isDenialvoiceWait)
                yield break;
            var timer = Time.time + 6f;
            while (!IsInsert && timer > Time.time)
            {
                SensibleH.Logger.LogDebug($"LoopController[RunAfterInsert]");
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
            _method.DynamicInvoke(_args);
        }
        //private IEnumerator RunAfterPullOut(Action _method, bool _out = false, params object[] _args)

        private void PickHStats(int _addOrg)
        {
            // We count the amount of orgasms per day per girl and keep it for a day.
            // No clue how to work with kplug though.
            SensibleH.Logger.LogDebug($"PickHStats for {_hFlag.lstHeroine[CurrentMain].Name}");
            if (!LstHeroine.ContainsKey(_hFlag.lstHeroine[CurrentMain].Name))
                LstHeroine.Add(_hFlag.lstHeroine[CurrentMain].Name, 0);

            LstHeroine[_hFlag.lstHeroine[CurrentMain].Name] += _addOrg;
        }
        public void DoSonyuClick(bool pullOut)
        {
            // There is a case when we click the moment animation changes from climax finish to idle, this won't register click.
            if (runAfterCoroutine != null)
            {
                StopCoroutine(runAfterCoroutine);
                runAfterCoroutine = null;
            }
            SensibleH.Logger.LogDebug($"DoSonyuClick");
            if (!pullOut)
                runAfterCoroutine = StartCoroutine(RunAfterInsert(() => _girlController[CurrentMain].StartConvulsion(Random.value)));
            else
                runAfterCoroutine = StartCoroutine(RunAfterPullout(() => _girlController[CurrentMain].StartConvulsion(Random.value)));
        }
        public void DoAnalClick()
        {
            if (_hFlag.isAnalInsertOK)
                DoSonyuClick(pullOut: false);
        }
        public void DoOrgasmF()
        {
            PickHStats(1);
            if (AutoMode.Value == AutoModeKind.BeginAndProceedWithPrompt)
                _busy = true;
            _hadClimax = true;
        }
        public void DoOrgasmM()
        {
            if (AutoMode.Value == AutoModeKind.BeginAndProceedWithPrompt)
                _busy = true;
            _climaxAt = Random.Range(75, 100);
            _hadClimax = true;
            MaleOrgCount += 1;
            ToggleHoushiAutoMode(state: false);
        }
        private void ToggleHoushiAutoMode(bool state)
        {
            switch (_hFlag.mode)
            {
                case HFlag.EMode.houshi:
                    _sprite.houshi.tglRely.Set(state);
                    break;
                case HFlag.EMode.houshi3P:
                    _sprite.houshi3P.tglRely.Set(state);
                    break;
            }
        }
        public void OnDestroy()
        {
            SensibleH.Logger.LogDebug($"LoopController[OnDestroy]");
            Destroy(fakeAnimButton);
        }
    }
}