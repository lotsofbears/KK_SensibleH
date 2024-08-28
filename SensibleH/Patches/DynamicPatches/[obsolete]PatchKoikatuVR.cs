//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Text;
//using HarmonyLib;

//namespace KK_SensibleH.Patches.DynamicPatches
//{
//    class PatchKoikatuVR
//    {
//        /// <summary>
//        /// We remove synthetic mouse up click that is being fed directly to "HandCtrl".
//        /// </summary>
//#if KK
//        [HarmonyPrefix, HarmonyPatch(typeof(KK_VR.Caress.HandCtrlHooks), nameof(KK_VR.Caress.HandCtrlHooks.InjectMouseButtonUp))]
//#else
//        [HarmonyPrefix, HarmonyPatch(typeof(KKS_VR.Caress.HandCtrlHooks), nameof(KKS_VR.Caress.HandCtrlHooks.InjectMouseButtonUp))]
//#endif
//        public static bool InjectMouseButtonUpPrefix()
//        {
//            return false;
//        }
//    }
//}
