using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using KK_SensibleH;

namespace KK_SensibleH.Patches
{
    internal class PatchHNoVR
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddKiss))]
        public static void HFlagAddKissPostfix()
        {
            SensibleHController.Instance.OnHandCtrlAction(HandCtrl.AibuColliderKind.mouth);
        }
    }
}
