using System;
using HarmonyLib;
using KSP.Localization;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux.HarmonyPatches
{
    //Modify air intake behavior so wind affects intake performance.
    [HarmonyPatch(typeof(ModuleResourceIntake), nameof(ModuleResourceIntake.FixedUpdate))]
    public static class AirIntakeHijacker
    {
        static bool Prefix(ModuleResourceIntake __instance) //This is an abomination. Please msg me if you have a cleaner implementation.
        {
            //fall back to stock behavior as a failsafe
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(__instance.part.vessel);
            if (__instance == null || VH == null)
            {
                return true;
            }
            Vector3 windvec = VH.InternalAppliedWind;
            double submerged = __instance.part.submergedPortion;
            windvec.LerpWith(Vector3.zero, (float)(submerged * submerged));

            double intakechokefactor = VH.IntakeChokeFactor;
            if ((!windvec.IsFinite() || Mathf.Approximately(windvec.magnitude, 0.0f)) && (!double.IsFinite(intakechokefactor) || intakechokefactor <= 0.0))
            {
                return true;
            }

            if (__instance.intakeEnabled && __instance.moduleIsEnabled && __instance.vessel != null && __instance.intakeTransform != null)
            {
                if (!__instance.part.ShieldedFromAirstream && !(__instance.checkNode && __instance.node.attachedPart != null))
                {
                    if (__instance.vessel.staticPressurekPa >= __instance.kPaThreshold && !(!__instance.vessel.mainBody.atmosphereContainsOxygen && __instance.checkForOxygen))
                    {
                        bool inocean = __instance.vessel.mainBody.ocean && FlightGlobals.getAltitudeAtPos(__instance.intakeTransform.position, __instance.vessel.mainBody) < 0.0;

                        //Get intake resource if one of the following is true:
                        //-both disableunderwater & underwateronly are false
                        //-disableunderwater is true and we're not in ocean
                        //-disableunderwater is false, underwateronly is true, and we're in ocean
                        if ((!__instance.disableUnderwater && !__instance.underwaterOnly) || (__instance.disableUnderwater && !inocean) || (!__instance.disableUnderwater && __instance.underwaterOnly && inocean))
                        {
                            //get intake resource
                            Vector3d vel = __instance.vessel.srf_velocity - (Vector3d)windvec;
                            double sqrmag = vel.sqrMagnitude;
                            double truespeed = Math.Sqrt(sqrmag);
                            Vector3d truedir = vel / truespeed;

                            double newmach = __instance.vessel.speedOfSound != 0.0 ? truespeed / __instance.vessel.speedOfSound : 0.0;

                            double intakeairspeed = (Mathf.Clamp01(Vector3.Dot((Vector3)truedir, __instance.intakeTransform.forward)) * truespeed) + __instance.intakeSpeed;
                            __instance.airSpeedGui = (float)intakeairspeed;
                            double intakemult = intakeairspeed * (__instance.unitScalar * __instance.area * (double)__instance.machCurve.Evaluate((float)newmach));
                            double airdensity = __instance.underwaterOnly ? __instance.vessel.mainBody.oceanDensity : __instance.vessel.atmDensity;
                            __instance.resourceUnits = intakemult * airdensity * __instance.densityRecip * UtilMath.Clamp01(1.0 - intakechokefactor);

                            if (intakechokefactor >= 1.0) //100% choked. completely choked.
                            {
                                __instance.status = Localizer.Format("#LOC_AATR_IntakeCompletelyChoked");
                                __instance.resourceUnits = 0.0;
                                __instance.airFlow = 0.0f;
                                __instance.part.TransferResource(__instance.resourceId, double.MinValue);
                                return false;
                            }

                            if (__instance.resourceUnits > 0.0)
                            {
                                __instance.airFlow = (float)__instance.resourceUnits;
                                __instance.resourceUnits *= (double)TimeWarp.fixedDeltaTime;
                                if (__instance.res.maxAmount - __instance.res.amount >= __instance.resourceUnits)
                                {
                                    __instance.part.TransferResource(__instance.resourceId, __instance.resourceUnits);
                                }
                                else
                                {
                                    __instance.part.RequestResource(__instance.resourceId, -__instance.resourceUnits);
                                }
                            }
                            else
                            {
                                __instance.resourceUnits = 0.0;
                                __instance.airFlow = 0.0f;
                            }

                            int chokefactorstatus = (int)Math.Floor((intakechokefactor * 4.0) + 0.5);
                            switch (chokefactorstatus)
                            {
                                case 1: //12.5% to 37.5% choked. slightly choked
                                    __instance.status = Localizer.Format("#LOC_AATR_IntakeSlightlyChoked");
                                    break;
                                case 2: //37.5% to 62.5% choked. moderately choked
                                    __instance.status = Localizer.Format("#LOC_AATR_IntakeModeratelyChoked");
                                    break;
                                case 3: //62.5% to 87.5% choked. heavily choked
                                    __instance.status = Localizer.Format("#LOC_AATR_IntakeHeavilyChoked");
                                    break;
                                case 4: //87.5% to 100% choked. severely choked
                                    __instance.status = Localizer.Format("#LOC_AATR_IntakeSeverelyChoked");
                                    break;
                                default: //<12.5% choked. nominal
                                    __instance.status = Localizer.Format("#autoLOC_235936");
                                    break;
                            }

                            return false;
                        }
                    }
                    //drain the resource
                    __instance.airFlow = 0.0f;
                    __instance.airSpeedGui = 0.0f;
                    __instance.part.TransferResource(__instance.resourceId, double.MinValue);
                    __instance.status = Localizer.Format("#autoLOC_235946");
                    return false;
                }
                //do nothing
                __instance.airFlow = 0.0f;
                __instance.airSpeedGui = 0.0f;
                __instance.status = Localizer.Format("#autoLOC_235899");
                return false;
            }
            __instance.status = Localizer.Format("#autoLOC_8005416");
            return false;
        }
    }
}
