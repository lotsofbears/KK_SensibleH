using KKAPI.MainGame;
using UnityEngine;
using Random = UnityEngine.Random;
using static KK_SensibleH.SensibleH;
using System.Linq;
using KKAPI;
using KKAPI.MainGame;

namespace KK_SensibleH
{
    /// <summary>
    /// In limbo for now.
    /// </summary>
    internal class MaleController : GameCustomFunctionController
    {
        private float nextMoveEye;
        private float nextMoveNeck;

        internal void LookLessDead()
        {
            if (nextMoveNeck < Time.time)
                nextMoveNeck = SetNeck();
            else if (nextMoveEye < Time.time)
                nextMoveEye = SetEyes();
        }
        private float SetNeck()
        {
            SetMalePoI();
            _chaControlM.neckLookCtrl.neckLookScript.changeTypeLeapTime = Random.Range(1.5f, 2.5f);
            return Time.time + Random.Range(10f, 20f);
        }
        private float SetEyes() 
        {
            EyeNeckPtn[2] = Random.Range(17, 29);
            return Time.time + Random.Range(2f, 5f);
        }
        private void SetMalePoI()
        {
            // Test properly in VR_PoV !!!

            // Simple dude, can look only in the eyes. The rest looks bad waaaay too often to implement. 
            // Still better then default 100m stare at "something ahead"
            Transform transform;
            var main = 0;
            if (_hFlag.mode == HFlag.EMode.houshi3P)
                main = Random.Range(0, 2);
            SensibleH.Logger.LogDebug($"chaControl[CurrentMain] = {_chaControl[main]}");
            //switch (Random.Range(0, 8))
            //{
            //    case 0:
            //        //transform = (from x in chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //        //             where x.name == "cf_d_bnip01_L"
            //        //             select x).FirstOrDefault<Transform>();

            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name == "cf_d_bnip01_L")
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    case 1:
            //        //transform = chaControl[main].objBodyBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cf_d_bnip01_R"));
            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name.Contains("cf_d_bnip01_R"))
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    case 2:
            //        //transform = chaControl[main].objBodyBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cm_J_dan_top"));
            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name.Contains("cm_J_dan_top"))
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    case 3:
            //        //transform = chaControl[main].objBodyBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cf_j_waist01"));
            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name.Contains("cf_j_waist01"))
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    case 4:
            //        //transform = chaControl[main].objBodyBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cf_j_spine02"));
            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name.Contains("cf_j_spine02"))
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    case 5:
            //        //transform = chaControl[main].objBodyBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cf_j_spine03"));
            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name.Contains("cf_j_spine03"))
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    case 6:
            //        //transform = chaControl[main].objBodyBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cf_j_neck"));
            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name.Contains("cf_j_neck"))
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    case 7:
            //        //transform = chaControl[main].objHeadBone.transform.Descendants().FirstOrDefault((Transform t) => t.name.Contains("cf_J_Eye_tz"));
            //        transform = chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
            //            .Where(x => x.name.Contains("cf_J_Eye_tz"))
            //            .Select(x => x.transform).FirstOrDefault<Transform>();
            //        break;
            //    default:
            //        transform = null;
            //        break;
            //}
            transform = _chaControl[main].objBodyBone.GetComponentsInChildren<Transform>().ToList<Transform>()
                        .Where(t => t.name.Contains("cf_J_Eye_tz"))
                        .Select(t => t.transform).FirstOrDefault<Transform>();
            SensibleH.Logger.LogDebug($"SetMalePoI: = {transform.gameObject}");
            MalePoI = transform.gameObject;
        }
    }
}
