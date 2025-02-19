using System;
using UnityEngine;
using ModularFI;
using HarmonyLib;
using System.Reflection;

namespace AdvancedAtmosphereToolsRedux
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class FlightDynamicsOverrides : MonoBehaviour
    {
        public static bool registeredoverrides = false;

        void Start()
        {
            CheckFAR();
            if (!registeredoverrides) //make sure that things dont get patched more than once. That would be very bad.
            {
                Utils.LogInfo("Initializing Flight Dynamics Overrides.");
                //register overrides with ModularFI
                try
                {
                    Utils.LogInfo("Registering AdvancedAtmosphereToolsRedux with ModularFlightIntegrator.");
                    //If FAR is installed, do not override aerodynamics. Leave the aerodynamics calulations to FAR.
                    if (!Settings.FAR_Exists)
                    {
                        if (ModularFlightIntegrator.RegisterUpdateAerodynamicsOverride(NewUpdateAerodynamics))
                        {
                            ModularFlightIntegrator.RegisterCalculateAerodynamicAreaOverride(AerodynamicAreaOverride);
                            Utils.LogInfo("Successfully registered AdvAtmoTools:Redux's Aerodynamics Overrides with ModularFlightIntegrator.");
                        }
                        else
                        {
                            Utils.LogWarning("Unable to register AdvAtmoTools:Redux's Aerodynamics Override with ModularFlightIntegrator.");
                        }
                    }
                    if (ModularFlightIntegrator.RegisterCalculatePressureOverride(CalcPressureOverride))
                    {
                        Utils.LogInfo("Successfully registered AdvAtmoTools:Redux's Pressure Override with ModularFlightIntegrator.");
                    }
                    else
                    {
                        Utils.LogWarning("Unable to register AdvAtmoTools:Redux's Pressure Override with ModularFlightIntegrator.");
                    }

                    if (ModularFlightIntegrator.RegistercalculateConstantsAtmosphereOverride(CalculateConstantsAtmosphereOverride))
                    {
                        Utils.LogInfo("Successfully registered AdvAtmoTools:Redux's Atmosphere and Thermodynamics Overrides with ModularFlightIntegrator.");
                    }
                    else
                    {
                        Utils.LogWarning("Unable to register AdvAtmoTools:Redux's Atmosphere and Thermodynamics Overrides with ModularFlightIntegrator.");
                    }
                    Utils.LogInfo("ModularFlightIntegrator Registration Complete.");
                }
                catch (Exception ex)
                {
                    Utils.LogError("ModularFlightIntegrator Registration Failed. Exception thrown: " + ex.ToString());
                }

                Utils.LogInfo("Patching Lifting Surface, Air Intake, and Kerbal Breathing behavior.");
                try
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    Harmony harmony = new Harmony("AdvAtmoTools:Redux");
                    harmony.PatchAll(assembly);
                    Utils.LogInfo("Patching Complete.");
                }
                catch (Exception ex)
                {
                    Utils.LogError($"Patching Failed. Exception thrown: {ex}");
                }
                registeredoverrides = true;
                Destroy(this);
            }
            else
            {
                Utils.LogWarning("Destroying duplicate Flight Dynamics Overrides. Check your install for duplicate mod folders.");
                DestroyImmediate(this);
            }
        }

        #region aerodynamics
        static void NewUpdateAerodynamics(ModularFlightIntegrator fi, Part part)
        {
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(fi.Vessel);

            if (VH != null)
            {
                //recalculate part static pressure
                double altitudeAtPos = FlightGlobals.getAltitudeAtPos((Vector3d)part.partTransform.position, fi.CurrentMainBody);
                //i dont wanna have to recalculate the pressure all over again for each part, so this is probably good enough. I'd rather not sandbag the runtime.
                double staticpress = fi.CurrentMainBody.GetPressure(altitudeAtPos) * VH.FIPressureMultiplier;
                if (fi.CurrentMainBody.ocean && altitudeAtPos <= 0.0)
                {
                    staticpress += fi.Vessel.gravityTrue.magnitude * -altitudeAtPos * fi.CurrentMainBody.oceanDensity;
                }
                staticpress *= 0.0098692326671601278;
                if (double.IsFinite(staticpress))
                {
                    part.staticPressureAtm = staticpress;
                }
            }
            
            //resume business as normal
            fi.BaseFIUpdateAerodynamics(part);
        }

        //Takes advantage of CalculateAerodynamicArea()'s placement inside UpdateAerodynamics() to inject a new drag vector into the part before UpdateAerodynamics() uses to calculate anything.
        static double AerodynamicAreaOverride(ModularFlightIntegrator fi, Part part)
        {
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(fi.Vessel);
            Vector3 windvec = VH != null ? VH.InternalAppliedWind : Vector3.zero;
            double submerged = part.submergedPortion;
            windvec.LerpWith(Vector3.zero, (float)(submerged * submerged));

            //add an offset to the velocity vector used for body drag/lift calcs and update the related fields.
            if (part.Rigidbody != null && windvec.IsFinite() && !Mathf.Approximately(windvec.magnitude, 0.0f))
            {
                part.dragVector = part.Rigidbody.velocity + Krakensbane.GetFrameVelocity() - windvec;
                part.dragVectorSqrMag = part.dragVector.sqrMagnitude;
                if (part.dragVectorSqrMag != 0.0)
                {
                    part.dragVectorMag = Mathf.Sqrt(part.dragVectorSqrMag);
                    part.dragVectorDir = part.dragVector / part.dragVectorMag;
                    part.dragVectorDirLocal = -part.partTransform.InverseTransformDirection(part.dragVectorDir);
                }
                else
                {
                    part.dragVectorMag = 0.0f;
                    part.dragVectorDir = part.dragVectorDirLocal = Vector3.zero;
                }
                part.dragScalar = 0.0f;
                if (!part.ShieldedFromAirstream && !(part.atmDensity <= 0 && part.submergedPortion <= 0.0) && !part.DragCubes.None)
                {
                    //update the drag from the drag cubes if they exist
                    part.DragCubes.SetDrag(part.dragVectorDirLocal, (float)fi.mach);
                }
            }

            //inlined CaclulateAerodynamicArea() to avoid passing an object reference again
            if (!part.DragCubes.None)
            {
                return part.DragCubes.Area;
            }
            else
            {
                switch (part.dragModel)
                {
                    case Part.DragModel.DEFAULT:
                    case Part.DragModel.CUBE:
                        return (!PhysicsGlobals.DragCubesUseSpherical && !part.DragCubes.None) ? part.DragCubes.Area : part.maximum_drag;
                    case Part.DragModel.CONIC:
                        return part.maximum_drag;
                    case Part.DragModel.CYLINDRICAL:
                        return part.maximum_drag;
                    case Part.DragModel.SPHERICAL:
                        return part.maximum_drag;
                    default:
                        return part.maximum_drag;
                }
            }
        }
        #endregion

        #region thermodynamics
        static void CalculateConstantsAtmosphereOverride(ModularFlightIntegrator fi)
        {
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(fi.Vessel);
            Vector3 windvec = VH != null ? VH.InternalAppliedWind : Vector3.zero;
            if (windvec.IsFinite() && !Mathf.Approximately(windvec.magnitude, 0.0f))
            {
                fi.Vel -= windvec;
                fi.Vessel.speed = fi.spd = !fi.Vessel.IgnoreSpeedActive ? fi.Vel.magnitude : 0.0; ;
                fi.nVel = (fi.spd != 0.0) ? fi.Vel / (float)fi.spd : Vector3.zero;
            }

            fi.CurrentMainBody.GetSolarAtmosphericEffects(fi.sunDot, fi.density, out fi.solarAirMass, out fi.solarFluxMultiplier);
            fi.Vessel.solarFlux = (fi.solarFlux *= fi.solarFluxMultiplier);
            fi.Vessel.atmosphericTemperature = fi.atmosphericTemperature = VH != null ? VH.Temperature : fi.CurrentMainBody.GetFullTemperature(fi.altitude, fi.atmosphereTemperatureOffset);

            double molarmass = VH != null ? VH.MolarMass : fi.CurrentMainBody.atmosphereMolarMass;
            fi.density = fi.Vessel.atmDensity = AtmoToolsReduxUtils.GetDensity(fi.staticPressurekPa, fi.atmosphericTemperature, molarmass);
            fi.Vessel.dynamicPressurekPa = fi.dynamicPressurekPa = 0.0005 * fi.density * fi.spd * fi.spd;

            double adiabaticIndex = VH != null ? VH.AdiabaticIndex : fi.CurrentMainBody.atmosphereAdiabaticIndex;
            fi.Vessel.speedOfSound = AtmoToolsReduxUtils.GetSpeedOfSound(fi.staticPressurekPa, fi.density, adiabaticIndex);
            fi.Vessel.mach = fi.mach = fi.Vessel.speedOfSound > 0.0 ? fi.spd / fi.Vessel.speedOfSound : 0.0;

            fi.convectiveMachLerp = Math.Pow(UtilMath.Clamp01((fi.mach - PhysicsGlobals.NewtonianMachTempLerpStartMach) / (PhysicsGlobals.NewtonianMachTempLerpEndMach - PhysicsGlobals.NewtonianMachTempLerpStartMach)), PhysicsGlobals.NewtonianMachTempLerpExponent);
            fi.Vessel.externalTemperature = fi.externalTemperature = Math.Max(fi.atmosphericTemperature, fi.BaseFICalculateShockTemperature());
            fi.Vessel.convectiveCoefficient = fi.convectiveCoefficient = CalculateConvectiveCoeff(fi);
            fi.pseudoReynolds = fi.density * fi.spd;
            fi.pseudoReLerpTimeMult = 1.0 / (PhysicsGlobals.TurbulentConvectionEnd - PhysicsGlobals.TurbulentConvectionStart);
            fi.pseudoReDragMult = (double)PhysicsGlobals.DragCurvePseudoReynolds.Evaluate((float)fi.pseudoReynolds);
        }

        static double CalculateConvectiveCoeff(ModularFlightIntegrator fi) //I would love to clean this up, but it works and I dont wanna touch it.
        {
            double coeff;
            double density = fi.density;
            double spd = fi.spd;
            if (fi.Vessel.situation == Vessel.Situations.SPLASHED)
            {
                coeff = (PhysicsGlobals.ConvectionFactorSplashed * PhysicsGlobals.NewtonianConvectionFactorBase + Math.Pow(spd, PhysicsGlobals.NewtonianVelocityExponent) * 10.0) * PhysicsGlobals.NewtonianConvectionFactorTotal;
            }
            else if (fi.convectiveMachLerp == 0.0)
            {
                coeff = CalculateConvecCoeffNewtonian(density, spd);
            }
            else if (fi.convectiveMachLerp == 1.0)
            {
                coeff = CalculateConvecCoeffMach(density, spd);
            }
            else
            {
                coeff = UtilMath.LerpUnclamped(CalculateConvecCoeffNewtonian(density, spd), CalculateConvecCoeffMach(density, spd), fi.convectiveMachLerp);
            }
            return coeff * fi.CurrentMainBody.convectionMultiplier;
        }
        static double CalculateConvecCoeffNewtonian(double density, double spd)
        {
            double coeff = density <= 1.0 ? Math.Pow(density, PhysicsGlobals.NewtonianDensityExponent) : density;
            double multiplier = PhysicsGlobals.NewtonianConvectionFactorBase + Math.Pow(spd, PhysicsGlobals.NewtonianVelocityExponent);
            return coeff * multiplier * PhysicsGlobals.NewtonianConvectionFactorTotal;
        }
        static double CalculateConvecCoeffMach(double density, double spd)
        {
            double coeff = density <= 1.0 ? Math.Pow(density, PhysicsGlobals.MachConvectionDensityExponent) : density;
            return coeff * 1E-07 * PhysicsGlobals.MachConvectionFactor * Math.Pow(spd, PhysicsGlobals.MachConvectionVelocityExponent);
        }

        static void CalcPressureOverride(ModularFlightIntegrator fi)
        {
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(fi.Vessel);
            if (VH == null)
            {
                fi.BaseFICalculatePressure();
                return;
            }

            if (fi.CurrentMainBody.atmosphere && fi.altitude <= fi.CurrentMainBody.atmosphereDepth)
            {
                fi.staticPressurekPa = VH.Pressure;
                fi.staticPressureAtm = fi.staticPressurekPa * 0.0098692326671601278;
            }
            else
            {
                fi.staticPressureAtm = fi.staticPressurekPa = 0.0;
            }
        }
        #endregion

        #region utility
        internal static void CheckFAR()
        {
            Utils.LogInfo("Checking for an instance of FerramAerospaceResearch.");
            try
            {
                Type FARAtm = null;
                foreach (var assembly in AssemblyLoader.loadedAssemblies)
                {
                    if (assembly.name == "FerramAerospaceResearch")
                    {
                        var types = assembly.assembly.GetExportedTypes();
                        foreach (Type t in types)
                        {
                            if (t.FullName.Equals("FerramAerospaceResearch.FARWind"))
                            {
                                FARAtm = t;
                            }
                            if (t.FullName.Equals("FerramAerospaceResearch.FARAtmosphere"))
                            {
                                FARAtm = t;
                            }
                        }
                    }
                }
                Settings.FAR_Exists = FARAtm != null;
                Utils.LogInfo(Settings.FAR_Exists ? "FerramAerospaceResearch detected. Flight Dynamics calculations will be deferred to FAR." : "No instances of FerramAerospaceResearch detected.");
            }
            catch (Exception ex)
            {
                Utils.LogError("Exception thrown when checking for FerramAerospaceResearch: " + ex.ToString());
                Settings.FAR_Exists = false;
            }
        }
        #endregion
    }
}
