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
using static KK_SensibleH.EyeNeckControl.EyeNeckDictionaries;

using static KK_SensibleH.AutoMode.LoopProperties;
using KK_SensibleH.AutoMode;
using static KK_SensibleH.GirlController;

namespace KK_SensibleH
{
    /// <summary>
    /// Recently Broken:
    /// 
    /// </summary>
    public class GirlController : MonoBehaviour
    {
        /*
         * Leave only Mid and Down ranges for neck, UP looks off
         * 
         * Opening some clothes triggers some reaction.
         * 
         * Add to pullOut PlayVoices[] = 0;
         * 
         * GotoDislikes()
         * 
         * heroine[].lewdness as modifier for things;
         * 
         * _chaControl.tearsLv
         * 
         */
        public void Initialize(int _main, float _familiarity)
        {
            main = _main;
            familiarity = _familiarity;
            _chara = _chaControl[main];
            AddPoi(_chara.objBodyBone.transform.Find(GetPoiPath(HandCtrl.AibuColliderKind.muneL)).gameObject, new Vector3(0.025f, 0f, 0.075f));
            AddPoi(_chara.objBodyBone.transform.Find(GetPoiPath(HandCtrl.AibuColliderKind.muneR)).gameObject, new Vector3(-0.025f, 0f, 0.075f));
            AddPoi(_chara.objBodyBone.transform.Find(GetPoiPath(HandCtrl.AibuColliderKind.kokan)).gameObject, new Vector3(0f, 0f, 0.075f));
            SensibleH.Logger.LogDebug($"familiarity[{main}] = [{familiarity}]");
            _vr = UnityEngine.VR.VRSettings.enabled;
            if (_vr)
            {
                _eyes = _chaControl[main].objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            }
            _voiceManager = Voice.Instance;
            _fixNeckMove = new SpecialNeckMovement(this, _main, _vr);
            GetPoseType();
            OnPositionChange();
            OnVoiceProc(forced: true);
        }
        private void OnDestroy()
        {
            foreach (var poi in _listOfMyPoI)
                Destroy(poi.gameObject);
        }
        private void AddPoi(GameObject parent, Vector3 localPos)
        {
            var notCube = new GameObject("PoI_SensibleH").transform;
            notCube.name = "PoI_SensibleH";
            notCube.SetParent(parent.transform, false);
            notCube.localScale = Vector3.zero;
            notCube.localPosition = localPos;
            _listOfMyPoI.Add(notCube);


            //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.SetParent(parent.transform, false);
            //cube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //cube.transform.localPosition = localPos;
            //cube.GetComponent<Collider>().enabled = false;
            ////cube.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
            //cube.GetComponent<Renderer>().enabled = false;
            //cube.name = "PoI_SensibleH";
        }
        public DirectionEye CurrentEyes;
        public DirectionNeck CurrentNeck;

        internal bool _neckActive;

        private int main;
        private float familiarity;
        internal float moveNeckUntil;
        internal float _neckNextMove;
        private float moveEyesNextMove;
        private float lookBeforeVoiceTimer = 2f;
        internal int _lastVoice;

        private bool malePoV;
        private bool femalePoV;
        private bool lookPoI;
        private bool lookAway;
        private bool lookAtCam;
        private bool _vr;
        private bool _camWasClose;

        private int[] reactions = { 9, 10, 13, 14 };
        private int[] reactionsTop = { 8, 9, 11, 12 };
        private int[] reactionsBottom = { 10, 13, 14 };
        private int[] reactionsFull = { 8, 9, 10, 11, 12, 13, 14 };
        private Transform _eyes;
        private static Manager.Voice _voiceManager;
        private List<Transform> _listOfMyPoI = new List<Transform>();
        private PoseTypes _poseType;
        internal SpecialNeckMovement _fixNeckMove;
        private ChaControl _chara;
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
        enum PoseTypes
        {
            Front,
            Behind,
            Still
        }
        private bool IsNeckTimeToMove => _neckNextMove < Time.time;
        private bool IsNeckTimeToStop => moveNeckUntil < Time.time;

        // Seems to be enough with all the extras that fixational movement can add.
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
        internal bool IsNeckRecent => _neckNextMove - Time.time > 5f;
        private bool IsVoiceActive => _hVoiceCtrl.nowVoices[main].state == HVoiceCtrl.VoiceKind.voice;
        private bool IsShortActive => _hVoiceCtrl.nowVoices[main].state == HVoiceCtrl.VoiceKind.breathShort;

        // Assigning neck a new position while it already is moving somewhere looks terrible.
        private bool IsNeckMoving => _chara.neckLookCtrl.neckLookScript.changeTypeLeapTime - _chaControl[main].neckLookCtrl.neckLookScript.changeTypeTimer != 0;
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
        private int GetNeckFromCurrentAnimation => main == 0 ? _eyeneckFemale.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[main].HExperience] / 17 * 17 :
            _eyeneckFemale1.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[main].HExperience] / 17 * 17;

        private int GetCurrentNeck
        {
            get
            {
                if (_neckActive && EyeNeckPtn[main] != -1)
                {
                    return (EyeNeckPtn[main] / 17) * 17;
                }
                else if (_hFlag.voice.eyenecks[main] != -1)
                {
                    return (_hFlag.voice.eyenecks[main] / 17) * 17;
                }
                else
                    return GetNeckFromCurrentAnimation;
            }
        }
        private int GetCurrentEyes
        {
            get
            {
                if (_neckActive && EyeNeckPtn[main] != -1)
                {
                    if (EyeNeckPtn[main] < 13)
                        return EyeNeckPtn[main] == 0 ? 0 : EyeNeckPtn[main] - 1;
                    else
                        return EyeNeckPtn[main] % 17;
                }
                else if (_hFlag.voice.eyenecks[main] != -1)
                {

                    if (_hFlag.voice.eyenecks[main] < 13)
                        return _hFlag.voice.eyenecks[main] == 0 ? 0 : _hFlag.voice.eyenecks[main] - 1;
                    else
                        return _hFlag.voice.eyenecks[main] % 17;
                }
                else
                {
                    if (main == 0)
                        return _eyeneckFemale.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[main].HExperience] % 17;
                    else
                        return _eyeneckFemale1.dicEyeNeck[_hFlag.nowAnimStateName].idEyeNecks[(int)_hFlag.lstHeroine[main].HExperience] % 17;
                }
            }
        }
        internal void OnPositionChange()
        {
            MoveNeckHalt();
            GetPoseType();
            if (SensibleH.EyeNeckControl.Value)
                OnVoiceProc();
            var neckTypes = _chara.neckLookCtrl.neckLookScript.neckTypeStates;

            // The higher the familiarity, the less abrupt (relaxed) are neck movements.
            var speed = (int)(10 * (2 - familiarity)) * 0.1f;
            SensibleH.Logger.LogDebug($"OnPositionChange[{main}] leapSpeed {speed}");
            foreach (var type in neckTypes)
            {
                type.leapSpeed = speed;
            }

        }
        private bool IsNeckMovable
        {
            get
            {
                if (_poseType == PoseTypes.Still)
                    return false;
                else
                {
                    switch (_hFlag.mode)
                    {
                        case HFlag.EMode.aibu:
                            var animName = _chaControl[main].animBody.runtimeAnimatorController.name;
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
                        return Random.value < 0.5f ? 51 : 85;
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

        public void OnKissStart()
        {
            _fixNeckMove.OnKissAuxCam();
            FemalePoI[0] = _fixNeckMove._auxCam;
            if (!_neckActive)
            {
                MoveNeckInit();
            }
            SetNeck(17, true);
            
        }
        public void OnKissEnd()
        {
            // At this moment CyuVR changes eyes back and thus changing eyes too soon is a bad idea.
            if (!_neckActive)
            {
                // Shouldn't happen but still.
                MoveNeckInit();
            }
            var properEyeCam = GetProperEyeCam;
            _fixNeckMove.SetAuxCamProperParent(properEyeCam);
            SetNeck(properEyeCam);
            SensibleH.Logger.LogDebug($"OnKissEnd ");
            moveNeckUntil = Time.time + Random.Range(20f, 40f);
            moveEyesNextMove = Time.time + 3f; 
            lookAtCam = true;
        }
        private bool IsCamClose => Vector3.Distance(_eyes.position, VR.Camera.SteamCam.head.position) < 0.35f;
        internal void Proc()
        {
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
                    MoveNeckHalt();
                }
            }
            else if (IsNeckMovable && !IsNeckMoving)
            {
                if (_neckActive)
                    _fixNeckMove.Proc();
                if (!IsVoiceActive)
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
                            SensibleH.Logger.LogDebug($"LookAlive[{main}][VR] SetEyeCam");
                        }
                        else if (IsNeckTimeToMove)
                        {
                            lookAtCam = false;
                            if (!lookAway && Random.value < 0.2f)
                            {
                                LookAway();
                                SensibleH.Logger.LogDebug($"LookAlive[{main}][VR] LookAway");
                            }
                            else
                            {
                                LookSomewhere();
                                SensibleH.Logger.LogDebug($"LookAlive[{main}][VR] LookSomewhere");
                            }
                        }
                    }
                    else if (_neckActive)
                    {
                        if (lookAtCam)
                        {
                            SetNeckNextMove(0.4f);
                            lookAtCam = false;
                        }
                        else if (IsNeckTimeToStop)
                        {
                            if (moveNeckUntil < _neckNextMove)
                                moveNeckUntil = _neckNextMove;
                            else
                                MoveNeckHalt();
                        }
                        else if (!IsNeckRecent && _poseType == PoseTypes.Front && !lookAtCam && GetNextVoiceTime < lookBeforeVoiceTimer)
                        {
                            lookAtCam = true;
                            if (Random.value < 0.25f)
                            {
                                SensibleH.Logger.LogDebug($"LookAlive[{main}][CamBeforeVoice] Abort");
                                return;
                            }
                            if (Random.value > familiarity)
                            {
                                // If the familiarity check fails, instead look somewhere else but the cam before speaking 
                                SensibleH.Logger.LogDebug($"LookAlive[{main}][CamBeforeVoice] LookOtherWay");
                                var neckList = AibuFrontIdleNeckDirections[NeckDirections[GetCurrentNeck]]
                                    .Where(n => n != DirectionNeck.Cam)
                                    .ToList();
                                var neck = neckList.ElementAt(Random.Range(0, neckList.Count));

                                SetNeck(GetCurrentEyes + NeckDirections.FirstOrDefault(n => n.Value == neck).Key);
                            }
                            else
                            {
                                SetNeck(GetProperEyeCam);
                                SensibleH.Logger.LogDebug($"LookAlive[{main}][CamBeforeVoice] LookAtCam");
                            }

                            lookBeforeVoiceTimer = Random.Range(1.5f, 3f);
                            SetNeckNextMove(0.2f);
                        }
                        else if (IsNeckTimeToMove)
                        {
                            FemalePoI[main] = null;
                            moveEyesNextMove = PickEyes();
                            LookSomewhere();
                        }
                        if (_camWasClose)
                            _camWasClose = false;
                    }
                }
            }
            else if (_neckActive && !IsNeckMovable)
                MoveNeckHalt();


            if (!lookPoI && !lookAway && moveEyesNextMove < Time.time) // && !IsNeckMoving)
            {
                if (IsVoiceActive && Random.value < 0.4f)
                    moveEyesNextMove = Time.time + Random.Range(2f, 5f);
                else
                    moveEyesNextMove = PickEyes();
            }
        }
        /// <summary>
        /// Currently is being used only to look at adjusted objects for breast and crouch during aibu.
        /// </summary>
        internal bool LookAtPoI(float _time)
        {
            if (lookPoI || lookAway || !IsNeckMovable || !SetFemalePoI())
                return false;
            SensibleH.Logger.LogDebug($"LookAtPoI[{main}]");
            lookPoI = true;
            moveNeckUntil = Time.time + Random.Range(_time, _time * 2f);
            MoveNeckInit();
            SetNeck(GetProperEyeCam);
            //PlayVoice()
            return true;
        }
        public void LookAway()
        {
            // Make new one.
            if (lookAway || _handCtrl.isKiss || !IsNeckMovable)
                return;
            SensibleH.Logger.LogDebug($"LookAway[{main}]");
            lookAway = true;
            var time = Random.Range(4f, 6f);
            var neckChoice = SpecialNeckDirections.ElementAt(Random.Range(0, SpecialNeckDirections.Count)).Key;
            _neckNextMove = Time.time + time;
            if (!_neckActive)
                MoveNeckInit();
            SetNeck(Random.Range(13, 17) + neckChoice, _quick: true);

            // Change eyes to something normal just before lookAway end.
            StartCoroutine(RunAfterTimer(() => PickEyes(), time - 0.3f));
        }
        /// <summary>
        /// Alternative version of hook to start normal neck movement.
        /// </summary>
        public void LookAtCamDoNot()
        {
            SensibleH.Logger.LogDebug($"LookAtCamDoNot[{main}]");
            var curNeck = GetCurrentNeck;
            if (NeckDirections[curNeck] == DirectionNeck.Cam)
                curNeck = GetNeckFromCurrentAnimation;

            // An option to only suppress neck movement during voice with no further movements.
            if (Random.value < 0.5f)
                MoveNeckInit(Time.time + 3f);
            else
                MoveNeckInit();
            SetNeck(curNeck + GetCurrentEyes);
        }
        /// <summary>
        /// Used mostly to initiate neck movement. The only hook for normal neck movement outside of VR.
        /// Arg "forced" only guarantees attempt to trigger.
        /// </summary>
        public void OnVoiceProc(bool forced = false)
        {
            if ((!lookAtCam && !_handCtrl.isKiss && !IsNeckRecent && IsNeckMovable && !IsNeckMoving) || forced)
            {
                if (_poseType == PoseTypes.Behind || Random.value < 0.5f)
                    LookAtCamDoNot();
                else
                {
                    SensibleH.Logger.LogDebug($"OnVoiceProc[{main}]");
                    lookAtCam = true;
                    MoveNeckInit();
                    SetNeck(GetProperEyeCam);
                }
            }
        }
        private void MoveNeckInit(float customUntil = 0f)
        {
            SensibleH.Logger.LogDebug($"MoveNeckInit[{main}]");
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
            FemalePoI[main] = null;
            SensibleH.Logger.LogDebug($"MoveNeckHalt[{main}]: stopping moveNeck");
            EyeNeckPtn[main] = -1;
            _neckActive = false;
            lookAway = false;
            lookAtCam = false;
            lookPoI = false;
            _camWasClose = false;
            IsNeckSet[main] = false;
        }
        private void GetPoseType()
        {
            string animName = _chaControl[main].animBody.runtimeAnimatorController.name;

            if (DontMoveAnimations.ContainsKey(_hFlag.mode) && DontMoveAnimations[_hFlag.mode].Contains(animName))
                _poseType = PoseTypes.Still;
            else if (FrontAnimations.ContainsKey(_hFlag.mode) && FrontAnimations[_hFlag.mode].Contains(animName))
                _poseType = PoseTypes.Front;
            else if (BackAnimations.ContainsKey(_hFlag.mode) && BackAnimations[_hFlag.mode].Contains(animName))
                _poseType = PoseTypes.Behind;
            else
            {
                _poseType = PoseTypes.Still;
                SensibleH.Logger.LogDebug($"GetPoseType[{main}]: unknown animation found [{animName}]");
            }
            SensibleH.Logger.LogDebug($"GetPoseType[{main}]:poseType[{_poseType}] animname[{animName}]");
        }
        private void LookSomewhere()
        {
            var camChance = familiarity * 0.75f;
            var curNeck = NeckDirections[GetCurrentNeck];
            DirectionNeck newNeck;
            
            if (_poseType == PoseTypes.Front)
            {
                if (EyeDirForNeckFollow.ContainsKey(GetCurrentEyes) &&
                AibuFrontIdleNeckDirections[curNeck].Contains(NeckFollowEyeDir[EyeDirForNeckFollow[GetCurrentEyes]].FirstOrDefault()) 
                && Random.value < 0.5f)
                {
                    // NeckFollowsEyes Only Front Positions
                    // Bloat that hardly works/worth it.
                    // It works reasonable, but would really like a buddy of some kind.
                    // For example: A Very slow Away neck, Kiss-dodge neck.
                    newNeck = NeckFollowEyeDir[EyeDirForNeckFollow[GetCurrentEyes]].FirstOrDefault();
                    SensibleH.Logger.LogDebug($"LookSomewhere[{main}]:NeckFollowEyesPosition, changing [{curNeck}] to [{newNeck}]");
                }
                else if (_hFlag.nowAnimStateName.StartsWith("A", StringComparison.Ordinal) || _hFlag.nowAnimStateName.StartsWith("M", StringComparison.Ordinal))
                {
                    // AsokoMunePositions
                    newNeck = AibuFrontActionNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontActionNeckDirections[curNeck].Count));
                    SensibleH.Logger.LogDebug($"LookSomewhere[{main}]:Asoko/MunePositions, changing [{curNeck}] to [{newNeck}]");
                }
                else //if (_hFlag.nowAnimStateName.StartsWith("I"))
                {
                    // FrontPositionsIdle
                    if (Random.value < camChance && AibuFrontIdleNeckDirections[curNeck].Contains(DirectionNeck.Cam))
                        newNeck = DirectionNeck.Cam;
                    else
                        newNeck = AibuFrontIdleNeckDirections[curNeck].ElementAt(Random.Range(0, AibuFrontIdleNeckDirections[curNeck].Count));
                    SensibleH.Logger.LogDebug($"LookSomewhere[{main}]:IdlePositions, changing [{curNeck}] to [{newNeck}]");
                }
            }
            else// if (poseType == 2)
            {
                // BackPositions
                newNeck = AibuBackNeckDirections[curNeck].ElementAt(Random.Range(0, AibuBackNeckDirections[curNeck].Count));
                SensibleH.Logger.LogDebug($"LookSomewhere[{main}]:BackPositions, changing [{curNeck}] to [{newNeck}]");
            }
            if (newNeck == DirectionNeck.Cam)
            {
                if (_hFlag.mode != HFlag.EMode.aibu)
                    SetFemalePoI();
                SetNeck(GetCurrentEyes + GetProperEyeCam);
            }
            else
                SetNeck(GetCurrentEyes + NeckDirections.FirstOrDefault(v => v.Value == newNeck).Key);
            SetNeckNextMove();
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


        private float PickEyes()
        {
            int eyes;
            int currentEyes = GetCurrentEyes;

            if (!IsVoiceActive && currentEyes == 0 && Random.value < 0.2f && (CurrentNeck == DirectionNeck.Cam || CurrentNeck == DirectionNeck.Mid || CurrentNeck == DirectionNeck.Pose)) // Dart away
                eyes = Random.Range(13, 15);
            else if (currentEyes != 0 && Random.value < 0.75f * familiarity) // EyeCam
                eyes = 1;
            else
                eyes = Random.Range(2, 13); // Anything but former choices


            if (_neckActive)
            {
                if (EyeNeckPtn[main] >= 17)
                    eyes += (GetCurrentNeck - 1);
            }
            else if (IsVoiceActive)
            {
                if(GetCurrentNeck >= 17)
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
                EyeNeckPtn[main] = _id;
            else
                _hFlag.voice.eyenecks[main] = _id;
            CurrentEyes = EyeDirections[GetCurrentEyes];
            SensibleH.Logger.LogDebug($"SetEyes[{main}][{CurrentEyes}]");
        }
        private void SetNeck(int _id, bool _quick = false)
        {
            var speedOfChange = 2f + ((int)(Random.value * 10f) * 0.1f);// (float)Math.Round(Random.Range(1.5f, 3f), 1);
            if (_quick)
                speedOfChange = 1f;
            else if (CurrentNeck == DirectionNeck.Cam)
            {
                speedOfChange = 3f;
            }
            _chaControl[main].neckLookCtrl.neckLookScript.changeTypeLeapTime = speedOfChange;
            _chaControl[main].neckLookCtrl.neckLookScript.changeTypeTimer = speedOfChange;
            //NeckChangeRate[main] = speedOfChange;
            EyeNeckPtn[main] = _id;
            IsNeckSet[main] = false;
            CurrentNeck = NeckDirections[GetCurrentNeck];
            if (CurrentNeck == DirectionNeck.Cam && FemalePoI[main] == null)
            {
                _fixNeckMove.SetAuxCamProperParent(GetProperEyeCam);
                FemalePoI[main] = _fixNeckMove._auxCam;
            }
            SensibleH.Logger.LogDebug($"SetNeck[{main}] = [{_id}] speed = [{speedOfChange}]");
        }

        /// <summary>
        /// Interrupt voice and maybe play it later.
        /// </summary>
        /// <param name="_time"></param> Minimal time in the queue.
        private void CatchVoice(float time = 1f)
        {
            SensibleH.Logger.LogDebug($"CatchVoice");
            if (IsVoiceActive)
            {
                StartCoroutine(RunAfterGasp(() => PlayVoice(_lastVoice), time));
                //_hFlag.voice.playVoices[main] = -1;
                //_voiceManager.Stop(_hFlag.transVoiceMouth[0]);
            }
        }
        internal void PlayVoice(int voiceId)
        {
            SensibleH.Logger.LogDebug($"GirlController[{main}] PlayVoices[{voiceId}] was supposed to happen");
            _hFlag.voice.playVoices[main] = voiceId;
        }
        internal void PlayShort(bool notOverwrite = false)
        {
            _hFlag.voice.playShorts[main] = Random.Range(0, 9);
            if (notOverwrite)
                _hVoiceCtrl.nowVoices[main].notOverWrite = true;
            SensibleH.Logger.LogDebug($"GirlController[{main}] - A Short Gasp has escaped");
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
                var hand = main == 0 ? _handCtrl : _handCtrl1;
                BetterSquirtController.RunSquirts(softSE: true, trigger: BetterSquirt.TriggerType.Touch, handCtrl: main == 0 ? _handCtrl : _handCtrl1);
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
            SensibleH.Logger.LogDebug($"StopVoice[{main}] - {id}");
            if (IsVoiceActive && _hVoiceCtrl.nowVoices[CurrentMain].voiceInfo.id == id)
            {
                _voiceManager.Stop(_hFlag.transVoiceMouth[CurrentMain]);
            }
        }
        private IEnumerator DoActionWhileTimer(Action _action, float time, params object[] _args)
        {
            time += Time.time;
            while (time > Time.time)
            {
                _action.DynamicInvoke(_args);
                yield return new WaitForSeconds(0.1f);
            }
        }
        internal IEnumerator RunAfterGasp(Action _method, float afterGaspWait, params object[] _args)
        {
            int _lastVoice = _hVoiceCtrl.nowVoices[main].voiceInfo.id;
            SensibleH.Logger.LogDebug($"RunAfterGasp[{main}]");
            while (IsShortActive)
            {
                yield return new WaitForSeconds(0.1f);
            }
            afterGaspWait += Time.time;
            while (afterGaspWait > Time.time)
            {
                yield return new WaitForSeconds(0.1f);
            }
            _method.DynamicInvoke(_args);
            SensibleH.Logger.LogDebug($"RunAfterGasp[{main}] [{_method.Method.Name}] was supposed to happen");
        }
        internal IEnumerator RunAfterTimer(Action _method, float timer, params object[] _args)
        {
            SensibleH.Logger.LogDebug($"RunAfterTimer[{main}]");
            timer += Time.time;
            while (timer > Time.time)
            {
                yield return new WaitForSeconds(0.1f);
            }
            _method.DynamicInvoke(_args);
            SensibleH.Logger.LogDebug($"RunAfterTimer[{main}] [{_method.Method.Name}] was supposed to happen");
        }
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
                                touchType = (HandCtrl.AibuColliderKind)reactionsTop[Random.Range(0, reactionsTop.Count())];
                                break;
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
            if (main == 0)
                _handCtrl.HitReactionPlay(touchType, shortPlay);
            else
                _handCtrl1.HitReactionPlay(touchType, shortPlay);
            SensibleH.Logger.LogDebug($"GirlController[{main}] - HitReactionPlay of type {touchType} was supposed to happen");
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
        private enum Target
        {
            Myself,
            FemalePartner,
            MalePartner
        }

        private Transform GetPoi(HandCtrl.AibuColliderKind aibuItem, Target target)
        {

            switch (target)
            {
                case Target.Myself:
                    return _listOfMyPoI[(int)aibuItem - 2].transform;
                case Target.FemalePartner:
                    return _chaControl[main == 0 ? 1 : 0].transform.Find(GetPoiPath(aibuItem));
                case Target.MalePartner:
                    return _chaControlM.transform.Find(GetPoiPath(aibuItem));
                default:
                    return null;
            }
        }
        private string GetPoiPath(HandCtrl.AibuColliderKind aibuItem)
        {
            switch (aibuItem)
            {
                case HandCtrl.AibuColliderKind.mouth:
                    return "cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/" +
                        "cf_s_head/p_cf_head_bone/cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz";
                case HandCtrl.AibuColliderKind.muneL:
                    return "cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/" +
                        "cf_s_bust00_L/cf_d_bust01_L/cf_j_bust01_L/cf_d_bust02_L/cf_j_bust02_L/cf_d_bust03_L";
                case HandCtrl.AibuColliderKind.muneR:
                    return "cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/" +
                        "cf_s_bust00_R/cf_d_bust01_R/cf_j_bust01_R/cf_d_bust02_R/cf_j_bust02_R/cf_d_bust03_R";
                case HandCtrl.AibuColliderKind.kokan:
                    return "cf_n_height/cf_j_hips/cf_j_waist01/cf_j_waist02/cf_d_kokan/cf_j_kokan/a_n_kokan";
                default:
                    return null;
            }
        }
        internal bool SetFemalePoI()
        {
            Transform transform = null;
            switch (_hFlag.mode)
            {
                case HFlag.EMode.aibu:
                    if(_handCtrl.actionUseItem != -1)
                    {
                        var itemList = new List<int>();
                        for (var i = 0; i < 3; i++)
                        {
                            if (_handCtrl.useAreaItems[i] != null)
                                itemList.Add(i);
                        }
                        transform = GetPoi((HandCtrl.AibuColliderKind)Random.Range(0, itemList.Count) + 2, Target.Myself);
                        //List<Transform> lstTransform = new List<Transform>();
                        //if (_handCtrl.useAreaItems[0] != null)
                        //    lstTransform.Add(customAccNipL.transform);
                        //if (_handCtrl.useAreaItems[1] != null)
                        //    lstTransform.Add(customAccNipR.transform);
                        //if (_handCtrl.useAreaItems[2] != null)
                        //    lstTransform.Add(customAccKokan.transform);
                        //if (lstTransform.Count > 0)
                        //    transform = lstTransform.ElementAt(Random.Range(0, lstTransform.Count));
                    }
                    break;
                //case HFlag.EMode.houshi:
                //    if(_hFlag.nowAnimationInfo.kindHoushi != 1)
                //    {
                //        transform = GetPoi((HandCtrl.AibuColliderKind)(Random.value < 0.5f ? 1 : 4), Target.MalePartner);
                //    }
                //    break;
                //case HFlag.EMode.sonyu:
                //    if (_handCtrl.actionUseItem != -1)
                //    {
                //        var itemList = new List<int>();
                //        for (var i = 0; i < 3; i++)
                //        {
                //            if (_handCtrl.useAreaItems[i] != null)
                //                itemList.Add(i);
                //        }
                //        transform = GetPoi((HandCtrl.AibuColliderKind)Random.Range(0, itemList.Count) + 2, Target.Myself);
                //        //List<Transform> lstTransform = new List<Transform>();
                //        //if (_handCtrl.useAreaItems[0] != null)
                //        //    lstTransform.Add(customAccNipL.transform);
                //        //if (_handCtrl.useAreaItems[1] != null)
                //        //    lstTransform.Add(customAccNipR.transform);
                //        //if (lstTransform.Count > 0)
                //        //    transform = lstTransform.ElementAt(Random.Range(0, lstTransform.Count));
                //    }
                //    else
                //        transform = GetPoi(HandCtrl.AibuColliderKind.mouth, Target.MalePartner);
                //    break;
                //case HFlag.EMode.masturbation:
                //    //switch (Random.Range(0, 2))
                //    //{
                //    //    case 0:
                //    //        transform = customAccNipL.transform;
                //    //        break;
                //    //    case 1:
                //    //        transform = customAccNipR.transform;
                //    //        break;
                //    //    case 2:
                //    //        transform = customAccKokan.transform;
                //    //        break;
                //    //    case 3:
                //    //        transform = null;
                //    //        break;
                //    //}
                //    break;
                case HFlag.EMode.lesbian:

                    //if (NoNeckMoveList.Contains(main == 0 ?  _hFlag.nowAnimationInfo.paramFemale.path.file : _hFlag.nowAnimationInfo.paramFemale1.path.file))
                    //switch (Random.Range(0, 3))
                    //{
                    //    case 0:
                    //        transform = GetPoI("a_n_kokan", main == 0 ? 1 : 0);
                    //        break;
                    //    case 1:
                    //        transform = GetPoI("cf_J_FaceUp_tz", main == 0 ? 1 : 0);
                    //        break;
                    //    case 2:
                    //        transform = GetPoI("cf_j_spine03", main == 0 ? 1 : 0);
                    //        break;
                    //}
                    transform = GetPoi((HandCtrl.AibuColliderKind)Random.Range(1, 5), Target.FemalePartner);
                    break;
            }
            SensibleH.Logger.LogDebug($"SetFemalePoI[{main}] = {transform}");
            if (transform != null)
            {
                FemalePoI[main] = transform.gameObject;
                return true;
            }
            else
                return false;
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