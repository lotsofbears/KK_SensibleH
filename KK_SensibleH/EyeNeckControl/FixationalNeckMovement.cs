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
    internal class FixationalNeckMovement
    {
        internal GameObject _fixMoveCamera;
        private GirlController _master;
        private Transform _eyes;
        private Transform _shoulders;
        private Transform _neckLookTarget;
        private ChaControl _chara;
        private int _main = 0;
        private float _nextMoveAt;
        internal FixationalNeckMovement(GirlController girlController, int main)
        {
            _main = main;
            _chara = SensibleH._chaControl[main];
            _master = girlController;
            _neckLookTarget = _chara.objNeckLookTarget.transform; // objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/" +
                //"cf_j_spine03/cf_s_spine03/N_NeckLookTargetP/N_NeckLookTarget");
            _shoulders = _chara.objBodyBone.transform.Find("cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_backsk_00");
            _eyes = _chara.objHeadBone.transform.Find("cf_J_N_FaceRoot/cf_J_FaceRoot/cf_J_FaceBase/cf_J_FaceUp_ty/cf_J_FaceUp_tz/cf_J_Eye_tz");
            //var camera = _chara.transform.parent.Find("CameraBase/Camera");

            var fixCam = new GameObject().transform;
            fixCam.name = "FixationalNeckMovement";
            fixCam.localScale = Vector3.zero;
            _fixMoveCamera = fixCam.gameObject;
            ParentFixMoveCam();

            //SensibleH.Logger.LogDebug($"FixNeckMove[Awake] {_neckLookTarget}");
        }
        /// <summary>
        /// Reparent camera for FixationalNeckMove to HScene, position stays.
        /// </summary>
        public void UnParentFixMoveCam()
        {
            // Using VR cam for VR feature is kinda given.. too fed up to try the other way.
            var cam = _fixMoveCamera.transform;
            var camVR = VR.Camera.transform;
            //var camVR = _chara.transform.parent.Find("CameraBase/Camera");
            // test camera movements outside VR.
           // cam.position = camVR.position;
            cam.position = camVR.position;
            cam.position += camVR.position + camVR.forward * -0.8f + camVR.up * 0.5f - cam.position;
            //SensibleH.Logger.LogDebug($"UnParentFixMoveCam position before = {cam.position} actualVrCam {actualCamVR.position}");
            //_fixMoveCamera.transform.SetParent(_chara.transform.parent, worldPositionStays: true);
            //_fixMoveCamera.transform.SetParent(_shoulders, worldPositionStays: true);
            cam.SetParent(_chara.objTop.transform, worldPositionStays: true);
            //var vec = _chara.objTop.transform.position - cam.position;
            //SensibleH.Logger.LogDebug($"UnParentFixMoveCam[localPos after{cam.position} distance[{Vector3.Distance(cam.position, _eyes.position)}");
            //cam.localPosition += vec * 5f;
        }
        /// <summary>
        /// Add to local position.
        /// </summary>
        public void MoveFixMoveCam(Vector3 position)
        {
            //var camPos = _fixMoveCamera.transform.localPosition;
            _fixMoveCamera.transform.position += position;
        }
        /// <summary>
        /// Reparent camera for FixationalNeckMove to Camera, position resets.
        /// </summary>
        public void ParentFixMoveCam()
        {
            var cam = _fixMoveCamera.transform;
            SensibleH.Logger.LogDebug($"ParentFixMoveCam[{Vector3.Distance(cam.position, _eyes.position)}");
            cam.localPosition = Vector3.zero;
            if (SensibleHController.Instance._vr)
            {
                cam.SetParent(VR.Camera.transform, worldPositionStays: false);
            }
            else
                cam.SetParent(_chara.transform.parent.Find("CameraBase/Camera"), worldPositionStays: false);
        }
        public void Proc()
        {
            if (_master._neckActive && _nextMoveAt < Time.time)
            {
                MovePoi();
            }
        }
        public void ResetFixCamera()
        {
            // The other one is done by the game by default.
            _fixMoveCamera.transform.localPosition = Vector3.zero;
        }
        private void MovePoi()
        {
            SensibleH.Logger.LogDebug($"MovePoi[{_fixMoveCamera.transform.position}]");
            var result = true;
            var curEyes = _master.CurrentEyes;
            if (_master.CurrentNeck == GirlController.DirectionNeck.Cam)
            {
                if (curEyes != GirlController.DirectionEye.Cam && curEyes != GirlController.DirectionEye.Mid && FixNeckEyeCamDic.ContainsKey(curEyes))
                {
                    var vec = FixNeckEyeCamDic[curEyes];
                    _fixMoveCamera.transform.localPosition += vec * (0.2f + Vector3.Distance(_fixMoveCamera.transform.position, _eyes.position));
                    SensibleH.Logger.LogDebug($"MoveFixCam[neck[{_master.CurrentNeck}]] [eyes[{curEyes}]] [{vec.x}] [{vec.y}]");
                }
                else
                {
                    SensibleH.Logger.LogDebug($"MoveFixCam[WontMove]");
                    ResetFixCamera();
                    result = false;
                }
            }
            else
            {
                if (curEyes == GirlController.DirectionEye.Cam && !_master.IsNeckRecent && Random.value < 0.66f)
                {
                    _master.OnVoiceProc(forced: true);
                    SensibleH.Logger.LogDebug($"MovePoi[SwitchToEyeCam]");
                }
                else if (FixNeckPoiCamDic.ContainsKey(curEyes))
                {
                    var vec = FixNeckPoiCamDic[curEyes];
                    _neckLookTarget.localPosition += FixNeckPoiCamDic[curEyes];
                    SensibleH.Logger.LogDebug($"MovePoiFollow[neck[{_master.CurrentNeck}]] [eyes[{curEyes}]] [{vec.x}] [{vec.y}]");
                }
                else
                {
                    var vec = new Vector3(-0.3f + Random.value * 0.6f, -0.3f + Random.value * 0.6f);
                    _neckLookTarget.localPosition += vec;
                    SensibleH.Logger.LogDebug($"MovePoiRandom[neck[{_master.CurrentNeck}]] [eyes[{curEyes}]] [{vec.x}] [{vec.y}]");
                }
            }
            var rand = Random.Range(2f, 6f);
            _nextMoveAt = Time.time + rand;
            if (result)
                _master.moveNeckNext += Mathf.Sqrt(rand); // * 0.1f + Random.value * 0.5f;
        }
    }
}
