//using ActionGame.Chara.Mover;
//using ADV.Commands.Base;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Random = UnityEngine.Random;
//using UnityEngine;
//using static KK_SensibleH.SensibleH;
//using static KK_SensibleH.EyeNeckControl.EyeNeckDictionaries;
//using VRGIN;
//using System.Runtime.InteropServices;
//using KKS_VR;
//using VRGIN.Core;
//using KKS_VR.Features;

//namespace KK_SensibleH.EyeNeckControl
//{
//    public class EyeNeckController :MonoBehaviour
//    {
//        // Features Proper dartAway with check for angle;

//        // Broken PreVoiceEyeCam (checks continuously)
//        internal void Initialize(GirlController master, int main, bool vr, float familiarity)
//        {
//            _master = master;
//            _main = main;
//            _chara = _chaControl[main];
//            _familiarity = familiarity;
//            _vr = vr;
//            _specialNeckMove = new SpecialNeckMovement(master, this, main, vr);
//            _poiHandler = new PoiHandler(main);
//            if (_vr)
//            {
//                _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
//            }
//            GetPoseType();
//            LookAtCam(forced: true);
//        }

//        public DirectionEye CurrentEyes;
//        public DirectionNeck CurrentNeck;
//        private GirlController _master;
//        private SpecialNeckMovement _specialNeckMove;
//        private PoiHandler _poiHandler;
//        private ChaControl _chara;
//        private Transform _eyes;

//        private int _main;
//        private float _familiarity;
//        internal float moveNeckUntil;
//        internal float _neckNextMove;
//        private float _eyesNextMove;
//        private float lookBeforeVoiceTimer = 2f;

//        internal bool _neckActive;
//        private bool lookPoi;
//        private bool lookAway;
//        private bool lookAtCam;
//        private bool _vr;
//        private bool _camWasClose;
//        private PoseType _poseType;

//        internal bool IsNeckRecent => _neckNextMove - Time.time > 5f;
//        private bool IsNeckTimeToMove => _neckNextMove < Time.time;
//        private bool IsNeckTimeToStop => moveNeckUntil < Time.time;
//        private bool IsNeckMoving => _chara.neckLookCtrl.neckLookScript.changeTypeLeapTime - _chara.neckLookCtrl.neckLookScript.changeTypeTimer != 0;

//        // Seems to be enough with all the extras that fixational movement adds.
//        private void SetNeckNextMove(float multiplier = 1f) => _neckNextMove = Time.time + 10f * multiplier;
//        private float GetNextVoiceTime
//        {
//            get
//            {
//                switch (hFlag.mode)
//                {
//                    case HFlag.EMode.aibu:
//                        return hFlag.voice.timeAibu.timeIdle - hFlag.voice.timeAibu.timeIdleCalc;
//                    case HFlag.EMode.houshi:
//                        return hFlag.voice.timeHoushi.timeIdle - hFlag.voice.timeHoushi.timeIdleCalc;
//                    case HFlag.EMode.sonyu:
//                        return hFlag.voice.timeSonyu.timeIdle - hFlag.voice.timeSonyu.timeIdleCalc;
//                    case HFlag.EMode.masturbation:
//                        return hFlag.timeMasturbation.timeIdle - hFlag.timeMasturbation.timeIdleCalc;
//                    //case HFlag.EMode.lesbian:
//                    //    return hFlag.timeLesbian.timeIdle - hFlag.timeLesbian.timeIdleCalc;
//                    default:
//                        return 10f;
//                }
//            }
//        }
//        private int GetNeckFromCurrentAnimation => _main == 0 ? _eyeneckFemale.dicEyeNeck[hFlag.nowAnimStateName].idEyeNecks[(int)hFlag.lstHeroine[_main].HExperience] / 17 * 17 :
//            _eyeneckFemale1.dicEyeNeck[hFlag.nowAnimStateName].idEyeNecks[(int)hFlag.lstHeroine[_main].HExperience] / 17 * 17;

//        private int GetCurrentNeck
//        {
//            get
//            {
//                if (_neckActive && EyeNeckPtn[_main] != -1)
//                {
//                    return (EyeNeckPtn[_main] / 17) * 17;
//                }
//                else if (hFlag.voice.eyenecks[_main] != -1)
//                {
//                    return (hFlag.voice.eyenecks[_main] / 17) * 17;
//                }
//                else
//                    return GetNeckFromCurrentAnimation;
//            }
//        }
//        private int GetCurrentEyes
//        {
//            get
//            {
//                if (_neckActive && EyeNeckPtn[_main] != -1)
//                {
//                    if (EyeNeckPtn[_main] < 13)
//                        return EyeNeckPtn[_main] == 0 ? 0 : EyeNeckPtn[_main] - 1;
//                    else
//                        return EyeNeckPtn[_main] % 17;
//                }
//                else if (hFlag.voice.eyenecks[_main] != -1)
//                {

//                    if (hFlag.voice.eyenecks[_main] < 13)
//                        return hFlag.voice.eyenecks[_main] == 0 ? 0 : hFlag.voice.eyenecks[_main] - 1;
//                    else
//                        return hFlag.voice.eyenecks[_main] % 17;
//                }
//                else
//                {
//                    if (_main == 0)
//                        return _eyeneckFemale.dicEyeNeck[hFlag.nowAnimStateName].idEyeNecks[(int)hFlag.lstHeroine[_main].HExperience] % 17;
//                    else
//                        return _eyeneckFemale1.dicEyeNeck[hFlag.nowAnimStateName].idEyeNecks[(int)hFlag.lstHeroine[_main].HExperience] % 17;
//                }
//            }
//        }
//        private bool IsNeckMovable
//        {
//            get
//            {
//                if (_poseType == PoseType.Still)
//                    return false;
//                else
//                {
//                    switch (hFlag.mode)
//                    {
//                        case HFlag.EMode.aibu:
//                            var animName = _chara.animBody.runtimeAnimatorController.name;
//                            if (DontMoveNeckSpecialCases.ContainsKey(animName))
//                            {
//                                foreach (var neck in DontMoveNeckSpecialCases[animName])
//                                {
//                                    if (hFlag.nowAnimStateName.StartsWith(neck, StringComparison.Ordinal))
//                                        return false;
//                                }
//                                //for (int i = 0; i < DontMoveNeckSpecialCases[animName].Count; i++)
//                                //{
//                                //    if (hFlag.nowAnimStateName.StartsWith(DontMoveNeckSpecialCases[animName].ElementAt(i), StringComparison.Ordinal))
//                                //        return false;
//                                //}
//                            }
//                            return true;
//                        case HFlag.EMode.houshi:
//                            if (hFlag.nowAnimationInfo.kindHoushi == 1)
//                                return false;
//                            else
//                                return true;
//                        case HFlag.EMode.sonyu:
//                            return true;
//                        case HFlag.EMode.masturbation:
//                            return true;
//                        case HFlag.EMode.peeping:
//                            return false;
//                        case HFlag.EMode.lesbian:
//                            //if (NoNeckMoveList.Contains(main == 0 ?  hFlag.nowAnimationInfo.paramFemale.path.file : hFlag.nowAnimationInfo.paramFemale1.path.file))
//                            return true;
//                        //    return false;
//                        //else
//                        //    return true;
//                        case HFlag.EMode.houshi3P:
//                            return false;
//                        case HFlag.EMode.sonyu3P:
//                            return true;
//                        default:
//                            return false;
//                    }
//                }
//            }
//        }
//        internal int GetProperEyeCam
//        {
//            // By default:
//            // 17 - camera
//            // 51 - partners head
//            // 85 - partners kokan
//            get
//            {
//                //if (!lookPoI && !lookAway && IsVoiceActive)
//                //    return GetCurrentNeck; // GetCurrentEyes;
//                switch (hFlag.mode)
//                {
//                    case HFlag.EMode.aibu:
//                        return 17;
//                    case HFlag.EMode.sonyu:
//                    case HFlag.EMode.sonyu3P:
//                    case HFlag.EMode.sonyu3PMMF:
//                    case HFlag.EMode.lesbian:
//                    case HFlag.EMode.houshi:
//                    case HFlag.EMode.houshi3P:
//                    case HFlag.EMode.houshi3PMMF:
//                        return 51;
//                    default:
//                        return 17;
//                }
//            }
//        }
//        private bool _wasEyeContact;
//        private bool GetEyeContact => Vector3.Angle(_eyes.position - VR.Camera.Head.position, VR.Camera.Head.forward) < 30f;
//        private bool IsCamClose => Vector3.Distance(_eyes.position, VR.Camera.Head.position) < 0.35f;
//        internal void Proc()
//        {
//            // Reorganize this garbage dump.

//            // TODO
//            // A Dodge Kiss feature.
//            if (_handCtrl.isKiss)
//            {
//                if (!_vr && _neckActive)
//                {
//                    //SensibleH.Logger.LogDebug($"Proc disabled neck");
//                    MoveNeckHalt();
//                }
//            }
//            else if (IsNeckMovable && !IsNeckMoving)
//            {
//                if (_neckActive)
//                {
//                    if (_vr && CurrentEyes == DirectionEye.Cam)
//                    {
//                        var contact = GetEyeContact;
//                        if (contact && !_wasEyeContact)
//                        {
//                            SetEyes(Random.Range(13, 15));
//                        }
//                        _wasEyeContact = contact;
//                    }
//                    _specialNeckMove.Proc();
//                }
//                if (!_master._voiceController.IsVoiceActive)
//                {
//                    if (_vr && IsCamClose)
//                    {
//                        if (!lookAtCam && !_camWasClose && !lookAway)
//                        {
//                            MoveNeckInit();
//                            SetNeckNextMove();
//                            SetNeck(GetProperEyeCam);
//                            lookAtCam = true;
//                            _camWasClose = true;
//                            //SensibleH.Logger.LogDebug($"LookAlive[{_main}][VR] SetEyeCam");
//                        }
//                        else if (IsNeckTimeToMove)
//                        {
//                            lookAtCam = false;
//                            if (!lookAway && Random.value < 0.2f)
//                            {
//                                LookAway();
//                                //SensibleH.Logger.LogDebug($"LookAlive[{_main}][VR] LookAway");
//                            }
//                            else
//                            {
//                                LookSomewhere();
//                                //SensibleH.Logger.LogDebug($"LookAlive[{_main}][VR] LookSomewhere");
//                            }
//                        }
//                    }
//                    else if (_neckActive)
//                    {
//                        if (lookAtCam)
//                        {
//                            SetNeckNextMove(0.5f);
//                            lookAtCam = false;
//                        }
//                        else if (IsNeckTimeToStop)
//                        {
//                            if (moveNeckUntil < _neckNextMove)
//                                moveNeckUntil = _neckNextMove;
//                            else
//                                MoveNeckHalt();
//                        }
//                        else if (!IsNeckRecent && _poseType == PoseType.Front && !lookAtCam && GetNextVoiceTime < lookBeforeVoiceTimer)
//                        {
//                            lookAtCam = true;
//                            if (Random.value < 0.33f)//0.25f)
//                            {
//                                //SensibleH.Logger.LogDebug($"LookAlive[{_main}][CamBeforeVoice] Abort");
//                                return;
//                            }
//                            //if (Random.value > familiarity)
//                            //{
//                            //    // If the familiarity check fails, instead look somewhere else but the cam before speaking 
//                            //    //SensibleH.Logger.LogDebug($"LookAlive[{main}][CamBeforeVoice] LookOtherWay");
//                            //    var neckList = AibuFrontIdleNeckDirections[NeckDirections[GetCurrentNeck]]
//                            //        .Where(n => n != DirectionNeck.Cam)
//                            //        .ToList();
//                            //    var neck = neckList.ElementAt(Random.Range(0, neckList.Count));

//                            //    SetNeck(GetCurrentEyes + NeckDirections.FirstOrDefault(n => n.Value == neck).Key);
//                            //}
//                            else
//                            {
//                                _specialNeckMove.ResetAuxCam();
//                                SetNeck(GetProperEyeCam);
//                                //SensibleH.Logger.LogDebug($"LookAlive[{_main}][CamBeforeVoice] LookAtCam");
//                            }
//                            lookBeforeVoiceTimer = Random.Range(2f, 4f);
//                            //SetNeckNextMove(0.5f);
//                        }
//                        else if (IsNeckTimeToMove)
//                        {
//                            FemalePoI[_main] = null;
//                            _eyesNextMove = PickEyes();
//                            LookSomewhere();
//                        }
//                        if (_camWasClose)
//                            _camWasClose = false;
//                    }
//                }
//            }
//            else if (_neckActive && !IsNeckMovable)
//                MoveNeckHalt();


//            if (_eyesNextMove < Time.time) // && !IsNeckMoving)
//            {
//                if (_master._voiceController.IsVoiceActive && Random.value < 0.4f)
//                    _eyesNextMove = Time.time + Random.Range(2f, 5f);
//                else
//                    _eyesNextMove = PickEyes();
//            }
//        }
//        private void LookSomewhere()
//        {
//            var camChance = _familiarity * 0.75f;
//            var curNeck = NeckDirections[GetCurrentNeck];
//            var curEyes = GetCurrentEyes;
//            DirectionNeck newNeck = 0;

//            if (_poseType == PoseType.Front)
//            {
//                if (Random.value < camChance)
//                {
//                    // 0.2 chance for absolute virgin to look at cam
//                    // 0.75 for lewd state with maxed out intimacy.
//                    SetNeck(GetProperEyeCam);
//                    //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}][EyeCam] changing [{curNeck}] to [eyeCam]");
//                }
//                else if (hFlag.nowAnimStateName.StartsWith("A", StringComparison.Ordinal) || hFlag.nowAnimStateName.StartsWith("M", StringComparison.Ordinal))
//                {
//                    // Aibu "asoko" / "mune".
//                    var list = GetAibuActionDir(CurrentNeck);
//                    newNeck = list[Random.Range(0, list.Count)];
//                    //newNeck = AibuFrontActionNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontActionNeckDirections[curNeck].Count));
//                    //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}][Asoko/MunePositions] changing [{curNeck}] to [{newNeck}]");
//                }
//                else
//                {
//                    var list = GetAibuIdleNeckDir(CurrentNeck);
//                    newNeck = list[Random.Range(0, list.Count)];
//                    //newNeck = AibuFrontIdleNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontIdleNeckDirections[curNeck].Count));
//                    //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:IdlePositions, changing [{curNeck}] to [{newNeck}]");
//                }
//            }
//            else //if (_poseType == PoseType.Behind)
//            {
//                newNeck = AibuBackNeckDirections[curNeck].ElementAt(Random.Range(0, AibuBackNeckDirections[curNeck].Count));
//                //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:BackPositions, changing [{curNeck}] to [{newNeck}]");
//            }



//            //if (EyeDirForNeckFollow.ContainsKey(curEyes) &&
//            //AibuFrontIdleNeckDirections[curNeck].Contains(NeckFollowEyeDir[EyeDirForNeckFollow[GetCurrentEyes]].FirstOrDefault())
//            //&& Random.value < 0.5f)
//            //{
//            //    // NeckFollowsEyes Only Front Positions
//            //    // Bloat that hardly works/worth it.
//            //    // It works reasonable, but would really like a buddy of some kind.
//            //    // For example: A Very slow Away neck, Kiss-dodge neck.
//            //    newNeck = NeckFollowEyeDir[EyeDirForNeckFollow[GetCurrentEyes]].FirstOrDefault();
//            //    //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:NeckFollowEyesPosition, changing [{curNeck}] to [{newNeck}]");
//            //}
//            //else if (hFlag.nowAnimStateName.StartsWith("A", StringComparison.Ordinal) || hFlag.nowAnimStateName.StartsWith("M", StringComparison.Ordinal))
//            //{
//            //    // AsokoMunePositions
//            //    newNeck = AibuFrontActionNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontActionNeckDirections[curNeck].Count));
//            //    //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:Asoko/MunePositions, changing [{curNeck}] to [{newNeck}]");
//            //}
//            //else //if (hFlag.nowAnimStateName.StartsWith("I"))
//            //{
//            //    // FrontPositionsIdle
//            //    if (Random.value < camChance && AibuFrontIdleNeckDirections[curNeck].Contains(DirectionNeck.Cam))
//            //        newNeck = DirectionNeck.Cam;
//            //    else
//            //        newNeck = AibuFrontIdleNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontIdleNeckDirections[curNeck].Count));
//            //    //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:IdlePositions, changing [{curNeck}] to [{newNeck}]");
//            //}
//            //else// if (poseType == 2)
//            //{
//            //    // BackPositions
//            //    newNeck = AibuBackNeckDirections[curNeck].ElementAt(Random.Range(0, AibuBackNeckDirections[curNeck].Count));
//            //    //SensibleH.Logger.LogDebug($"LookSomewhere[{_main}]:BackPositions, changing [{curNeck}] to [{newNeck}]");
//            //}
//            if (newNeck != 0)
//            {
//                SetNeck(GetCurrentEyes + NeckDirections.FirstOrDefault(v => v.Value == newNeck).Key);
//            }
//            SetNeckNextMove();
//        }

//        private float PickEyes()
//        {
//            int eyes;
//            int currentEyes = GetCurrentEyes;

//            if (!_master._voiceController.IsVoiceActive && currentEyes == 0 && Random.value < 0.2f && (CurrentNeck == DirectionNeck.Cam || CurrentNeck == DirectionNeck.Mid || CurrentNeck == DirectionNeck.Pose)) // Dart away
//                eyes = Random.Range(13, 15);
//            else if (currentEyes != 0 && Random.value < 0.75f * _master._familiarity) // EyeCam
//                eyes = 1;
//            else
//                eyes = Random.Range(2, 13); // Anything but former choices


//            if (_neckActive)
//            {
//                if (EyeNeckPtn[_main] >= 17)
//                    eyes += (GetCurrentNeck - 1);
//            }
//            else if (_master._voiceController.IsVoiceActive)
//            {
//                if (GetCurrentNeck >= 17)
//                    eyes += (GetCurrentNeck - 1);
//            }
//            else
//            {
//                if (GetNeckFromCurrentAnimation >= 17)
//                    eyes += (GetNeckFromCurrentAnimation - 1);
//            }

//            SetEyes(eyes);
//            return Time.time + Random.Range(2f, 5f);
//        }
//        private void SetEyes(int id)
//        {
//            if (_neckActive)
//                EyeNeckPtn[_main] = id;
//            else
//                hFlag.voice.eyenecks[_main] = id;
//            CurrentEyes = EyeDirections[GetCurrentEyes];
//            //SensibleH.Logger.LogDebug($"SetEyes[{_main}][{CurrentEyes}]");
//        }
//        private void SetNeck(int id, bool quick = false)
//        {
//            var speedOfChange = 3f;// 2f + ((int)(Random.value * 10f) * 0.1f);// (float)Math.Round(Random.Range(1.5f, 3f), 1);
//            if (quick)
//                speedOfChange = 1f;
//            _chara.neckLookCtrl.neckLookScript.changeTypeLeapTime = speedOfChange;
//            _chara.neckLookCtrl.neckLookScript.changeTypeTimer = speedOfChange;
//            EyeNeckPtn[_main] = id;
//            IsNeckSet[_main] = false;
//            CurrentNeck = NeckDirections[GetCurrentNeck];
//            if (CurrentNeck == DirectionNeck.Cam && FemalePoI[_main] == null)
//            {
//                _specialNeckMove.SetAuxCamProperParent(GetProperEyeCam);
//                FemalePoI[_main] = _specialNeckMove._auxCam;
//            }
//            _specialNeckMove.SetCooldown();
//            //SensibleH.Logger.LogDebug($"SetNeck[{_main}] = [{id}] speed = [{speedOfChange}]");
//        }
        
//    }
//}
