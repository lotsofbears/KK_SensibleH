using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KK_SensibleH.EyeNeckControl; 
using static KK_SensibleH.EyeNeckControl.EyeNeckDictionaries;
using Random = UnityEngine.Random;
using VRGIN.Core;

namespace KK_SensibleH.EyeNeckControl
{
    /*
     * 
     * Implement check for direction of away eyes, and introduce HIGH bias to look that way.
     * 
     * 
     */
    /// <summary>
    /// Controls "Fixational Neck Movement" and "Dodge Camera".
    /// There are 2 objects, one attached to the chara and another to the camera.
    /// We focus at one and move when appropriate. AuxCamera is surrogate, AuxPoi is native object.
    /// </summary>
    class SpecialNeckMovement
    {
        internal GameObject _auxCam;
        private GirlController _master;
        private Transform _eyes;
        //private Transform _shoulders;
        private Transform _neckLookTarget;
        private Transform _auxCamParent;
        private Vector3 _auxCamParentLastPos;
        private ChaControl _chara;
        private int _main = 0;
        private float _nextMoveAt;
       // private Vector3 _auxCamLastPos;
        private bool _vr;
        //private Transform _camera;
        private HMotionEyeNeckFemale _eyeNeckMotion;
        private bool _keepAuxCamStill; // don't move it when parent is kokan.
        internal SpecialNeckMovement(GirlController girlController, int main, bool vr)
        {
            _main = main;
            _vr = vr;
            _chara = SensibleH._chaControl[main];
            _eyeNeckMotion = main == 0 ? SensibleH._eyeneckFemale : SensibleH._eyeneckFemale1;
            _master = girlController;
            _neckLookTarget = _chara.objNeckLookTarget.transform; // objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/" +
                //"cf_j_spine03/cf_s_spine03/N_NeckLookTargetP/N_NeckLookTarget");
            //_shoulders = _chara.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_backsk_00");
            _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            //var camera = _chara.transform.parent.Find("CameraBase/Camera");

            var fixCam = new GameObject().transform;
            fixCam.name = "FixationalNeckMovement";
            fixCam.localScale = Vector3.zero;
            _auxCam = fixCam.gameObject;

            //if (vr)
            //{
            //    _camera = VR.Camera.transform;
            //}
            //else
            //    _camera = _chara.transform.parent.Find("CameraBase/Camera");

            SetAuxCamProperParent(17);

            //SensibleH.Logger.LogDebug($"FixNeckMove[Awake] {_neckLookTarget}");
        }
        /// <summary>
        /// Reparent camera for FixationalNeckMove to HScene, position stays.
        /// </summary>
        public void OnKissAuxCam()
        {
            // Using VR cam for VR feature is kinda given.. too fed up to try the other way.
            var cam = _auxCam.transform;
            var camVR = VR.Camera.transform;
            //var camVR = _chara.transform.parent.Find("CameraBase/Camera");
            // test camera movements outside VR.
           // cam.position = camVR.position;
            cam.position = camVR.position;
            cam.position += camVR.position + camVR.forward * -0.8f + camVR.up * 0.4f - cam.position;
            cam.SetParent(_chara.objTop.transform, worldPositionStays: true);
        }
        //private void OnDodgeCam()
        //{
        //    var angle = Vector3.Angle(_camera.position - _eyes.position, _eyes.forward);
        //    SensibleH.Logger.LogDebug($"OnDodgeCam[{angle}]");
        //    if (angle < 15f)
        //        SetAuxCamDodge();
        //}
        //private void SetAuxCamDodge()
        //{
        //    ResetAuxCam();
        //    var distance = Vector3.Distance(_eyes.position, _camera.position);
        //    _auxCam.transform.localPosition = AuxCamDodgeList.ElementAt(Random.Range(0, AuxCamDodgeList.Count)) * (distance * 10);
        //    SensibleH.Logger.LogDebug($"SetAuxCamDodge[{distance}]");
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
        public void Proc()
        {
            if (_master._neckActive && _master.CurrentNeck != GirlController.DirectionNeck.Pose && _master.CurrentNeck != GirlController.DirectionNeck.Mid)
            {
                if (_nextMoveAt < Time.time)
                {
                    DoFixationalNeck();
                }
                else if (_master.CurrentNeck == GirlController.DirectionNeck.Cam && Vector3.Distance(_auxCamParent.position, _auxCamParentLastPos) > 0.15f)
                {
                    // That is whatever AuxCam is being attached to has moved.
                    ResetAuxCam();
                }
            }
        }
        public void SetAuxCamProperParent(int eyeCamId)
        {
            ResetAuxCam();
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
                    parent = _chara.transform.parent.Find("CameraBase/Camera");
                    _keepAuxCamStill = false;
                    break;

            }
            _auxCam.transform.SetParent(parent, worldPositionStays: false);
            _auxCamParent = parent;
            _auxCamParentLastPos = parent.position;
        }
        public void ResetAuxCam()
        {
            // Second object (Poi) is being reset by the game.
            _auxCam.transform.localPosition = Vector3.zero;
            _auxCamParentLastPos = _auxCamParent.position;
        }
        private void DoFixationalNeck()
        {
            SensibleH.Logger.LogDebug($"DoFixationalNeck.");
            var curEyes = _master.CurrentEyes;
            var curNeck = _master.CurrentNeck;
            if (curNeck == GirlController.DirectionNeck.Cam && !_keepAuxCamStill)
            {
                // While looking at the camera, we introduce deviation to target's position as long as the camera doesn't move too much and eyes don't look at cam.

                //SensibleH.Logger.LogDebug($"EyeCam Delta distance {lastFixCamPosDelta}");
                if (curEyes == GirlController.DirectionEye.Cam && !_master.IsNeckRecent)
                {
                    _master.OnVoiceProc();
                    SensibleH.Logger.LogDebug($"MoveAuxCam[SwitchToEyeCam]");
                }
                else// if (AuxCamDic.ContainsKey(curEyes))
                {
                    var testDist = 3f / Vector3.Distance(_auxCamParent.position, _eyes.position);
                    var vec = AuxCamDic[curEyes];
                    _auxCam.transform.localPosition += vec * testDist;
                    _auxCamParentLastPos = _auxCamParent.position;
                    SensibleH.Logger.LogDebug($"MoveAuxCam[neck[{_master.CurrentNeck}]][eyes[{curEyes}]][{vec.x}][{vec.y}] distance [{testDist}]");
                }
            }
            else
            {
                var dist = Vector3.Distance(_eyes.position, _neckLookTarget.position);
                if (curEyes == GirlController.DirectionEye.Cam && !_master.IsNeckRecent)// && Random.value < 0.66f)
                {
                    // Initiate eyeCam when eyes look at cam from other position.
                    _master.OnVoiceProc();
                    SensibleH.Logger.LogDebug($"MovePoi[SwitchToEyeCam]");
                }
                else if (AuxPoiCamDic.ContainsKey(curEyes))
                {
                    var vec = AuxPoiCamDic[curEyes];
                    _neckLookTarget.localPosition += AuxPoiCamDic[curEyes];
                    SensibleH.Logger.LogDebug($"MovePoiFollow[neck[{_master.CurrentNeck}]][eyes[{curEyes}]][{vec.x}][{vec.y}] distance [{dist}]");
                }
                else
                {
                    var vec = new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                    _neckLookTarget.localPosition += vec;
                    SensibleH.Logger.LogDebug($"MovePoiRandom[neck[{_master.CurrentNeck}]] [eyes[{curEyes}]] [{vec.x}] [{vec.y}] distance [{dist}]);");
                }
            }
            var rand = Random.Range(2f, 6f);
            _nextMoveAt = Time.time + rand;
            _master._neckNextMove += Mathf.Sqrt(rand); // * 0.1f + Random.value * 0.5f;
        }
    }
}
