using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static KK_SensibleH.SensibleH;
using KKAPI;
using KKAPI.MainGame;

namespace KK_SensibleH.EyeNeckControl
{
    internal class PoiHandler
    {// BepInEx_Manager (RuntimeUnityEditor.Core.Gizmos.lib.GizmosInstance)
        internal PoiHandler(int main)
        {
            _main = main;
            _chara = lstFemale[main];
            AddPoi(_chara.objBodyBone.transform.Find(GetPoiPath(HandCtrl.AibuColliderKind.muneL)).gameObject, new Vector3(0.025f, 0f, 0.075f));
            AddPoi(_chara.objBodyBone.transform.Find(GetPoiPath(HandCtrl.AibuColliderKind.muneR)).gameObject, new Vector3(-0.025f, 0f, 0.075f));
            AddPoi(_chara.objBodyBone.transform.Find(GetPoiPath(HandCtrl.AibuColliderKind.kokan)).gameObject, new Vector3(0f, 0f, 0.075f));
        }
        private void AddPoi(GameObject parent, Vector3 localPos)
        {
            var notCube = new GameObject("PoI_SensibleH").transform;
            notCube.name = "PoI_SensibleH";
            notCube.SetParent(parent.transform, false);
            notCube.localScale = Vector3.zero;
            notCube.localPosition = localPos;
            _listOfMyPoI.Add(notCube);

            // For sake of remembering how to create a primitive.
            //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.SetParent(parent.transform, false);
            //cube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //cube.transform.localPosition = localPos;
            //cube.GetComponent<Collider>().enabled = false;
            ////cube.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
            //cube.GetComponent<Renderer>().enabled = false;
            //cube.name = "PoI_SensibleH";
        }
        private int _main;
        private ChaControl _chara;
        private List<Transform> _listOfMyPoI = new List<Transform>();
        private Transform GetPoi(HandCtrl.AibuColliderKind aibuItem, Target target)
        {
            //SensibleH.Logger.LogDebug($"Poi:Get:{aibuItem}:{target}");
            switch (target)
            {
                case Target.Myself:
                    return _listOfMyPoI[(int)aibuItem - 2].transform;

                case Target.FemalePartner:
                    return lstFemale[_main == 0 ? 1 : 0].transform.Find(GetPoiPath(aibuItem));

                case Target.MalePartner:
                    return male.transform.Find(GetPoiPath(aibuItem));

                default:
                    return null;
            }
        }

        private enum Target
        {
            Myself,
            FemalePartner,
            MalePartner
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
        internal bool SetFemalePoI(int item)
        {
            Transform transform = null;
            switch (hFlag.mode)
            {
                case HFlag.EMode.aibu:
                case HFlag.EMode.sonyu:
                    if (item == -1)
                    {
                        var itemList = new List<int>();
                        for (var i = 0; i < 3; i++)
                        {
                            if (handCtrl.useAreaItems[i] != null)
                                itemList.Add(i);
                        }
                        var count = itemList.Count;
                        //SensibleH.Logger.LogDebug($"Poi:Set:Aibu:Options:{count}");
                        if (count > 0)
                        {
                            transform = GetPoi((HandCtrl.AibuColliderKind)(itemList[Random.Range(0, count)] + 2), Target.Myself);
                        }
                    }
                    else
                    {
                        transform = GetPoi((HandCtrl.AibuColliderKind)(item + 2), Target.Myself);
                    }
                    break;
                //case HFlag.EMode.houshi:
                //    if(hFlag.nowAnimationInfo.kindHoushi != 1)
                //    {
                //        transform = GetPoi((HandCtrl.AibuColliderKind)(Random.value < 0.5f ? 1 : 4), Target.MalePartner);
                //    }
                //    break;
                //case HFlag.EMode.sonyu:
                //    if (handCtrl.actionUseItem != -1)
                //    {
                //        var itemList = new List<int>();
                //        for (var i = 0; i < 3; i++)
                //        {
                //            if (handCtrl.useAreaItems[i] != null)
                //                itemList.Add(i);
                //        }
                //        transform = GetPoi((HandCtrl.AibuColliderKind)Random.Range(0, itemList.Count) + 2, Target.Myself);
                //        //List<Transform> lstTransform = new List<Transform>();
                //        //if (handCtrl.useAreaItems[0] != null)
                //        //    lstTransform.Add(customAccNipL.transform);
                //        //if (handCtrl.useAreaItems[1] != null)
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

                    //if (NoNeckMoveList.Contains(main == 0 ?  hFlag.nowAnimationInfo.paramFemale.path.file : hFlag.nowAnimationInfo.paramFemale1.path.file))
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
            //SensibleH.Logger.LogDebug($"Poi:Set:{transform}");
            if (transform != null)
            {
                FemalePoI[_main] = transform.gameObject;
                return true;
            }
            else
                return false;
        }
    }
}
