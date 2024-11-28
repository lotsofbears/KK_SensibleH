
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Core;
using KKAPI;
using KKAPI.MainGame;
using KK_SensibleH;

internal static class VRHelper
{
    //private static Controller _controller;
    //private static Controller _controller1;
    //public static Vector3 GetOriginPosition()
    //{
    //    SensibleH.Logger.LogWarning($"VRHelper:OriginPosition");
    //    return VR.Camera.Origin.position;
    //}
    //public static void SetController(Controller controller)
    //{
    //    _controller = controller;
    //    _controller1 = _controller.Other;
    //}

    public static bool IsGirlPoV()
    {
        return KK_VR.Features.PoV.Active && KK_VR.Features.PoV.GirlPoV;
    }
}