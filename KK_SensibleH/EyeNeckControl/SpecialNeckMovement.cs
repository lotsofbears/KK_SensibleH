using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KK_SensibleH.EyeNeckControl; 
using static KK_SensibleH.EyeNeckControl.EyeNeckDictionaries;
using static KK_SensibleH.EyeNeckControl.EyeNeckController;
using Random = UnityEngine.Random;
using VRGIN.Core;

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
        internal GameObject _auxCam;


        private bool _vr;
        private bool _keepAuxCamStill; // don't move it when parent is kokan.

        private int _main;
        private float _nextMoveAt;
        private float _moveDelta;

        private GirlController _master;
        private EyeNeckController _neck;
        private ChaControl _chara;
        private Transform _eyes;
        private Transform _neckLookTarget;
        private Transform _auxCamParent;
        private Vector3 _auxCamParentLastPos;
        private HMotionEyeNeckFemale _eyeNeckMotion;
        internal SpecialNeckMovement(GirlController master, EyeNeckController neck, int main, bool vr)
        {
            _master = master;
            _neck = neck;
            _main = main;
            _vr = vr;
            _chara = SensibleH._chaControl[main];
            _eyeNeckMotion = main == 0 ? SensibleH._eyeneckFemale : SensibleH._eyeneckFemale1;
            _neckLookTarget = _chara.objNeckLookTarget.transform;
            _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            //var camera = _chara.transform.parent.Find("CameraBase/Camera");

            var fixCam = new GameObject().transform;
            fixCam.name = "FixationalNeckMovement";
            fixCam.localScale = Vector3.zero;
            _auxCam = fixCam.gameObject;
            if (vr)
                _moveDelta = 0.05f;
            else
                _moveDelta = 0.15f;
            SetAuxCamProperParent(17);
        }
        /// <summary>
        /// Reparent AuxCamera to chara, position stays.
        /// </summary>
        public void OnKissVrAuxCam()
        {
            // Using VR cam for VR feature is kinda given.. too fed up to try the other way.
            var cam = _auxCam.transform;
            var camVR = VR.Camera.transform;
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
            if (_neck._neckActive && _neck.CurrentNeck != DirectionNeck.Pose && _neck.CurrentNeck != DirectionNeck.Mid)
            {
                if (_nextMoveAt < Time.time)
                {
                    DoFixationalNeck();
                }
                else if (_neck.CurrentNeck == DirectionNeck.Cam && Vector3.Distance(_auxCamParent.position, _auxCamParentLastPos) > _moveDelta)
                {
                    // That is whatever AuxCam is being attached to has moved.
                    ResetAuxCam();
                }
            }
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
                        // There are some weird inconsistencies in vr, native camera doesn't always align perfectly/align at all.
                        // One of the bugs is probably me not patching all the camera teleportation away, still remains somewhere.
                        // Minor misalignments are real conundrum, because positions are the same.
                        // Solved in the most sober way, by using VR toolset in VR mode.
                        parent = VR.Camera.transform;
                    }
                    else
                        parent = _chara.transform.parent.Find("CameraBase/Camera");

                    _keepAuxCamStill = false;
                    break;

            }
            _auxCam.transform.SetParent(parent, worldPositionStays: false);
            _auxCamParent = parent;
            ResetAuxCam();
        }
        public void ResetAuxCam()
        {
            // Second object (Poi) is being reset by the game.
            _auxCam.transform.localPosition = Vector3.zero;
            _auxCamParentLastPos = _auxCamParent.position;
        }
        private void DoFixationalNeck()
        {
            var voice = _master._voiceController.IsVoiceActive;
            if (!voice || (voice && Random.value < 0.5f))
            {
                var curEyes = _neck.CurrentEyes;
                //SensibleH.Logger.LogDebug($"DoFixationalNeck. {_master.CurrentNeck} move - {_keepAuxCamStill == false}");
                if (_neck.CurrentNeck == DirectionNeck.Cam)
                {
                    if (!_keepAuxCamStill)
                    {
                        var dist = Vector3.Distance(_auxCamParent.position, _eyes.position);

                        var vec = GetAuxCamDic(curEyes);
                        _auxCam.transform.localPosition += vec * (dist / 0.8f);
                        if (_auxCam.transform.position.y < 0f)
                        {
                            _auxCam.transform.localPosition += Vector3.up * (Random.value * 0.2f);
                        }
                        _auxCamParentLastPos = _auxCamParent.position;
                        SensibleH.Logger.LogDebug($"MoveAuxCam[neck[{_neck.CurrentNeck}]][eyes[{curEyes}]][{vec.x}][{vec.y}] dist[{dist}]");
                    }

                }
                else
                {
                    var dist = Vector3.Distance(_eyes.position, _neckLookTarget.position);
                    if (curEyes == DirectionEye.Cam && !_neck.IsNeckRecent)// && Random.value < 0.66f)
                    {
                        // Attempt to initiate eyeCam when eyes look at cam from other position.
                        _neck.LookAtCam();
                        SensibleH.Logger.LogDebug($"MovePoi[SwitchToEyeCam]");
                    }
                    else// if (AuxPoiCamDic.ContainsKey(curEyes))
                    {
                        var vec = GetAuxPoiDic(curEyes);
                        _neckLookTarget.localPosition += vec;
                        SensibleH.Logger.LogDebug($"MovePoiFollow[neck[{_neck.CurrentNeck}]][eyes[{curEyes}]][{vec.x}][{vec.y}] distance [{dist}]");
                    }
                    //else
                    //{
                    //    var vec = new Vector3(-0.15f + Random.value * 0.3f, -0.15f + Random.value * 0.3f);
                    //    _neckLookTarget.localPosition += vec;
                    //    SensibleH.Logger.LogDebug($"MovePoiRandom[neck[{_master.CurrentNeck}]] [eyes[{curEyes}]] [{vec.x}] [{vec.y}] distance [{dist}]);");
                    //}
                }
            }
            var rand = Random.Range(2f, 6f);
            _nextMoveAt = Time.time + rand;
            _neck._neckNextMove += Mathf.Sqrt(rand); // * 0.1f + Random.value * 0.5f;
        }
    }
}
