using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using VRGIN.Core;
using static KK_SensibleH.SensibleH;
using static KK_SensibleH.EyeNeckControl.EyeNeckDictionaries;
using System.Diagnostics;
using Illusion.Extensions;
using KKAPI;
using KKAPI.MainGame;
using static KK_SensibleH.EyeNeckControl.NewNeckController;
using KK_SensibleH.Caress;
using KK_SensibleH.AutoMode;

namespace KK_SensibleH.EyeNeckControl
{
    internal class NewNeckController
    {
        private DirectionEye _currentEyes;
        private DirectionNeck _currentNeck;

        private HeadManipulator _master;
        private SpecialNeckMovement _specialNeckMove;
        private PoiHandler _poiHandler;

        private ChaControl _chara;
        private Transform _eyes;
        private HMotionEyeNeckFemale _hMotionEyeNeck;

        private int _main;
        private float _familiarity;
        internal float _neckMoveUntil;
        internal float _neckNextMove;
        private float _eyesNextMove;
        private float lookBeforeVoiceTimer = 2f;
        private float _eyeCamSaturation;

        internal bool _neckActive;
        //private bool lookPoi;
        //private bool lookAway;
        private bool _neckBusy;
        private bool _neckAfterEvent;
        private bool _camWasClose;
        //private bool _preSetVoiceEyeCam;
        private PoseType _poseType;

        public enum PoseType
        {
            Front,
            Behind,
            Still
        }
        private Dictionary<string, HMotionEyeNeckFemale.EyeNeck> KissDic = [];
        private Dictionary<string, HMotionEyeNeckFemale.EyeNeck> OrigDic = [];
        internal bool IsNeckRecent => _neckNextMove - Time.time > 6f;
        private bool IsNeckTimeToMove => _neckNextMove < Time.time;
        private bool IsNeckTimeToStop => _neckMoveUntil < Time.time; 
        private bool IsNeckMoving => _chara.neckLookCtrl.neckLookScript.changeTypeLeapTime != _chara.neckLookCtrl.neckLookScript.changeTypeTimer;
        private void SetNeckNextMove(float multiplier = 1f) => _neckNextMove = Time.time + 10f * multiplier;
        private void SetEyesNextMove(float multiplier = 1f) => _eyesNextMove = Time.time + (2f + Random.value * 3f) * multiplier;
        private bool FamiliarityCheck(float multiplier = 1f) => _familiarity * multiplier - _eyeCamSaturation > Random.value;
        private float GetNextVoiceTime
        {
            get
            {
                return hFlag.mode switch
                {
                    HFlag.EMode.aibu => hFlag.voice.timeAibu.timeIdle - hFlag.voice.timeAibu.timeIdleCalc,
                    HFlag.EMode.houshi => hFlag.voice.timeHoushi.timeIdle - hFlag.voice.timeHoushi.timeIdleCalc,
                    HFlag.EMode.sonyu => hFlag.voice.timeSonyu.timeIdle - hFlag.voice.timeSonyu.timeIdleCalc,
                    HFlag.EMode.masturbation => hFlag.timeMasturbation.timeIdle - hFlag.timeMasturbation.timeIdleCalc,
                    //case HFlag.EMode.lesbian:
                    //    return hFlag.timeLesbian.timeIdle - hFlag.timeLesbian.timeIdleCalc;
                    _ => 10f,
                };
            }
        }
        private int GetNeckFromCurrentAnimation => _hMotionEyeNeck.dicEyeNeck[hFlag.nowAnimStateName].idEyeNecks[(int)hFlag.lstHeroine[_main].HExperience] / 17 * 17;
        private int GetCurrentNeck
        {
            get
            {
                if (_neckActive && EyeNeckPtn[_main] != -1)
                {
                    return (EyeNeckPtn[_main] / 17) * 17;
                }
                else if (hFlag.voice.eyenecks[_main] != -1)
                {
                    return (hFlag.voice.eyenecks[_main] / 17) * 17;
                }
                return GetNeckFromCurrentAnimation;
            }
        }
        private int GetCurrentEyes
        {
            get
            {
                if (_neckActive && EyeNeckPtn[_main] != -1)
                {
                    if (EyeNeckPtn[_main] < 13)
                        return EyeNeckPtn[_main] == 0 ? 0 : EyeNeckPtn[_main] - 1;
                    else
                        return EyeNeckPtn[_main] % 17;
                }
                else if (hFlag.voice.eyenecks[_main] != -1)
                {

                    if (hFlag.voice.eyenecks[_main] < 13)
                        return hFlag.voice.eyenecks[_main] == 0 ? 0 : hFlag.voice.eyenecks[_main] - 1;
                    else
                        return hFlag.voice.eyenecks[_main] % 17;
                }
                return _hMotionEyeNeck.dicEyeNeck[hFlag.nowAnimStateName].idEyeNecks[(int)hFlag.lstHeroine[_main].HExperience] % 17;
                
            }
        }
        private bool IsNeckMovable
        {
            get
            {
                if (_poseType == PoseType.Still)
                {
                    return false;
                }
                else
                {
                    switch (hFlag.mode)
                    {
                        case HFlag.EMode.aibu:
                            var controller = _chara.animBody.runtimeAnimatorController.name;
                            if (DontMoveNeckSpecialCases.ContainsKey(controller))
                            {
                                foreach (var firstLetter in DontMoveNeckSpecialCases[controller])
                                {
                                    if (hFlag.nowAnimStateName.StartsWith(firstLetter, StringComparison.Ordinal))
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        case HFlag.EMode.houshi:
                            return hFlag.nowAnimationInfo.kindHoushi != 1;
                        case HFlag.EMode.sonyu:
                        case HFlag.EMode.lesbian:
                        case HFlag.EMode.sonyu3P:
                            return true;
                        case HFlag.EMode.masturbation: // Has nice neck by default.
                        case HFlag.EMode.peeping:
                        case HFlag.EMode.houshi3P:
                            return false;
                        default:
                            return false;
                    }
                }
            }
        }
        internal int GetProperEyeCam
        {
            // By default:
            // 17 - camera
            // 51 - partners head
            // 85 - partners kokan
            get
            {
                switch (hFlag.mode)
                {
                    case HFlag.EMode.aibu:
                        return 17;
                    default:
                        return 51;
                }
            }
        }
        private bool _wasEyeContact;
        private bool GetEyeContact => Vector3.Angle(_eyes.position - VR.Camera.Head.position, VR.Camera.Head.forward) < 30f;
        private bool IsCamClose => Vector3.Distance(_eyes.position, VR.Camera.Head.position) < 0.4f;
        internal void Initialize(HeadManipulator master, int main, bool vr, float familiarity)
        {
            _master = master;
            _main = main;
            _hMotionEyeNeck = main == 0 ? _eyeneckFemale : _eyeneckFemale1;
            _chara = lstFemale[main];
            _familiarity = familiarity;
            _specialNeckMove = new SpecialNeckMovement(master, this, main, vr);
            _poiHandler = new PoiHandler(main);
            if (SensibleHController.IsVR)
            {
                _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            }
            OnPositionChange();
            LookAtCam(forced: true);
        }
        private void SetProximityDic(bool state)
        {
            //SensibleH.Logger.LogDebug($"SetProximityDic");
            if (KissDic.Count == 0)
            {
                SetDictionaries();
            }
            if (state)
            {
                _hMotionEyeNeck.dicEyeNeck = KissDic;
            }
            else
            {
                _hMotionEyeNeck.dicEyeNeck = OrigDic;
            }
        }
        internal void Proc()
        {
            var voiceActive = _master._voiceController.IsVoiceActive;
            var neckMovable = IsNeckMovable;
            var neckMoving = IsNeckMoving;
            if (_eyesNextMove < Time.time)
            {
                if (voiceActive && Random.value < 0.4f)
                    SetEyesNextMove();
                else
                    PickEyes();
            }
            if (neckMovable && !neckMoving && !LoopProperties.IsKissLoop) // handCtrl.IsKissAction())
            {
                if (SensibleHController.IsVR)
                {
                    if (IsCamClose)
                    {
                        if (!_camWasClose || IsNeckTimeToMove)
                        {
                            if (_neckAfterEvent)
                            {
                                _neckAfterEvent = false;
                                SetNeckNextMove(0.1f + Random.value * 0.1f);
                            }
                            else
                            {
                                if ( _poseType == PoseType.Front && FamiliarityCheck())
                                {
                                    MoveNeckInit();
                                    SetNeckNextMove();
                                    FemalePoI[_main] = null;
                                    SetNeck(GetProperEyeCam);
                                    //_neckAfterEvent = true;
                                }

                                // Native one has rather poor implementation.
                                //else if (_currentNeck == DirectionNeck.Cam && !FamiliarityCheck())
                                //{
                                //    //SensibleH.Logger.LogDebug($"Neck:Main:Proc:VR:Close:LookAway");
                                //    LookAway();
                                //}
                                else
                                {
                                    //SensibleH.Logger.LogDebug($"Neck:Main:Proc:VR:Close:LookSomewhere");
                                    FemalePoI[_main] = null;
                                    LookSomewhere();
                                }
                                _camWasClose = true;
                            }
                            return;
                        }
                    }
                    else
                    {
                        _camWasClose = false;
                    }
                }
                else
                {
                    if (handCtrl.IsKissAction())
                    {
                        if (_neckActive)
                        {
                            Halt();
                        }
                        return;
                    }
                }
                if (_neckActive)
                {
                    //if (SwapStaticNeck()) return;
                    if (!_neckBusy)
                    {
                        if (IsNeckTimeToMove)
                        {
                            if (_neckAfterEvent)// || _preSetVoiceEyeCam)
                            {
                                //SensibleH.Logger.LogDebug($"Neck:Main:Proc:AttemptToMove:Prolong");
                                _neckAfterEvent = false;
                                SetNeckNextMove(0.1f + Random.value * 0.1f);
                            }
                            else
                            {
                                FemalePoI[_main] = null;
                                if (_currentNeck != DirectionNeck.Cam || !voiceActive) LookSomewhere();
                            }
                            return;
                        }
                        else if (!voiceActive)
                        {
                            if (!_neckAfterEvent && !IsNeckRecent && GetNextVoiceTime < lookBeforeVoiceTimer)
                            {
                                //_preSetVoiceEyeCam = true;
                                if (Random.value > 0.33f)
                                {
                                    if (FamiliarityCheck())
                                    {
                                        //SensibleH.Logger.LogDebug($"Neck:Main:Proc:PreSetNeck:EyeCam");
                                        SetNeck(GetProperEyeCam);
                                        SetNeckNextMove(GetNextVoiceTime * 0.1f);
                                    }
                                    else
                                    {
                                        //SensibleH.Logger.LogDebug($"Neck:Main:Proc:PreSetNeck:SomewhereElse");
                                        LookSomewhere();
                                        //var neck = GetAibuIdleNeckDir(DirectionNeck.Mid);
                                        //SetNeck(neck);
                                        //SetNeckNextMove(0.75f);
                                    }
                                }
                                else
                                {
                                    _neckBusy = true;
                                    //SensibleH.Logger.LogDebug($"Neck:Main:Proc:PreSetNeck:Abort");
                                }
                                _neckAfterEvent = true;
                                lookBeforeVoiceTimer = Random.Range(3f, 5f);
                            }
                            else if (IsNeckTimeToStop)
                            {
                                if (_neckMoveUntil < _neckNextMove)
                                {
                                    _neckMoveUntil = _neckNextMove;
                                    //SensibleH.Logger.LogDebug($"Neck:Main:Proc:AttemptToHalt:ExtendTimeBecauseNeck");
                                }
                                else if (GetNextVoiceTime < 5f)
                                {
                                    //SensibleH.Logger.LogDebug($"Neck:Main:Proc:AttemptToHalt:ExtendTimeBecauseVoice");
                                    _neckMoveUntil = Time.time + 5f;
                                }
                                else
                                {
                                    //SensibleH.Logger.LogDebug($"Neck:Main:Proc:AttemptToHalt:Halt");
                                    Halt();
                                }
                            }
                        }
                        _specialNeckMove.Proc(voiceActive, _currentNeck, _currentEyes);
                    }
                }
                
            }
            else if (!neckMovable && _neckActive && !neckMoving && (!SensibleHController.IsVR || !IsCamClose))
            {
                //SensibleH.Logger.LogDebug($"Neck:Main:Proc:Halt:BadState");
                Halt();
            }
        }
        private bool SwapStaticNeck()
        {
            if (_currentNeck == DirectionNeck.Pose)
            {
                _specialNeckMove.SetAuxCamForStaticNeck();
                _neckAfterEvent = true;
                FemalePoI[_main] = _specialNeckMove.AuxCam;
                SetNeck(17);
                return true;
            }
            else
            {
                return false;
            }
        }
        private void SetProperEyeCam()
        {
            //SensibleH.Logger.LogDebug($"Neck:Main:ProperEyeCam:Set");
            _specialNeckMove.SetAuxCamProperParent(GetProperEyeCam);
            FemalePoI[_main] = _specialNeckMove.AuxCam;
        }
        private void LookSomewhere()
        {

            //SensibleH.Logger.LogDebug($"Neck:LookSomewhere:PoseType = {_poseType}");
            //var camChance = _familiarity * 0.75f;
            var curNeck = GetNeckDirection(GetCurrentNeck);
            var curEyes = GetCurrentEyes;
            List<DirectionNeck> newNeck = new List<DirectionNeck>();
            if (_poseType == PoseType.Front)
            {
                if (FamiliarityCheck(0.75f))
                {
                    // 0.2 chance for absolute virgin to look at cam
                    // 0.75 for lewd state with maxed out intimacy.
                    SetNeck(GetProperEyeCam);
                    //SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:LookAtCam");
                }
                else if (IsAction())
                {
                    newNeck = GetAibuActionDir(curNeck);
                    //SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:Asoko/Mune");
                }
                else
                {
                    newNeck = GetAibuIdleNeckDir(curNeck);
                    //SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:IdlePose");
                }
            }
            else //if (_poseType == PoseType.Behind)
            {
                newNeck = GetAibuBackDir(curNeck);
               // //SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:BackPose");
            }
            if (newNeck.Count > 0)
            {
                // Should not happen no more.

                SetNeck(newNeck);
                SetEyesNextMove();
            }
            SetNeckNextMove();
        }
        private bool IsAction()
        {
            switch (hFlag.mode)
            {
                case HFlag.EMode.aibu:
                    return hFlag.nowAnimStateName.StartsWith("A", StringComparison.Ordinal) || hFlag.nowAnimStateName.StartsWith("M", StringComparison.Ordinal);
                case HFlag.EMode.sonyu:
                    return hFlag.nowAnimStateName.EndsWith("Loop", StringComparison.Ordinal);
                default:
                    return false;
            }

        }
        internal void OnKissVrStart()
        {
            MoveNeckInit();
            //SetProximityDic(true);
            FemalePoI[0] = _specialNeckMove.AuxCam;
            SetNeck(17, quick: true);
            _specialNeckMove.OnKissVrSetAuxCam();
            //SensibleH.Logger.LogDebug($"Neck:Main:VR:KissStart:{FemalePoI[0]}");
        }
        internal void OnKissVrEnd()
        {
            // At this moment CyuVR changes eyes back and thus changing eyes too soon is a bad idea.

            MoveNeckInit();
            var properEyeCam = GetProperEyeCam;
            _specialNeckMove.SetAuxCamProperParent(properEyeCam);
            FemalePoI[_main] = null;
            _camWasClose = true;
            //SetProximityDic(false);
            SetNeck(properEyeCam);
            SetNeckNextMove();
            SetEyesNextMove();
            //moveNeckUntil = Time.time + Random.Range(20f, 40f);
            //lookAtCam = true;
            //SensibleH.Logger.LogDebug($"Neck:Main:VR:KissEnd:{FemalePoI[0]}");
        }

        /// <summary>
        /// Currently is being used only to look at adjusted objects for breast and crouch during caress.
        /// </summary>
        internal bool LookAtPoI(int item = -1)
        {
            if (IsNeckMovable && !handCtrl.IsKissAction() && _poiHandler.SetFemalePoI(item))
            {
                //SensibleH.Logger.LogDebug($"Neck:Main:LookAtPoi");
                _neckAfterEvent = true;
                if (!_neckActive)
                {
                    MoveNeckInit();
                }
                SetEyesNextMove(1f);
                //SetNeckNextMove(0.1f);
                SetNeck(GetProperEyeCam);
                return true;
            }
            else
            {
                //SensibleH.Logger.LogDebug($"Neck:Main:LookAtPoi:NoPoi");
                return false;
            }
        }
        public void LookAway()
        {
            if (IsNeckMovable && !handCtrl.IsKissAction())
            {
                //SensibleH.Logger.LogDebug($"Neck:Main:LookAway");
                _neckAfterEvent = true;

                // TODO Remake dic into method
                var neckChoice = SpecialNeckDirections.ElementAt(Random.Range(0, SpecialNeckDirections.Count)).Key;
                if (!_neckActive)
                {
                    MoveNeckInit();
                }
                SetEyesNextMove(0.5f);
                SetNeckNextMove(0.1f);
                SetNeck(Random.Range(13, 17) + neckChoice, quick: true);
            }
            else
            {
                //SensibleH.Logger.LogDebug($"Neck:Main:LookAway:Abort");
            }
        }
        /// <summary>
        /// A trigger to turn on/off neck movements, has 80 to 20 ratio;
        /// Arg "forced" only guarantees an attempt to trigger.
        /// </summary>
        internal void LookAtCam(bool forced = false)
        {
            //SensibleH.Logger.LogDebug($"LookAtCam {new StackTrace()}");
            if ((!LoopProperties.IsKissLoop && IsNeckMovable) || forced)
            {
                if (!_neckBusy && _poseType == PoseType.Front && FamiliarityCheck(0.75f))
                {
                    MoveNeckInit();
                    if (!_neckAfterEvent && !IsNeckRecent && !IsNeckMoving)
                    {
                        //SensibleH.Logger.LogDebug($"Neck:Main:LookAtCam:EyeCam");
                        SetNeck(GetProperEyeCam);
                    }
                    else
                    {
                        //SensibleH.Logger.LogDebug($"Neck:Main:LookAtCam:Extend");
                    }
                }
                else
                {
                    LookAtCamDoNot();
                }
            }
            _neckBusy = false;
        }
        private void LookAtCamDoNot()
        {
            var curNeck = GetCurrentNeck;
            //if (NeckDirections[curNeck] == DirectionNeck.Cam)
            //    curNeck = GetNeckFromCurrentAnimation;

            // An option to only suppress neck movement during voice with no further movements.
            if (!_neckActive)
            {
                if (Random.value < 0.2f / (1f - _familiarity * 0.75f))
                {
                    //SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:Suppress:{curNeck}");
                    MoveNeckInit(Time.time + 3f);
                }
                else
                {
                    //SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:NoAction:{curNeck}");
                }
            }
            else
            {
                //SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:Extend:{curNeck}");
                MoveNeckInit();
            }

            //if (Random.value < 0.2f  / (1f - _familiarity * 0.75f))
            //{
            //    if (!_neckActive)
            //    {
            //        //SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:Suppress:{curNeck}");
            //        MoveNeckInit(Time.time + 3f);
            //    }
            //    else
            //    {
            //        //SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:NoAction:{curNeck}");
            //    }
            //}
            //else
            //{
            //    if (_neckActive)
            //    {
            //        //SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:Extend:{curNeck}");
            //        MoveNeckInit();
            //    }
            //}
            SetNeck(curNeck + GetCurrentEyes);
        }
        private void MoveNeckInit(float customUntil = 0f)
        {
            if (KK_VR.Features.PoV.Active && KK_VR.Features.PoV.GirlPoV)
            {
                // Not really tested, but my guess it won't be pretty.
                return;
            }
            //SensibleH.Logger.LogDebug($"Neck:Main:Initiate:{Time.time}");
            _neckActive = true;
            MoveNeckGlobal = true;
            if (customUntil == 0f)
            {
                _neckMoveUntil = Time.time + Random.Range(40, 60);
            }
            else
            {
                _neckMoveUntil = customUntil;
            }
        }
        /// <summary>
        /// Unconditionally stops all non-native neck movements.
        /// </summary>
        public void Halt()
        {
            //SensibleH.Logger.LogDebug($"Neck:Main:Halt:{Time.time}");
            FemalePoI[_main] = null;
            EyeNeckPtn[_main] = -1;
            _neckActive = false;
            //lookAway = false;
            _neckAfterEvent = false;
            //_preSetVoiceEyeCam = false;
            //lookPoi = false;
            _camWasClose = false;
            _eyeCamSaturation = 0f;
            IsNeckSet[_main] = false;
        }
        private void SetNeck(DirectionNeck direction, bool quick = false)
        {
            var id = GetNeckDirection(direction) + GetCurrentEyes;
            SetNeck(id, quick);
        }
        private void SetNeck(List<DirectionNeck> directions, bool quick = false)
        {
            var direction = directions.ElementAt(Random.Range(0, directions.Count));
            SetNeck(direction, quick);
        }
        private void SetNeck(int id, bool quick = false)
        {
            var speedOfChange = 3f;
            if (quick)
            {
                speedOfChange = 1f;
            }
            _chara.neckLookCtrl.neckLookScript.changeTypeLeapTime = speedOfChange;
            _chara.neckLookCtrl.neckLookScript.changeTypeTimer = speedOfChange;
            EyeNeckPtn[_main] = id;
            IsNeckSet[_main] = false;
            _currentNeck = GetNeckDirection(GetCurrentNeck);
            if (!_neckAfterEvent)
            {
                if (_currentNeck == DirectionNeck.Cam)
                {
                    _eyeCamSaturation += _familiarity * 0.2f;
                    SetProperEyeCam();
                }
                else
                {
                    _eyeCamSaturation = 0f;
                }
            }
            
            _specialNeckMove.SetCooldown();
            //SensibleH.Logger.LogDebug($"Neck:Main:SetNeck:{_currentNeck}:{_eyeCamSaturation}");
        }
        private void PickEyes()
        {
            int eyes;
            int currentEyes = GetCurrentEyes;

            //if (!_master._voiceController.IsVoiceActive && currentEyes == 0 && Random.value < 0.2f && (CurrentNeck == DirectionNeck.Cam || CurrentNeck == DirectionNeck.Mid || CurrentNeck == DirectionNeck.Pose)) // Dart away
            //    eyes = Random.Range(13, 15);
            if (currentEyes != 0 && FamiliarityCheck(0.75f)) // EyeCam
            {
                _eyeCamSaturation = Mathf.Clamp01(_eyeCamSaturation + (_familiarity * 0.03f));
                eyes = 1;
                //SensibleH.Logger.LogDebug($"Neck:Main:PickEyes:EyeCam:{_eyeCamSaturation}");
            }
            else
            {
                //_eyeCamSaturation = Mathf.Clamp01(_eyeCamSaturation - (_familiarity * 0.05f));
                eyes = Random.Range(2, 13); // Anything but former choices
                //SensibleH.Logger.LogDebug($"Neck:Main:PickEyes:Mundane:{eyes}:{_eyeCamSaturation}");
            }

            _currentEyes = EyeDirections[eyes];
            var curNeck = GetCurrentNeck;

            if (_neckActive)
            {
                if (EyeNeckPtn[_main] >= 17)
                {
                    eyes += (curNeck - 1);
                }
            }
            else if (_master._voiceController.IsVoiceActive)
            {
                if (curNeck >= 17)
                {
                    eyes += (curNeck - 1);
                }
            }
            else
            {
                var curAnimNeck = GetNeckFromCurrentAnimation;
                if (curAnimNeck >= 17)
                {
                    eyes += (curAnimNeck - 1);
                }
            }

            SetEyes(eyes);
            SetEyesNextMove();
        }
        private void SetEyes(int id)
        {
            if (_neckActive)
            {
                EyeNeckPtn[_main] = id;
            }
            else
            {
                hFlag.voice.eyenecks[_main] = id;
            }
        }
        private void SetPoseType()
        {
            string animName = _chara.animBody.runtimeAnimatorController.name;
            var mode = hFlag.mode;

            if (DontMoveAnimations.ContainsKey(mode) && DontMoveAnimations[mode].Contains(animName))
            {
                _poseType = PoseType.Still;
            }
            else if (FrontAnimations.ContainsKey(mode) && FrontAnimations[mode].Contains(animName))
            {
                _poseType = PoseType.Front;
            }
            else if (BackAnimations.ContainsKey(mode) && BackAnimations[mode].Contains(animName))
            {
                _poseType = PoseType.Behind;
            }
            else
            {
                _poseType = PoseType.Still;
                //SensibleH.Logger.LogDebug($"Neck:Main:PoseType:Unknown:{animName}");
            }
            //SensibleH.Logger.LogDebug($"Neck:Main:PoseType:{_poseType}:{animName}");
        }
        internal void OnPositionChange()
        {
            //SensibleH.Logger.LogDebug($"Neck:Main:PositionChange");
            Halt();
            SetPoseType();
            _specialNeckMove.ResetAuxCam();
            if (SensibleH.EyeNeckControl.Value)
            {
                LookAtCam();
            }
#if KK
            var dic = _hMotionEyeNeck.dicInfo;
#else
            var dic = _hMotionEyeNeck.DicInfo;
#endif
            foreach (var kvPair in dic.Values)
            {
                // Not sure if this one actually has any effect at all.
                kvPair.rateNeck = 3f;
            }
            //SetDictionaries();
            //SetProximityDic(false);
        }
        private void SetDictionaries()
        {
            OrigDic = _hMotionEyeNeck.dicEyeNeck;
            KissDic.Clear();
            KissDic = OrigDic.DeepCopy();
            foreach (var kvPair in KissDic.Values)
            {
                kvPair.rangeNeck.up = -45f;
                kvPair.rangeNeck.down = 45f;
                kvPair.rangeNeck.left = -45f;
                kvPair.rangeNeck.right = 45f;
                kvPair.rangeFace.up = -45f;
                kvPair.rangeFace.down = 45f;
                kvPair.rangeFace.left = -45f;
                kvPair.rangeFace.right = 45f;
            }
        }
    }
}
