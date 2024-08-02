using ActionGame.Chara.Mover;
using ADV.Commands.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Random = UnityEngine.Random;
using UnityEngine;
using static KK_SensibleH.SensibleH;
using static KK_SensibleH.EyeNeckControl.EyeNeckDictionaries;
using VRGIN.Core;
using System.Runtime.InteropServices;
using KoikatuVR;

namespace KK_SensibleH.EyeNeckControl
{
    public class EyeNeckController :MonoBehaviour
    {
        // Features Proper dartAway with check for angle;

        // Broken PreVoiceEyeCam (checks continuously)
        internal void Initialize(GirlController master, int main, bool vr, float familiarity)
        {
            _master = master;
            _main = main;
            _chara = _chaControl[main];
            _familiarity = familiarity;
            _vr = vr;
            _specialNeckMove = new SpecialNeckMovement(master, this, main, vr);
            _poiHandler = new PoiHandler(main);
            if (_vr)
            {
                _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            }
            GetPoseType();
            LookAtCam(forced: true);
        }

        public DirectionEye CurrentEyes;
        public DirectionNeck CurrentNeck;
        private GirlController _master;
        private SpecialNeckMovement _specialNeckMove;
        private PoiHandler _poiHandler;
        private ChaControl _chara;
        private Transform _eyes;

        private int _main;
        private float _familiarity;
        internal float moveNeckUntil;
        internal float _neckNextMove;
        private float _eyesNextMove;
        private float lookBeforeVoiceTimer = 2f;

        internal bool _neckActive;
        private bool lookPoi;
        private bool lookAway;
        private bool lookAtCam;
        private bool _vr;
        private bool _camWasClose;
        private PoseType _poseType;
        public enum DirectionEye
        {
            Cam,
            Away,
            PoiUp,
            PoiDown,
            PoiRollAway,
            UpMid,
            UpRight,
            UpLeft,
            Mid,
            MidRight,
            MidLeft,
            DownMid,
            DownDownMid,
            DownRight,
            DownLeft,
            Pose
        }
        public enum DirectionNeck
        {
            Pose,
            Cam,
            Away,
            UpMid,
            UpRight,
            UpRightFar,
            UpLeft,
            Mid,
            MidRight,
            MidLeft,
            DownMid,
            DownRight,
            DownLeft,
            DownDownLeft
        }
        enum PoseType
        {
            Front,
            Behind,
            Still
        }
        internal bool IsNeckRecent => _neckNextMove - Time.time > 5f;
        private bool IsNeckTimeToMove => _neckNextMove < Time.time;
        private bool IsNeckTimeToStop => moveNeckUntil < Time.time;
        private bool IsNeckMoving => _chara.neckLookCtrl.neckLookScript.changeTypeLeapTime - _chara.neckLookCtrl.neckLookScript.changeTypeTimer != 0;

        // Seems to be enough with all the extras that fixational movement adds.
        private void SetNeckNextMove(float multiplier = 1f) => _neckNextMove = Time.time + 10f * multiplier;
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
        private int GetNeckFromCurrentAnimation => _main == 0 ? _eyeneckFemale.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[_main].HExperience] / 17 * 17 :
            _eyeneckFemale1.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[_main].HExperience] / 17 * 17;

        private int GetCurrentNeck
        {
            get
            {
                if (_neckActive && EyeNeckPtn[_main] != -1)
                {
                    return (EyeNeckPtn[_main] / 17) * 17;
                }
                else if (_hFlag.voice.eyenecks[_main] != -1)
                {
                    return (_hFlag.voice.eyenecks[_main] / 17) * 17;
                }
                else
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
                else if (_hFlag.voice.eyenecks[_main] != -1)
                {

                    if (_hFlag.voice.eyenecks[_main] < 13)
                        return _hFlag.voice.eyenecks[_main] == 0 ? 0 : _hFlag.voice.eyenecks[_main] - 1;
                    else
                        return _hFlag.voice.eyenecks[_main] % 17;
                }
                else
                {
                    if (_main == 0)
                        return _eyeneckFemale.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[_main].HExperience] % 17;
                    else
                        return _eyeneckFemale1.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[_main].HExperience] % 17;
                }
            }
        }
        private bool IsNeckMovable
        {
            get
            {
                if (_poseType == PoseType.Still)
                    return false;
                else
                {
                    switch (_hFlag.mode)
                    {
                        case HFlag.EMode.aibu:
                            var animName = _chara.animBody.runtimeAnimatorController.name;
                            if (DontMoveNeckSpecialCases.ContainsKey(animName))
                            {
                                foreach (var neck in DontMoveNeckSpecialCases[animName])
                                {
                                    if (_hFlag.nowAnimStateName.StartsWith(neck, StringComparison.Ordinal))
                                        return false;
                                }
                                //for (int i = 0; i < DontMoveNeckSpecialCases[animName].Count; i++)
                                //{
                                //    if (_hFlag.nowAnimStateName.StartsWith(DontMoveNeckSpecialCases[animName].ElementAt(i), StringComparison.Ordinal))
                                //        return false;
                                //}
                            }
                            return true;
                        case HFlag.EMode.houshi:
                            if (_hFlag.nowAnimationInfo.kindHoushi == 1)
                                return false;
                            else
                                return true;
                        case HFlag.EMode.sonyu:
                            return true;
                        case HFlag.EMode.masturbation:
                            return true;
                        case HFlag.EMode.peeping:
                            return false;
                        case HFlag.EMode.lesbian:
                            //if (NoNeckMoveList.Contains(main == 0 ?  _hFlag.nowAnimationInfo.paramFemale.path.file : _hFlag.nowAnimationInfo.paramFemale1.path.file))
                            return true;
                        //    return false;
                        //else
                        //    return true;
                        case HFlag.EMode.houshi3P:
                            return false;
                        case HFlag.EMode.sonyu3P:
                            return true;
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
                //if (!lookPoI && !lookAway && IsVoiceActive)
                //    return GetCurrentNeck; // GetCurrentEyes;
                switch (_hFlag.mode)
                {
                    case HFlag.EMode.aibu:
                        return 17;
                    case HFlag.EMode.sonyu:
                    case HFlag.EMode.sonyu3P:
                    case HFlag.EMode.sonyu3PMMF:
                    case HFlag.EMode.lesbian:
                    case HFlag.EMode.houshi:
                    case HFlag.EMode.houshi3P:
                    case HFlag.EMode.houshi3PMMF:
                        return 51;
                    default:
                        return 17;
                }
            }
        }
        private bool _wasEyeContact;
        private bool GetEyeContact => Vector3.Angle(_eyes.position - VR.Camera.Head.position, VR.Camera.Head.forward) < 30f;
        private bool IsCamClose => Vector3.Distance(_eyes.position, VR.Camera.Head.position) < 0.35f;
        internal void Proc()
        {
            // Reorganize this garbage dump.

            // TODO
            // A Dodge Kiss feature.

            //if (_handCtrl.isKiss)
            //{
            //    if (!_neckActive)
            //    {

            //        MoveNeckInit();
            //        moveNeckNext = Time.time + Random.Range(3f, 10f);
            //        SetNeck(GetProperEyeCam);
            //    }
            //    return;
            //}
            if (_handCtrl.isKiss)
            {
                if (!_vr && _neckActive)
                {
                    SensibleH.Logger.LogDebug($"Proc disabled neck");
                    MoveNeckHalt();
                }
            }
            else if (IsNeckMovable && !IsNeckMoving)
            {
                if (_neckActive)
                {
                    if (_vr && CurrentEyes == DirectionEye.Cam)
                    {
                        var contact = GetEyeContact;
                        if (contact && !_wasEyeContact)
                        {
                            SetEyes(Random.Range(13, 15));
                        }
                        _wasEyeContact = contact;
                    }
                    _specialNeckMove.Proc();
                }
                if (!_master._voiceController.IsVoiceActive)
                {
                    if (_vr && IsCamClose)
                    {
                        if (!lookAtCam && !_camWasClose && !lookAway)
                        {
                            MoveNeckInit();
                            SetNeckNextMove();
                            SetNeck(GetProperEyeCam);
                            lookAtCam = true;
                            _camWasClose = true;
                            SensibleH.Logger.LogDebug($"LookAlive[{_main}][VR] SetEyeCam");
                        }
                        else if (IsNeckTimeToMove)
                        {
                            lookAtCam = false;
                            if (!lookAway && Random.value < 0.2f)
                            {
                                LookAway();
                                SensibleH.Logger.LogDebug($"LookAlive[{_main}][VR] LookAway");
                            }
                            else
                            {
                                LookSomewhere();
                                SensibleH.Logger.LogDebug($"LookAlive[{_main}][VR] LookSomewhere");
                            }
                        }
                    }
                    else if (_neckActive)
                    {
                        if (lookAtCam)
                        {
                            SetNeckNextMove(0.5f);
                            lookAtCam = false;
                        }
                        else if (IsNeckTimeToStop)
                        {
                            if (moveNeckUntil < _neckNextMove)
                                moveNeckUntil = _neckNextMove;
                            else
                                MoveNeckHalt();
                        }
                        else if (!IsNeckRecent && _poseType == PoseType.Front && !lookAtCam && GetNextVoiceTime < lookBeforeVoiceTimer)
                        {
                            lookAtCam = true;
                            if (Random.value < 0.33f)//0.25f)
                            {
                                SensibleH.Logger.LogDebug($"LookAlive[{_main}][CamBeforeVoice] Abort");
                                return;
                            }
                            //if (Random.value > familiarity)
                            //{
                            //    // If the familiarity check fails, instead look somewhere else but the cam before speaking 
                            //    SensibleH.Logger.LogDebug($"LookAlive[{main}][CamBeforeVoice] LookOtherWay");
                            //    var neckList = AibuFrontIdleNeckDirections[NeckDirections[GetCurrentNeck]]
                            //        .Where(n => n != DirectionNeck.Cam)
                            //        .ToList();
                            //    var neck = neckList.ElementAt(Random.Range(0, neckList.Count));

                            //    SetNeck(GetCurrentEyes + NeckDirections.FirstOrDefault(n => n.Value == neck).Key);
                            //}
                            else
                            {
                                SetNeck(GetProperEyeCam);
                                SensibleH.Logger.LogDebug($"LookAlive[{_main}][CamBeforeVoice] LookAtCam");
                            }

                            lookBeforeVoiceTimer = Random.Range(2f, 4f);
                            //SetNeckNextMove(0.5f);
                        }
                        else if (IsNeckTimeToMove)
                        {
                            FemalePoI[_main] = null;
                            _eyesNextMove = PickEyes();
                            LookSomewhere();
                        }
                        if (_camWasClose)
                            _camWasClose = false;
                    }
                }
            }
            else if (_neckActive && !IsNeckMovable)
                MoveNeckHalt();


            if (_eyesNextMove < Time.time) // && !IsNeckMoving)
            {
                if (_master._voiceController.IsVoiceActive && Random.value < 0.4f)
                    _eyesNextMove = Time.time + Random.Range(2f, 5f);
                else
                    _eyesNextMove = PickEyes();
            }
        }
        internal void OnKissVrStart()
        {
            SensibleH.Logger.LogDebug($"OnKissVrStart {FemalePoI[0]} ");
            _specialNeckMove.OnKissVrAuxCam();
            FemalePoI[0] = _specialNeckMove._auxCam;
            if (!_neckActive)
            {
                MoveNeckInit();
            }
            SetNeck(17, true);

        }
        internal void OnKissVrEnd()
        {
            // At this moment CyuVR changes eyes back and thus changing eyes too soon is a bad idea.
            if (!_neckActive)
            {
                // Shouldn't happen but still.
                MoveNeckInit();
            }
            var properEyeCam = GetProperEyeCam;
            _specialNeckMove.SetAuxCamProperParent(properEyeCam);
            SetNeck(properEyeCam);
            SetNeckNextMove();
            moveNeckUntil = Time.time + Random.Range(20f, 40f);
            _eyesNextMove = Time.time + 3f;
            //lookAtCam = true;
            SensibleH.Logger.LogDebug($"OnKissVrEnd {FemalePoI[0]} ");
        }

        /// <summary>
        /// Currently is being used only to look at adjusted objects for breast and crouch during aibu.
        /// </summary>
        internal bool LookAtPoI(float _time)
        {
            if (!lookPoi && !lookAway && IsNeckMovable && _poiHandler.SetFemalePoI())
            {
                SensibleH.Logger.LogDebug($"LookAtPoI[{_main}]");
                lookPoi = true;
                moveNeckUntil = Time.time + Random.Range(_time, _time * 2f);
                MoveNeckInit();
                SetNeck(GetProperEyeCam);
                //PlayVoice()
                return true;
            }
            else
                return false;
        }
        public void LookAway()
        {
            // Make a new one.
            if (lookAway || _handCtrl.isKiss || !IsNeckMovable)
                return;
            SensibleH.Logger.LogDebug($"LookAway[{_main}]");
            lookAway = true;
            var time = Random.Range(4f, 6f);
            var neckChoice = SpecialNeckDirections.ElementAt(Random.Range(0, SpecialNeckDirections.Count)).Key;
            _neckNextMove = Time.time + time;
            _eyesNextMove = time - 1f;
            if (!_neckActive)
                MoveNeckInit();
            SetNeck(Random.Range(13, 17) + neckChoice, _quick: true);

            // Change eyes to something normal just before lookAway end.
            //StartCoroutine(RunAfterTimer(() => PickEyes(), time - 0.3f));
        }

        /// <summary>
        /// Alternative version of hook to start normal neck movement.
        /// </summary>
        private void LookAtCamDoNot()
        {
            var curNeck = GetCurrentNeck;
            SensibleH.Logger.LogDebug($"LookAtCamDoNot[{_main}] curNeck[{curNeck}]");
            if (NeckDirections[curNeck] == DirectionNeck.Cam)
                curNeck = GetNeckFromCurrentAnimation;

            // An option to only suppress neck movement during voice with no further movements.
            //if (Random.value < 0.5f)
            if (Random.value < 0.25f / (1f - _familiarity * 0.75f))
                MoveNeckInit(Time.time + 3f);
            else
                MoveNeckInit();
            SetNeck(curNeck + GetCurrentEyes);
        }
        /// <summary>
        /// Trigger to turn on/off neck movements, has 75 to 25 ratio;
        /// Arg "forced" only guarantees attempt to trigger.
        /// </summary>
        internal void LookAtCam(bool forced = false)
        {
            if ((!lookAtCam && !_handCtrl.isKiss && !IsNeckRecent && IsNeckMovable && !IsNeckMoving) || forced)
            {
                if (_poseType == PoseType.Front && Random.value < _familiarity * 0.75f)
                {
                    SensibleH.Logger.LogDebug($"OnVoiceProc[{_main}]");
                    lookAtCam = true;
                    MoveNeckInit();
                    SetNeck(GetProperEyeCam);
                }
                else
                {
                    LookAtCamDoNot();
                }
            }
        }
        private void MoveNeckInit(float customUntil = 0f)
        {
            if (POV.Instance != null && POV.Active && POV.GirlPOV)
                return;
            SensibleH.Logger.LogDebug($"MoveNeckInit[{_main}]");
            _neckActive = true;
            MoveNeckGlobal = true;
            if (customUntil == 0f)
                moveNeckUntil = Time.time + Random.Range(40f, 60f);
            else
                moveNeckUntil = customUntil;
        }
        /// <summary>
        /// Unconditionally stop all non-native neck movements.
        /// </summary>
        public void MoveNeckHalt()
        {
            FemalePoI[_main] = null;
            SensibleH.Logger.LogDebug($"MoveNeckHalt[{_main}]: stopping moveNeck");
            EyeNeckPtn[_main] = -1;
            _neckActive = false;
            lookAway = false;
            lookAtCam = false;
            lookPoi = false;
            _camWasClose = false;
            IsNeckSet[_main] = false;
        }
        private void GetPoseType()
        {
            string animName = _chara.animBody.runtimeAnimatorController.name;

            if (DontMoveAnimations.ContainsKey(_hFlag.mode) && DontMoveAnimations[_hFlag.mode].Contains(animName))
                _poseType = PoseType.Still;
            else if (FrontAnimations.ContainsKey(_hFlag.mode) && FrontAnimations[_hFlag.mode].Contains(animName))
                _poseType = PoseType.Front;
            else if (BackAnimations.ContainsKey(_hFlag.mode) && BackAnimations[_hFlag.mode].Contains(animName))
                _poseType = PoseType.Behind;
            else
            {
                _poseType = PoseType.Still;
                SensibleH.Logger.LogDebug($"GetPoseType[{_main}]: unknown animation found [{animName}]");
            }
            SensibleH.Logger.LogDebug($"GetPoseType[{_main}]:poseType[{_poseType}] animname[{animName}]");
        }

        private void LookSomewhere()
        {
            var camChance = _familiarity * 0.75f;
            var curNeck = NeckDirections[GetCurrentNeck];
            var curEyes = GetCurrentEyes;
            DirectionNeck newNeck = 0;

            if (_poseType == PoseType.Front)
            {
                if (CurrentNeck != DirectionNeck.Cam && Random.value < camChance)
                {
                    // 0.2 chance for absolute virgin to look at cam
                    // 0.75 for lewd state with maxed out intimacy.
                    SetNeck(GetProperEyeCam);
                    SensibleH.Logger.LogDebug($"LookSomewhere[{_main}][EyeCam] changing [{curNeck}] to [eyeCam]");
                }
                else if (_hFlag.nowAnimStateName.StartsWith("A", StringComparison.Ordinal) || _hFlag.nowAnimStateName.StartsWith("M", StringComparison.Ordinal))
                {
                    // Aibu "asoko" / "mune".
                    var list = GetAibuIdleNeckDir(CurrentNeck);
                    newNeck = list[Random.Range(0, list.Count)];
                    //newNeck = AibuFrontActionNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontActionNeckDirections[curNeck].Count));
                    SensibleH.Logger.LogDebug($"LookSomewhere[{_main}][Asoko/MunePositions] changing [{curNeck}] to [{newNeck}]");
                }
                else
                {
                    newNeck = AibuFrontIdleNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontIdleNeckDirections[curNeck].Count));
                    SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:IdlePositions, changing [{curNeck}] to [{newNeck}]");
                }
            }
            else //if (_poseType == PoseType.Behind)
            {
                newNeck = AibuBackNeckDirections[curNeck].ElementAt(Random.Range(0, AibuBackNeckDirections[curNeck].Count));
                SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:BackPositions, changing [{curNeck}] to [{newNeck}]");
            }



            //if (EyeDirForNeckFollow.ContainsKey(curEyes) &&
            //AibuFrontIdleNeckDirections[curNeck].Contains(NeckFollowEyeDir[EyeDirForNeckFollow[GetCurrentEyes]].FirstOrDefault())
            //&& Random.value < 0.5f)
            //{
            //    // NeckFollowsEyes Only Front Positions
            //    // Bloat that hardly works/worth it.
            //    // It works reasonable, but would really like a buddy of some kind.
            //    // For example: A Very slow Away neck, Kiss-dodge neck.
            //    newNeck = NeckFollowEyeDir[EyeDirForNeckFollow[GetCurrentEyes]].FirstOrDefault();
            //    SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:NeckFollowEyesPosition, changing [{curNeck}] to [{newNeck}]");
            //}
            //else if (_hFlag.nowAnimStateName.StartsWith("A", StringComparison.Ordinal) || _hFlag.nowAnimStateName.StartsWith("M", StringComparison.Ordinal))
            //{
            //    // AsokoMunePositions
            //    newNeck = AibuFrontActionNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontActionNeckDirections[curNeck].Count));
            //    SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:Asoko/MunePositions, changing [{curNeck}] to [{newNeck}]");
            //}
            //else //if (_hFlag.nowAnimStateName.StartsWith("I"))
            //{
            //    // FrontPositionsIdle
            //    if (Random.value < camChance && AibuFrontIdleNeckDirections[curNeck].Contains(DirectionNeck.Cam))
            //        newNeck = DirectionNeck.Cam;
            //    else
            //        newNeck = AibuFrontIdleNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontIdleNeckDirections[curNeck].Count));
            //    SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:IdlePositions, changing [{curNeck}] to [{newNeck}]");
            //}
            //else// if (poseType == 2)
            //{
            //    // BackPositions
            //    newNeck = AibuBackNeckDirections[curNeck].ElementAt(Random.Range(0, AibuBackNeckDirections[curNeck].Count));
            //    SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:BackPositions, changing [{curNeck}] to [{newNeck}]");
            //}
            if (newNeck != 0)
            {
                SetNeck(GetCurrentEyes + NeckDirections.FirstOrDefault(v => v.Value == newNeck).Key);
            }
            SetNeckNextMove();
        }

        private float PickEyes()
        {
            int eyes;
            int currentEyes = GetCurrentEyes;

            if (!_master._voiceController.IsVoiceActive && currentEyes == 0 && Random.value < 0.2f && (CurrentNeck == DirectionNeck.Cam || CurrentNeck == DirectionNeck.Mid || CurrentNeck == DirectionNeck.Pose)) // Dart away
                eyes = Random.Range(13, 15);
            else if (currentEyes != 0 && Random.value < 0.75f * _master._familiarity) // EyeCam
                eyes = 1;
            else
                eyes = Random.Range(2, 13); // Anything but former choices


            if (_neckActive)
            {
                if (EyeNeckPtn[_main] >= 17)
                    eyes += (GetCurrentNeck - 1);
            }
            else if (_master._voiceController.IsVoiceActive)
            {
                if (GetCurrentNeck >= 17)
                    eyes += (GetCurrentNeck - 1);
            }
            else
            {
                if (GetNeckFromCurrentAnimation >= 17)
                    eyes += (GetNeckFromCurrentAnimation - 1);
            }

            SetEyes(eyes);
            return Time.time + Random.Range(2f, 5f);
        }
        private void SetEyes(int _id)
        {
            if (_neckActive)
                EyeNeckPtn[_main] = _id;
            else
                _hFlag.voice.eyenecks[_main] = _id;
            CurrentEyes = EyeDirections[GetCurrentEyes];
            SensibleH.Logger.LogDebug($"SetEyes[{_main}][{CurrentEyes}]");
        }
        private void SetNeck(int _id, bool _quick = false)
        {
            var speedOfChange = 3f + ((int)(Random.value * 10f) * 0.1f);// (float)Math.Round(Random.Range(1.5f, 3f), 1);
            if (_quick)
                speedOfChange = 1f;
            else if (CurrentNeck == DirectionNeck.Cam)
            {
                speedOfChange = 4f;
            }
            _chara.neckLookCtrl.neckLookScript.changeTypeLeapTime = speedOfChange;
            _chara.neckLookCtrl.neckLookScript.changeTypeTimer = speedOfChange;
            //NeckChangeRate[main] = speedOfChange;
            EyeNeckPtn[_main] = _id;
            IsNeckSet[_main] = false;
            CurrentNeck = NeckDirections[GetCurrentNeck];
            if (CurrentNeck == DirectionNeck.Cam && FemalePoI[_main] == null)
            {
                _specialNeckMove.SetAuxCamProperParent(GetProperEyeCam);
                FemalePoI[_main] = _specialNeckMove._auxCam;
            }
            SensibleH.Logger.LogDebug($"SetNeck[{_main}] = [{_id}] speed = [{speedOfChange}]");
        }
        internal void OnPositionChange()
        {
            MoveNeckHalt();
            GetPoseType();
            _specialNeckMove.ResetAuxCam();
            if (SensibleH.EyeNeckControl.Value)
                LookAtCam();

            // The higher the familiarity, the less abrupt (more relaxed, slow) are neck movements.
        }
        
    }
}
