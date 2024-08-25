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

namespace KK_SensibleH.EyeNeckControl
{
    internal class NewNeckController
    {
        private DirectionEye _currentEyes;
        private DirectionNeck _currentNeck;

        private GirlController _master;
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
        private bool _neckAfterEvent;
        private bool _vr;
        private bool _camWasClose;
        //private bool _preSetVoiceEyeCam;
        private PoseType _poseType;

        public enum PoseType
        {
            Front,
            Behind,
            Still
        }
        private Dictionary<string, HMotionEyeNeckFemale.EyeNeck> KissDic = new Dictionary<string, HMotionEyeNeckFemale.EyeNeck>();
        private Dictionary<string, HMotionEyeNeckFemale.EyeNeck> OrigDic = new Dictionary<string, HMotionEyeNeckFemale.EyeNeck>();
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
                switch (_hFlag.mode)
                {
                    case HFlag.EMode.aibu:
                        return _hFlag.voice.timeAibu.timeIdle - _hFlag.voice.timeAibu.timeIdleCalc;
                    case HFlag.EMode.houshi:
                        return _hFlag.voice.timeHoushi.timeIdle - _hFlag.voice.timeHoushi.timeIdleCalc;
                    case HFlag.EMode.sonyu:
                        return _hFlag.voice.timeSonyu.timeIdle - _hFlag.voice.timeSonyu.timeIdleCalc;
                    case HFlag.EMode.masturbation:
                        return _hFlag.timeMasturbation.timeIdle - _hFlag.timeMasturbation.timeIdleCalc;
                    //case HFlag.EMode.lesbian:
                    //    return _hFlag.timeLesbian.timeIdle - _hFlag.timeLesbian.timeIdleCalc;
                    default:
                        return 10f;
                }
            }
        }
        private int GetNeckFromCurrentAnimation => _hMotionEyeNeck.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[_main].HExperience] / 17 * 17;
        private int GetCurrentNeck
        {
            get
            {
                int neck; 
                if (_neckActive && EyeNeckPtn[_main] != -1)
                {
                    neck = (EyeNeckPtn[_main] / 17) * 17;
                }
                else if (_hFlag.voice.eyenecks[_main] != -1)
                {
                    neck = (_hFlag.voice.eyenecks[_main] / 17) * 17;
                }
                else
                {
                    neck = GetNeckFromCurrentAnimation;
                }
                return neck;
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
                else if (_hFlag.voice.eyenecks[_main] != -1)
                {

                    if (_hFlag.voice.eyenecks[_main] < 13)
                        return _hFlag.voice.eyenecks[_main] == 0 ? 0 : _hFlag.voice.eyenecks[_main] - 1;
                    else
                        return _hFlag.voice.eyenecks[_main] % 17;
                }
                else
                {
                    return _hMotionEyeNeck.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[_main].HExperience] % 17;
                }
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
                    switch (_hFlag.mode)
                    {
                        case HFlag.EMode.aibu:
                            var animName = _chara.animBody.runtimeAnimatorController.name;
                            if (DontMoveNeckSpecialCases.ContainsKey(animName))
                            {
                                foreach (var firstLetter in DontMoveNeckSpecialCases[animName])
                                {
                                    if (_hFlag.nowAnimStateName.StartsWith(firstLetter, StringComparison.Ordinal))
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        case HFlag.EMode.houshi:
                            if (_hFlag.nowAnimationInfo.kindHoushi == 1)
                                return false;
                            else
                                return true;
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
                switch (_hFlag.mode)
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
        internal void Initialize(GirlController master, int main, bool vr, float familiarity)
        {
            _master = master;
            _main = main;
            _hMotionEyeNeck = main == 0 ? _eyeneckFemale : _eyeneckFemale1;
            _chara = _chaControl[main];
            _familiarity = familiarity;
            _vr = vr;
            _specialNeckMove = new SpecialNeckMovement(master, this, main, vr);
            _poiHandler = new PoiHandler(main);
            if (_vr)
            {
                _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            }
            OnPositionChange();
            LookAtCam(forced: true);
        }
        private void SetProximityDic(bool state)
        {
            SensibleH.Logger.LogDebug($"SetProximityDic");
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
            if (_eyesNextMove < Time.time)
            {
                if (voiceActive && Random.value < 0.4f)
                    _eyesNextMove = Time.time + Random.Range(2f, 5f);
                else
                    PickEyes();
            }

            if (neckMovable && !IsNeckMoving && !_handCtrl.IsKissAction())
            {
                if (_vr)
                {
                    if (IsCamClose)
                    {
                        if (!_camWasClose || IsNeckTimeToMove)
                        {
                            if (FamiliarityCheck())
                            {
                                //SensibleH.Logger.LogDebug($"Neck:Main:Proc:VR:Close:LookAtCam");
                                MoveNeckInit();
                                SetNeckNextMove();
                                SetNeck(GetProperEyeCam);
                                //_neckAfterEvent = true;
                            }
                            else if (_currentNeck == DirectionNeck.Cam && !FamiliarityCheck())
                            {
                                //SensibleH.Logger.LogDebug($"Neck:Main:Proc:VR:Close:LookAway");
                                LookAway();
                            }
                            else
                            {
                                //SensibleH.Logger.LogDebug($"Neck:Main:Proc:VR:Close:LookSomewhere");
                                LookSomewhere();
                            }
                            //_neckAfterEvent = false;
                            _camWasClose = true;
                        }
                        else
                        {
                            //SensibleH.Logger.LogDebug($"Neck:Main:Proc:VR:Close:Busy");
                        }
                        return;
                    }
                    else
                    {
                        //SensibleH.Logger.LogDebug($"Neck:Main:Proc:VR:Far");
                        _camWasClose = false;
                    }
                }
                if (_neckActive)
                {
                    if (!voiceActive)
                    {
                        if (IsNeckTimeToStop)
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
                        else if (!_neckAfterEvent && !IsNeckRecent && GetNextVoiceTime < lookBeforeVoiceTimer)
                        {
                            _neckAfterEvent = true;
                            //_preSetVoiceEyeCam = true;
                            if (Random.value > 0.33f)
                            {
                                if (FamiliarityCheck())
                                {
                                    SensibleH.Logger.LogDebug($"Neck:Main:Proc:PreSetNeck:EyeCam");
                                    SetNeck(GetProperEyeCam);
                                    SetNeckNextMove(GetNextVoiceTime * 0.1f);
                                }
                                else
                                {
                                    SensibleH.Logger.LogDebug($"Neck:Main:Proc:PreSetNeck:SomewhereElse");
                                    LookSomewhere();
                                    //var neck = GetAibuIdleNeckDir(DirectionNeck.Mid);
                                    //SetNeck(neck);
                                    //SetNeckNextMove(0.75f);
                                }
                            }
                            else
                            {
                                // A tad of extra time.
                                SensibleH.Logger.LogDebug($"Neck:Main:Proc:PreSetNeck:Abort");
                            }
                            lookBeforeVoiceTimer = Random.Range(3f, 5f);
                        }
                        else if (IsNeckTimeToMove)
                        {
                            if (_neckAfterEvent)// || _preSetVoiceEyeCam)
                            {
                                //SensibleH.Logger.LogDebug($"Neck:Main:Proc:AttemptToMove:Prolong");
                                _neckAfterEvent = false;
                                SetNeckNextMove(0.1f + Random.value * 0.1f);
                            }
                            else
                            {
                                //SensibleH.Logger.LogDebug($"Neck:Main:Proc:AttemptToMove:Move");
                                FemalePoI[_main] = null;
                                LookSomewhere();
                                //PickEyes();
                            }

                        }
                    }
                    _specialNeckMove.Proc(voiceActive, _currentNeck, _currentEyes);
                   
                }
                
            }
            else if (!neckMovable && _neckActive)
            {
                SensibleH.Logger.LogDebug($"Neck:Main:Proc:Halt:BadState");
                Halt();
            }
        }
        private void SetProperEyeCam()
        {
            _specialNeckMove.SetAuxCamProperParent(GetProperEyeCam);
            FemalePoI[_main] = _specialNeckMove.AuxCam;
        }
        private void LookSomewhere()
        {
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
                    SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:LookAtCam");
                }
                else if (IsAction())
                {
                    newNeck = GetAibuActionDir(curNeck);
                    SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:Asoko/Mune");
                }
                else
                {
                    newNeck = GetAibuIdleNeckDir(curNeck);
                    SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:IdlePose");
                }
            }
            else //if (_poseType == PoseType.Behind)
            {
                newNeck = GetAibuBackDir(curNeck);
                SensibleH.Logger.LogDebug($"Neck:Main:LookSomewhere:BackPose");
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
            switch (_hFlag.mode)
            {
                case HFlag.EMode.aibu:
                    return _hFlag.nowAnimStateName.StartsWith("A", StringComparison.Ordinal) || _hFlag.nowAnimStateName.StartsWith("M", StringComparison.Ordinal);
                case HFlag.EMode.sonyu:
                    return _hFlag.nowAnimStateName.EndsWith("Loop", StringComparison.Ordinal);
                default:
                    return false;
            }

        }
        internal void OnKissVrStart()
        {
            SensibleH.Logger.LogDebug($"Neck:Main:VR:KissStart:{FemalePoI[0]}");
            MoveNeckInit();
            SetProximityDic(true);
            FemalePoI[0] = _specialNeckMove.AuxCam;
            SetNeck(17, quick: true);
            _specialNeckMove.OnKissVrSetAuxCam();
        }
        internal void OnKissVrEnd()
        {
            // At this moment CyuVR changes eyes back and thus changing eyes too soon is a bad idea.

            MoveNeckInit();
            var properEyeCam = GetProperEyeCam;
            _specialNeckMove.SetAuxCamProperParent(properEyeCam);
            FemalePoI[_main] = null;
            _camWasClose = true;
            SetProximityDic(false);
            SetNeck(properEyeCam);
            SetNeckNextMove();
            SetEyesNextMove();
            //moveNeckUntil = Time.time + Random.Range(20f, 40f);
            //lookAtCam = true;
            SensibleH.Logger.LogDebug($"Neck:Main:VR:KissEnd:{FemalePoI[0]}");
        }

        /// <summary>
        /// Currently is being used only to look at adjusted objects for breast and crouch during caress.
        /// </summary>
        internal bool LookAtPoI(int item = -1)
        {
            if (IsNeckMovable && _poiHandler.SetFemalePoI(item))
            {
                SensibleH.Logger.LogDebug($"Neck:Main:LookAtPoi");
                _neckAfterEvent = true;
                if (!_neckActive)
                {
                    MoveNeckInit();
                }
                SetEyesNextMove(1f);
                SetNeckNextMove(0.1f);
                SetNeck(GetProperEyeCam);
                return true;
            }
            else
            {
                SensibleH.Logger.LogDebug($"Neck:Main:LookAtPoi:NoPoi");
                return false;
            }
        }
        public void LookAway()
        {
            if (IsNeckMovable && !_handCtrl.IsKissAction())
            {
                SensibleH.Logger.LogDebug($"Neck:Main:LookAway");
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
                SensibleH.Logger.LogDebug($"Neck:Main:LookAway:Abort");
            }
        }
        /// <summary>
        /// A trigger to turn on/off neck movements, has 80 to 20 ratio;
        /// Arg "forced" only guarantees attempt to trigger.
        /// </summary>
        internal void LookAtCam(bool forced = false)
        {
            //SensibleH.Logger.LogDebug($"LookAtCam {new StackTrace()}");
            if ((!_handCtrl.IsKissAction() && IsNeckMovable) || forced)
            {
                if (_poseType == PoseType.Front && FamiliarityCheck(0.75f))
                {
                    MoveNeckInit();
                    if (!_neckAfterEvent && !IsNeckRecent && !IsNeckMoving)
                    {
                        SensibleH.Logger.LogDebug($"Neck:Main:LookAtCam:EyeCam");
                        SetNeck(GetProperEyeCam);
                    }
                    else
                    {
                        SensibleH.Logger.LogDebug($"Neck:Main:LookAtCam:Extend");
                    }
                }
                else
                {
                    LookAtCamDoNot();
                }
            }
        }
        private void LookAtCamDoNot()
        {
            var curNeck = GetCurrentNeck;
            //if (NeckDirections[curNeck] == DirectionNeck.Cam)
            //    curNeck = GetNeckFromCurrentAnimation;

            // An option to only suppress neck movement during voice with no further movements.
            if (Random.value < 0.2f  / (1f - _familiarity * 0.75f))
            {
                if (!_neckActive)
                {
                    SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:Suppress:{curNeck}");
                    MoveNeckInit(Time.time + 3f);
                }
                else
                {
                    SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:NoAction:{curNeck}");
                }
            }
            else
            {
                SensibleH.Logger.LogDebug($"Neck:Main:DoNotLookAtCam:Extend:{curNeck}");
                MoveNeckInit();
            }
            SetNeck(curNeck + GetCurrentEyes);
        }
        private void MoveNeckInit(float customUntil = 0f)
        {
            if (VRHelper.IsGirlPoV())
            {
                // Not really tested, but my guess it won't be pretty.
                return;
            }
            SensibleH.Logger.LogDebug($"Neck:Main:Initiate:{Time.time}");
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
            SensibleH.Logger.LogDebug($"Neck:Main:Halt:{Time.time}");
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
            SensibleH.Logger.LogDebug($"Neck:Main:SetNeck:{id}:{_currentNeck}:{_eyeCamSaturation}");
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
                SensibleH.Logger.LogDebug($"Neck:Main:PickEyes:EyeCam:{_eyeCamSaturation}");
            }
            else
            {
                //_eyeCamSaturation = Mathf.Clamp01(_eyeCamSaturation - (_familiarity * 0.05f));
                eyes = Random.Range(2, 13); // Anything but former choices
                SensibleH.Logger.LogDebug($"Neck:Main:PickEyes:Mundane:{eyes}:{_eyeCamSaturation}");
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
                _hFlag.voice.eyenecks[_main] = id;
            }
        }
        private void SetPoseType()
        {
            string animName = _chara.animBody.runtimeAnimatorController.name;
            var mode = _hFlag.mode;

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
                SensibleH.Logger.LogDebug($"Neck:Main:PoseType:Unknown:{animName}");
            }
            SensibleH.Logger.LogDebug($"Neck:Main:PoseType:{_poseType}:{animName}");
        }
        internal void OnPositionChange()
        {
            SensibleH.Logger.LogDebug($"Neck:Main:PositionChange");
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
            SetDictionaries();
            SetProximityDic(false);
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
