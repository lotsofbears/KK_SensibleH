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
    public class HeadManipulator : MonoBehaviour
    {
        /*
         * Opening some clothes triggers some reaction. Just patch "maybe HitReactionPlay()" there. Or throw away. Low value.
         */
        public void Initialize(int main, float familiarity)
        {
            _main = main;
            _chara = lstFemale[main];
            _familiarity = familiarity;
            //SensibleH.Logger.LogDebug($"familiarity[{main}] = [{familiarity}]");
            //_vr = UnityEngine.VR.VRSettings.enabled;
            _voiceController = this.gameObject.AddComponent<VoiceController>();
            _voiceController.Initialize(this, main);
            //_neckController = this.gameObject.AddComponent<NewNeckController>();
            _neckController = new NewNeckController();
            _neckController.Initialize(this, main, SensibleHController.IsVR, familiarity);
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

        private readonly int[] reactions = { 9, 10, 13, 14 };
        private readonly int[] reactionsTop = { 8, 9, 11, 12 };
        private readonly int[] reactionsBottom = { 10, 13, 14 };
        private readonly int[] reactionsFull = { 8, 9, 10, 11, 12, 13, 14 };
        private ChaControl _chara;
        internal NewNeckController _neckController;
        internal VoiceController _voiceController;
        private bool IsShortActive => _hVoiceCtrl.nowVoices[_main].state == HVoiceCtrl.VoiceKind.breathShort;

        internal void OnPositionChange()
        {
            _neckController.OnPositionChange();
            // The higher the familiarity, the less abrupt (more relaxed, slow) are neck movements.
            var neckTypes = _chara.neckLookCtrl.neckLookScript.neckTypeStates;
            var speed = (int)(10 * (2 - _familiarity)) * 0.1f;
            //SensibleH.Logger.LogDebug($"OnPositionChange[{_main}] leapSpeed {speed}");
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
        internal void PlayVoice(int voiceId)
        {
            //SensibleH.Logger.LogDebug($"GirlController[{_main}] PlayVoices[{voiceId}] was supposed to happen");
            hFlag.voice.playVoices[_main] = voiceId;
        }
        internal void PlayShort(bool notOverwrite = false)
        {
            hFlag.voice.playShorts[_main] = Random.Range(0, 9);
            if (notOverwrite)
                _hVoiceCtrl.nowVoices[_main].notOverWrite = true;
            //SensibleH.Logger.LogDebug($"GirlController[{_main}] - A Short Gasp has escaped");
        }

        internal bool SquirtHandler()
        {
            if ((hFlag.gaugeFemale - 25f) * 0.005f > Random.value)
            {
                OverrideSquirt = true;
                var hand = _main == 0 ? handCtrl : handCtrl1;
                BetterSquirtController.RunSquirts(softSE: true, trigger: BetterSquirt.TriggerType.Touch, handCtrl: _main == 0 ? handCtrl : handCtrl1);
                OverrideSquirt = false;
                return true;
            }
            else
                return false;
        }
        private void StopVoice(int id)
        {
            //SensibleH.Logger.LogDebug($"StopVoice[{_main}] - {id}");
            if (_voiceController.IsVoiceActive && _hVoiceCtrl.nowVoices[CurrentMain].voiceInfo.id == id)
            {
#if KK
                Manager.Voice.Instance.Stop(hFlag.transVoiceMouth[0]);
#else
                Manager.Voice.Stop(hFlag.transVoiceMouth[0]);
#endif
            }
        }
        internal bool Reaction(bool shortPlay = true)
        {
            HandCtrl.AibuColliderKind touchType;
            switch (hFlag.mode)
            {
                case HFlag.EMode.aibu:
                case HFlag.EMode.houshi:
                    if (handCtrl.actionUseItem == -1)
                        touchType = (HandCtrl.AibuColliderKind)reactionsFull[Random.Range(0, reactionsFull.Count())];
                    else
                    {
                        switch (handCtrl.useItems[handCtrl.actionUseItem].kindTouch)
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
                handCtrl.HitReactionPlay(touchType, shortPlay);
            else
                handCtrl1.HitReactionPlay(touchType, shortPlay);
            //SensibleH.Logger.LogDebug($"GirlController[{_main}] - HitReactionPlay of type {touchType} was supposed to happen");
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