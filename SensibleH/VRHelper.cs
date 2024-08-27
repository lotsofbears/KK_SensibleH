
using UnityEngine;
using VRGIN.Controls;
using VRGIN.Core;
using KKAPI;
using KKAPI.MainGame;
using KK_SensibleH;

internal static class VRHelper
{
    private static Controller _controller;
    private static Controller _controller1;
    public static Vector3 GetOriginPosition()
    {
        SensibleH.Logger.LogWarning($"VRHelper:OriginPosition");
        return VR.Camera.Origin.position;
    }
    public static void SetController(Controller controller)
    {
        _controller = controller;
        _controller1 = _controller.Other;
    }
    public static bool IsTriggerPress()
    {
#if KK
        return _controller.Input.GetPress(SteamVR_Controller.ButtonMask.Trigger) || _controller1.Input.GetPress(SteamVR_Controller.ButtonMask.Trigger);
#else
        return _controller.Input.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) || _controller1.Input.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
#endif
    }
    public static bool IsGirlPoV()
    {
#if KK
        return KK_VR.Features.PoV.Instance != null && KK_VR.Features.PoV.Instance.Active && KK_VR.Features.PoV.Instance.GirlPOV;
#else
        return KKS_VR.Features.PoV.Instance != null && KKS_VR.Features.PoV.Instance.Active && KKS_VR.Features.PoV.Instance.GirlPOV;
#endif
    }
}