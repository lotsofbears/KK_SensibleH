using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using static KK_SensibleH.SensibleH;
using System.Collections;
using System.Linq;
using System;
using VRGIN.Core;
using Manager;
using KK_BetterSquirt;
using KK_SensibleH.EyeNeckControl;
using KK_SensibleH.AutoMode;
using KKAPI;
using KKAPI.MainGame;

namespace KK_SensibleH
{
    /// <summary>
    /// Recently Broken:
    /// 
    /// </summary>
    public class GirlController : MonoBehaviour
    {
        /*
         * Opening some clothes triggers some reaction. Just patch "maybe HitReactionPlay()" there. Or throw away. Low value.
         */
        public void Initialize(int main, float familiarity)
        {
            _main = main;
            _chara = _chaControl[_main];
            _familiarity = familiarity;
            SensibleH.Logger.LogDebug($"familiarity[{main}] = [{familiarity}]");
            //_vr = UnityEngine.VR.VRSettings.enabled;
            _vr = VRGIN.Helpers.SteamVRDetector.IsRunning;
            _voiceController = this.gameObject.AddComponent<VoiceController>();
            _voiceController.Initialize(this, main);
            //_neckController = this.gameObject.AddComponent<NewNeckController>();
            _neckController = new NewNeckController();
            _neckController.Initialize(this, main, _vr, familiarity);
            OnPositionChange();
        }
        //private void OnDestroy()
        //{
        //    //They are parented, they'll be destroyed automatically.
        //    foreach (var poi in _listOfMyPoI)
        //        Destroy(poi.gameObject);
        //}
        


        private int _main;
        internal float _familiarity;
        internal int _lastVoice;

        //private bool malePoV;
        //private bool femalePoV;
        private bool _vr;

        private int[] reactions = { 9, 10, 13, 14 };
        private int[] reactionsTop = { 8, 9, 11, 12 };
        private int[] reactionsBottom = { 10, 13, 14 };
        private int[] reactionsFull = { 8, 9, 10, 11, 12, 13, 14 };
        private ChaControl _chara;
        internal NewNeckController _neckController;
        internal VoiceController _voiceController;
        //internal bool IsVoiceActive => _hVoiceCtrl.nowVoices[_main].state == HVoiceCtrl.VoiceKind.voice;
        private bool IsShortActive => _hVoiceCtrl.nowVoices[_main].state == HVoiceCtrl.VoiceKind.breathShort;

        // Assigning neck a new position while it is moving already looks terrible.
        //private void SetVoiceProcTime(float _time)
        //{
        //    switch (_hFlag.mode)
        //    {
        //        case HFlag.EMode.aibu:
        //            _hFlag.voice.timeAibu.timeIdleCalc = _time;
        //            break;
        //        case HFlag.EMode.houshi:
        //            _hFlag.voice.timeHoushi.timeIdleCalc = _time;
        //            break;
        //        case HFlag.EMode.sonyu:
        //            _hFlag.voice.timeSonyu.timeIdleCalc = _time;
        //            break;
        //        case HFlag.EMode.masturbation:
        //            _hFlag.timeMasturbation.timeIdleCalc = _time;
        //            break;
        //        case HFlag.EMode.lesbian:
        //            _hFlag.timeLesbian.timeIdleCalc = _time;
        //            break;
        //    }
        //}
        internal void OnPositionChange()
        {
            _neckController.OnPositionChange();
            // The higher the familiarity, the less abrupt (more relaxed, slow) are neck movements.
            var neckTypes = _chara.neckLookCtrl.neckLookScript.neckTypeStates;
            var speed = (int)(10 * (2 - _familiarity)) * 0.1f;
            SensibleH.Logger.LogDebug($"OnPositionChange[{_main}] leapSpeed {speed}");
            foreach (var type in neckTypes)
            {
                type.leapSpeed = speed;
            }
        }

        internal void Proc()
        {
            _neckController.Proc();
            _voiceController.Proc();
        }
        //private void LookAlive(float _coef, int _main)
        //{
        //    /*
        //     * Nervousness coefficient - people look less in the eyes the more nervous they are
        //     * 
        //     * Eyes peak a random PoI, the neck has a chance to follow in the same direction or after awhile eyes peak a new PoI and so in continues.
        //     * Player can be a PoI too, with appropriate (not)rotation;
        //     * After the new neck pos, Eyes may follow the previous PoI, unless it's UpMid(ceiling isn't a PoI, while UpSide is a "turnAway from player PoI")
        //     * 
        //     * Up positions from corresponding Mid positions only
        //     * 
        //     * LookAtCam, in some way avert the eyes and on it's base turn the neck
        //     */
        //    //Neck turn to poi -> eye turn to poi -> if poi was extreme then mid poi -> player or player outright from beginning 
        //    //Neck turn to poi -> eye turn to poi -> in quick succession dart at player and roll/turn away -> after a bit turn Neck back with eyes away/down
        //    //Neck first or Eye first before turn of neck ?
        //    //EyeFirst
        //    //Look buttom, stop with eyes at mid OR bottom(for "down in the dumps" look)
        //    //Look up, stop with eyes in mid, NO top(looks weird, what's so interesting in ceiling)
        //    //Look side, stop either middle or far
        //    //Use eyes as the pointer to a new neck direction,



        /// <summary>
        /// Interrupt voice and maybe play it later.
        /// </summary>
        /// <param name="_time"></param> Minimal time in the queue.
        //private void CatchVoice(float time = 1f)
        //{
        //    SensibleH.Logger.LogDebug($"CatchVoice");
        //    if (IsVoiceActive)
        //    {
        //        StartCoroutine(RunAfterGasp(() => PlayVoice(_lastVoice), time));
        //        //_hFlag.voice.playVoices[main] = -1;
        //        //_voiceManager.Stop(_hFlag.transVoiceMouth[0]);
        //    }
        //}
        internal void PlayVoice(int voiceId)
        {
            SensibleH.Logger.LogDebug($"GirlController[{_main}] PlayVoices[{voiceId}] was supposed to happen");
            _hFlag.voice.playVoices[_main] = voiceId;
        }
        internal void PlayShort(bool notOverwrite = false)
        {
            _hFlag.voice.playShorts[_main] = Random.Range(0, 9);
            if (notOverwrite)
                _hVoiceCtrl.nowVoices[_main].notOverWrite = true;
            SensibleH.Logger.LogDebug($"GirlController[{_main}] - A Short Gasp has escaped");
        }
        //internal void SupressVoice()
        //{
        //    SensibleH.Logger.LogDebug($"SupressVoice[{main}]");
        //    Voice.Instance.Stop(_hFlag.transVoiceMouth[main]);
        //}

        internal bool SquirtHandler()
        {
            if ((_hFlag.gaugeFemale - 25f) * 0.005f > Random.value)
            {
                OverrideSquirt = true;
                var hand = _main == 0 ? _handCtrl : _handCtrl1;
                BetterSquirtController.RunSquirts(softSE: true, trigger: BetterSquirt.TriggerType.Touch, handCtrl: _main == 0 ? _handCtrl : _handCtrl1);
                OverrideSquirt = false;
                return true;
            }
            else
                return false;
        }
        //public void SupressVoice(int id)
        //{
        //    SensibleH.Logger.LogDebug($"SupressVoice[{main}] - {id}");
        //    StartCoroutine(DoActionWhileTimer(() => StopVoice(id), 1f));
        //}
        private void StopVoice(int id)
        {
            SensibleH.Logger.LogDebug($"StopVoice[{_main}] - {id}");
            if (_voiceController.IsVoiceActive && _hVoiceCtrl.nowVoices[CurrentMain].voiceInfo.id == id)
            {
#if KK
                Manager.Voice.Instance.Stop(_hFlag.transVoiceMouth[0]);
#else
                Manager.Voice.Stop(_hFlag.transVoiceMouth[0]);
#endif
            }
        }
        //private IEnumerator DoActionWhileTimer(Action _action, float time, params object[] _args)
        //{
        //    time += Time.time;
        //    while (time > Time.time)
        //    {
        //        _action.DynamicInvoke(_args);
        //        yield return new WaitForSeconds(0.1f);
        //    }
        //}
        //internal IEnumerator RunAfterGasp(Action _method, float afterGaspWait, params object[] _args)
        //{
        //    int _lastVoice = _hVoiceCtrl.nowVoices[main].voiceInfo.id;
        //    SensibleH.Logger.LogDebug($"RunAfterGasp[{main}]");
        //    while (IsShortActive)
        //    {
        //        yield return new WaitForSeconds(0.1f);
        //    }
        //    afterGaspWait += Time.time;
        //    while (afterGaspWait > Time.time)
        //    {
        //        yield return new WaitForSeconds(0.1f);
        //    }
        //    _method.DynamicInvoke(_args);
        //    SensibleH.Logger.LogDebug($"RunAfterGasp[{main}] [{_method.Method.Name}] was supposed to happen");
        //}
        //internal IEnumerator RunAfterTimer(Action _method, float timer, params object[] _args)
        //{
        //    SensibleH.Logger.LogDebug($"RunAfterTimer[{main}]");
        //    timer += Time.time;
        //    while (timer > Time.time)
        //    {
        //        yield return new WaitForSeconds(0.1f);
        //    }
        //    _method.DynamicInvoke(_args);
        //    SensibleH.Logger.LogDebug($"RunAfterTimer[{main}] [{_method.Method.Name}] was supposed to happen");
        //}
        internal bool Reaction(bool shortPlay = true)
        {
            HandCtrl.AibuColliderKind touchType;
            switch (_hFlag.mode)
            {
                case HFlag.EMode.aibu:
                case HFlag.EMode.houshi:
                    if (_handCtrl.actionUseItem == -1)
                        touchType = (HandCtrl.AibuColliderKind)reactionsFull[Random.Range(0, reactionsFull.Count())];
                    else
                    {
                        switch (_handCtrl.useItems[_handCtrl.actionUseItem].kindTouch)
                        {
                            case HandCtrl.AibuColliderKind.muneL:
                            case HandCtrl.AibuColliderKind.muneR:
                                touchType = (HandCtrl.AibuColliderKind)reactionsTop[Random.Range(0, reactionsTop.Count())];
                                break;
                            default:
                                touchType = (HandCtrl.AibuColliderKind)reactionsBottom[Random.Range(0, reactionsBottom.Count())];
                                break;
                        }
                    }
                    break;
                case HFlag.EMode.sonyu:
                    if (LoopProperties.IsActionLoop)
                        touchType = (HandCtrl.AibuColliderKind)reactionsBottom[Random.Range(0, reactionsBottom.Count())];
                    else
                        touchType = (HandCtrl.AibuColliderKind)reactions[Random.Range(0, reactions.Count())];
                    break;
                default:
                    touchType = (HandCtrl.AibuColliderKind)reactionsFull[Random.Range(0, reactionsBottom.Count())];
                    break;
            }
            if (_main == 0)
                _handCtrl.HitReactionPlay(touchType, shortPlay);
            else
                _handCtrl1.HitReactionPlay(touchType, shortPlay);
            SensibleH.Logger.LogDebug($"GirlController[{_main}] - HitReactionPlay of type {touchType} was supposed to happen");
            return SquirtHandler();
        }
        public void StartConvulsion(float time, bool playVoiceAfter, int specificVoice = -1)
        {
            //SensibleH.Logger.LogDebug($"StartConvulsion[{main}]");
            StartCoroutine(Convulsion(time, playVoice: playVoiceAfter, specificVoice));
        }
        /// <summary>
        /// A set of HitReactionPlay()  with option to play voice afterwards.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Convulsion(float time, bool playVoice, int specificVoice)//, bool big = false)
        {
            //bool cacophony = Random.value < 0.3f;
            //CatchVoice(time / 2f);
            PlayShort();
            time += Time.time;
            while (time > Time.time)
            {
                //CatchVoice(time / 2f);
                if (!IsShortActive && time > 0.5f)
                    PlayShort();

                var seconds = Random.Range(0.1f, 0.5f);

                Reaction(shortPlay: false); //cacophony

                yield return new WaitForSeconds(seconds);
                //_hVoiceCtrl.nowVoices[main].notOverWrite = false;
            }
            SuppressVoice = false;
            if (playVoice)
            {
                if (specificVoice == -1)
                {
                    PlayVoice(_lastVoice);
                }
                else
                    PlayVoice(specificVoice);
            }
        }
        internal void OnVoiceProc()
        {
            _voiceController.OnVoiceProc();

        }
        //public Transform GetPoI(string name, int main)
        //{
        //    Transform transform = null;

        //    // Negative "main" is for dude.
        //    if (main >= 0 && _chaControl[main] != null) 
        //        transform = _chaControl[main].objBodyBone.Descendants()
        //                .Where(t => t.name.Contains(name))
        //                .Select(t => t.transform)
        //                .FirstOrDefault();
        //    else if (main < 0 && _chaControlM != null)
        //        transform = _chaControlM.objBodyBone.Descendants()
        //                .Where(t => t.name.Contains(name))
        //                .Select(t => t.transform)
        //                .FirstOrDefault();
        //    return transform;
        //}


    }
}

/*
 * eyenecks 
 * 330 - eyeCam neckUpLeft +++ NeckRotation
 * 326 - eyeCam neckUpMiddle
 * 307 - eyeCam neckPoseAhead
 * 289 - eyeCam neckStraightAhead
 * 272 - eyeCam neckUpLeft
 * 255 - eyeCam neckMiddleLeft
 * 238 - eyeCam neckLowLeft
 * 221 - eyeCam neckLowMiddle
 * 204 - eyeCam neckLowRight
 * 187 - eyeCam neckMiddleRight
 * 170 - eyeCam neckUpRight
 * 153 - eyeCam neckUpMiddle
 * 136 - eyeCam neckUpFarRight
 * 119 - eyeCam neckLowLeft
 * 35 - turnAway eyeAhead
 * 34 - turnAway eyeOnCam
 * 
 * 17 - eyesToCam nec cam
 * (135)16 - eyeUpFar_RightFar'ish neckPose /Avert EyeLook from PoI UpFar and then roll to the SIDE
 * (134)15 - eyeUpFar_RightFar neckPose /Avert EyeLook from PoI UpFar but stay PoI Side
 * (133)14 - eyeLowFar_Middle neckPose / Towards PoI
 * (132 - (14))13 - eyeMiddle_DirectionFar neckPose / EyesLookAway
 * (131 - (13))?? - eyeAhead neckPose 
 * (130)12 - eyeUpLittle_Left neckPose
 * (129)11 - eyeMiddle_Left neckPose
 * (128)10 - eyeLowLittle_Left neckPose
 * (127)9 - eyeLowLittle_Middle neckPose
 * (126)8 - eyeLowLittle_Right neckPose
 * (125)7 - eyeMiddle_Right
 * (124)6 - eyeUpLittle_Right neckPose
 * (123)5 - eyeUpLittle_Middle neckPose
 * (122)4 - eyeLowMiddle neckPose
 * (121)3 - eyeLowMiddle neckPose
 * (120)2 - eyeAhead neckPose
 * (119)1 - eyeToCam neckPose // Starting one
 * 0 - eyeAhead neckPose
 * 
 * ChaControl.SetTears
 * EyeLookController.target = _.transform
 * 
 * Look to eyeRightBot_neckRightBot -> eyeAhead_neckRightBot -> eyeAhead_neckRightMid -> eyeRightUp_neckRightMid

                //girlController[CurrentMain].PlayVoice(tempCounter);
                //tempCounter += 1;
                // 145 denial ?
                // 110 omocha
                //107 hageshi
                // 108 itai
                // 109 - no ketsu
                // 104 hajimete/mune -> stop
                // 105 kokan ->stop
                // 103 no chuu
 */