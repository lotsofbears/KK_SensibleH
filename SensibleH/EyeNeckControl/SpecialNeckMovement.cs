using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KK_SensibleH.EyeNeckControl; 
using static KK_SensibleH.EyeNeckControl.EyeNeckDictionaries;
using Random = UnityEngine.Random;
using VRGIN.Core;
using ADV.Commands.Base;
using KKAPI;
using KKAPI.MainGame;
using ADV.Commands.Object;

namespace KK_SensibleH.EyeNeckControl
{
    /*
     * 
     * Implement check for direction of away eyes, and introduce HIGH bias to look that way.
     * 
     * Bring back active PoI swap.
     * 
     */
    /// <summary>
    /// Controls "Fixational Neck Movement" and "Dodge Camera".
    /// There are 2 objects, one attached to the chara and another to the camera.
    /// We focus at one and move when appropriate. AuxCamera is surrogate, AuxPoi is native object.
    /// </summary>
    class SpecialNeckMovement
    {
        public static SpecialNeckMovement Instance;
        public GameObject AuxCam;

        private bool _vr;
        private bool _keepAuxCamStill; // don't move it when parent is kokan.

        private int _main;
        private float _nextMoveAt;
        private float _moveDelta;

        private GirlController _master;
        private NewNeckController _neck;
        private ChaControl _chara;
        private Transform _eyes;
        private Transform _head;
        private Transform _neckLookTarget;
        private Transform _auxCamParent;
        private Vector3 _auxCamParentLastPos;
        private HMotionEyeNeckFemale _eyeNeckMotion;
        internal SpecialNeckMovement(GirlController master, NewNeckController neck, int main, bool vr)
        {
            Instance = this;
            _master = master;
            _neck = neck;
            _main = main;
            _vr = vr;
            _chara = SensibleH._chaControl[main];
            _eyeNeckMotion = main == 0 ? SensibleH._eyeneckFemale : SensibleH._eyeneckFemale1;
            _neckLookTarget = _chara.objNeckLookTarget.transform;
            _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            _head = _chara.objHeadBone.transform;
            //var camera = _chara.transform.parent.Find("CameraBase/Camera");

            var fixCam = new GameObject().transform;
            fixCam.name = "FixationalNeckMovement";
            fixCam.localScale = Vector3.zero;
            AuxCam = fixCam.gameObject;
            if (vr)
            {
                _moveDelta = 0.05f;
            }
            else
            {
                _moveDelta = 0.15f;
            }
            SetAuxCamProperParent(17);
        }
        /// <summary>
        /// Reparent AuxCamera to chara, position stays.
        /// </summary>
        public void OnKissVrSetAuxCam()
        {
            var cam = AuxCam.transform;
            cam.position = _head.position + (VR.Camera.transform.position - _head.position) * 5f;
            cam.SetParent(_chara.transform, worldPositionStays: true);
        }
        //public void OnKissVRMoveAuxCam(Vector3 position)
        //{
        //    AuxCam.transform.position += position;
        //}
        //private void OnDodgeCam()
        //{
        //    var angle = Vector3.Angle(_camera.position - _eyes.position, _eyes.forward);
        //    //SensibleH.Logger.LogDebug($"OnDodgeCam[{angle}]");
        //    if (angle < 15f)
        //        SetAuxCamDodge();
        //}
        //private void SetAuxCamDodge()
        //{
        //    ResetAuxCam();
        //    var distance = Vector3.Distance(_eyes.position, _camera.position);
        //    _auxCam.transform.localPosition = AuxCamDodgeList.ElementAt(Random.Range(0, AuxCamDodgeList.Count)) * (distance * 10);
        //    //SensibleH.Logger.LogDebug($"SetAuxCamDodge[{distance}]");
        //}
        /// <summary>
        /// Add to local position.
        /// </summary>
        //public void MoveAuxCam(Vector3 position)
        //{
        //    //var camPos = _fixMoveCamera.transform.localPosition;
        //    _fixMoveCamera.transform.position += position;
        //}
        /// <summary>
        /// Reparent camera for FixationalNeckMove to Camera, position resets.
        /// </summary>
        //public void ParentAuxCam()
        //{
        //    ResetFixCamera();
        //    //SensibleH.Logger.LogDebug($"ParentFixMoveCam[{Vector3.Distance(cam.position, _eyes.position)}");
        //    //cam.localPosition = Vector3.zero;
        //    //if (SensibleHController.Instance._vr)
        //    //{
        //    //    cam.SetParent(VR.Camera.transform, worldPositionStays: false);
        //    ///}
        //    //else
        //    _fixMoveCamera.transform.SetParent(_chara.transform.parent.Find("CameraBase/Camera"), worldPositionStays: false);
        //}

        public void Proc(bool voiceActive, DirectionNeck neck, DirectionEye eyes)
        {
            if (neck != DirectionNeck.Pose && neck != DirectionNeck.Mid)
            {
                if (neck == DirectionNeck.Cam)
                {
                    if (Vector3.Distance(_auxCamParent.position, _auxCamParentLastPos) > _moveDelta)
                    {
                        // That is whatever AuxCam is being attached to has moved.
                        // So we move it back to parent's position.
                        ResetAuxCam();
                    }
                    if (voiceActive && Random.value < 0.6f)
                    {
                        return;
                    }
                }
                if (_nextMoveAt < Time.time)
                {
                    DoFixationalNeck(neck, eyes);
                }
            }
        }
        public void SetAuxCamForStaticNeck()
        {
            AuxCam.transform.localPosition = Vector3.zero;
            AuxCam.transform.position = _eyes.position + _eyes.forward;

            AuxCam.transform.SetParent(_chara.transform, worldPositionStays: true);
            _auxCamParent = _chara.transform;
            _auxCamParentLastPos = _auxCamParent.position;
        }
        public void SetAuxCamProperParent(int eyeCamId)
        {
            Transform parent;
            switch (eyeCamId)
            {
                //case 17:
                case 51:
                    parent = _eyeNeckMotion.objPartnerHead.transform;
                    _keepAuxCamStill = false;
                    break;
                case 85:
                    parent = _eyeNeckMotion.objPartnerKokan.transform;
                    _keepAuxCamStill = true;
                    break;
                default:
                    if (_vr)
                    {
                        parent = VR.Camera.transform;
                    }
                    else
                    {
                        parent = _chara.transform.parent.Find("CameraBase/Camera");
                    }
                    _keepAuxCamStill = false;
                    break;

            }
            AuxCam.transform.SetParent(parent, worldPositionStays: false);
            _auxCamParent = parent;
            ResetAuxCam();
        }
        public void ResetAuxCam()
        {
            // Second object (Poi) is being reset by the game.
            AuxCam.transform.localPosition = Vector3.zero;
            _auxCamParentLastPos = _auxCamParent.position;
        }
        private void DoFixationalNeck(DirectionNeck neck, DirectionEye eyes)
        {
            var voice = _master._voiceController.IsVoiceActive;
            if (!voice || Random.value < 0.5f)
            {
                //SensibleH.Logger.LogDebug($"DoFixationalNeck. {_master.CurrentNeck} move - {_keepAuxCamStill == false}");
                if (neck == DirectionNeck.Cam)
                {
                    if (!_keepAuxCamStill)
                    {
                        var dist = Vector3.Distance(_auxCamParent.position, _eyes.position);

                        var vec = GetAuxCamDic(eyes);
                        AuxCam.transform.localPosition += vec * (dist / 1.5f);// 0.8f);
                        if (AuxCam.transform.position.y < 0f)
                        {
                            AuxCam.transform.localPosition += Vector3.up * (Random.value * 0.2f);
                        }
                        _auxCamParentLastPos = _auxCamParent.position;
                        //SensibleH.Logger.LogDebug($"Neck:Extra:EyeCam:PoI:Move");
                    }

                }
                else
                {
                    var dist = Vector3.Distance(_eyes.position, _neckLookTarget.position);
                    if (eyes == DirectionEye.Cam && !_neck.IsNeckRecent)// && Random.value < 0.66f)
                    {
                        // Attempt to initiate eyeCam when eyes look at cam from other position.
                        _neck.LookAtCam();
                        //SensibleH.Logger.LogDebug($"Neck:Extra:Mundane:SwitchToEyeCam");
                    }
                    else// if (AuxPoiCamDic.ContainsKey(curEyes))
                    {
                        var vec = GetAuxPoiDic(eyes);
                        _neckLookTarget.localPosition += vec;
                        //SensibleH.Logger.LogDebug($"Neck:Extra:Mundane:PoI:Move");
                    }
                    //else
                    //{
                    /*
                     * Fixed VRGIN_XR, now new version works with current CharaStudio and new KKS_MainGame
Tested both games in Action/ADV/Talk/MainGameH/FreeH scenes, and back and forth between them all. 
                     */

                    //    var vec = new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                    //    _neckLookTarget.localPosition += vec;
                    //    //SensibleH.Logger.LogDebug($"MovePoiRandom[neck[{_master.CurrentNeck}]] [eyes[{curEyes}]] [{vec.x}] [{vec.y}] distance [{dist}]);");
                    //}
                }
            }
            var rand = Random.Range(2f, 6f);
            _nextMoveAt = Time.time + rand;
            _neck._neckNextMove += Mathf.Sqrt(rand); // * 0.1f + Random.value * 0.5f;
        }
        public void SetCooldown()
        {
            _nextMoveAt = Time.time + Random.Range(2f, 6f);
        }
    }
}
