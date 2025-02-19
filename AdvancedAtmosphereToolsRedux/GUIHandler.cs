using System;
using UnityEngine;
using KSP.Localization;
using KSP.UI.Screens;
using ToolbarControl_NS;

namespace AdvancedAtmosphereToolsRedux
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class GUIHandler : MonoBehaviour
    {
        private ToolbarControl toolbarController;
        private bool toolbarButtonAdded = false;
        private bool GUIEnabled = false;

        internal const string LogoPath = "AdvancedAtmosphereToolsRedux/Textures/AATR_Logo";
        internal const string modNAME = "AdvAtmoToolsRedux";
        internal const string modID = "AdvAtmoToolsRedux_NS";

        private string UIHeader => $"AdvAtmoToolsRedux v{Utils.Version}";

        private Rect windowPos;
        private static float Xpos => 100f * GameSettings.UI_SCALE;
        private static float Ypos => 100f * GameSettings.UI_SCALE;
        private static float Xwidth => 285.0f * Mathf.Clamp(GameSettings.UI_SCALE, 0.75f, 1.25f);
        private static float Yheight => 60f * GameSettings.UI_SCALE;
        private static int FontSize => (int)(12.0f * Mathf.Clamp(GameSettings.UI_SCALE, 0.75f, 1.25f));

        private static string Speedunit => Localizer.Format("#LOC_AATR_meterspersec");
        private static string Pressunit => Localizer.Format("#LOC_AATR_kpa");
        private static string Forceunit => Localizer.Format("#LOC_AATR_kilonewton");
        private static string Tempunit => Localizer.Format("#LOC_AATR_kelvin");
        private static string DensityUnit => Localizer.Format("#LOC_AATR_kgmcubed");
        private static string Degreesstr => Localizer.Format("°");
        private static string Minutesstr => Localizer.Format("'");
        private static string Secondsstr => Localizer.Format("″");
        private static readonly string[] directions = { "N", "S", "E", "W" };
        private static readonly string[] cardinaldirs = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

        void Start()
        {
            //add to toolbar
            ApplicationLauncher.AppScenes scenes = ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW;
            toolbarController = gameObject.AddComponent<ToolbarControl>();
            if (!toolbarButtonAdded)
            {
                toolbarController.AddToAllToolbars(ToolbarButtonOnTrue, ToolbarButtonOnFalse, scenes, modID, "991295", LogoPath, LogoPath, modNAME);
                toolbarButtonAdded = true;
            }
            windowPos = new Rect(Xpos, Ypos, Xwidth, Yheight);
            Settings.buttondisablewindstationary = Settings.buttonindicatorsenabled = false;
        }

        void OnDestroy()
        {
            RemoveToolbarButton();
            GameEvents.onGUIApplicationLauncherDestroyed.Remove(RemoveToolbarButton);
        }

        void OnGUI()
        {
            GUI.skin.label.margin = new RectOffset(2, 2, 2, 2);
            GUI.skin.label.fontSize = FontSize;
            if (GUIEnabled)
            {
                windowPos = GUILayout.Window("AdvAtmoToolsRedux".GetHashCode(), windowPos, DrawWindow, UIHeader);
            }
        }

        //TODO: Add font scaling
        void DrawWindow(int windowID)
        {
            GUIStyle button = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 6, 0),
                margin = new RectOffset(2, 2, 2, 2),
                stretchWidth = true,
                stretchHeight = false,
                fontSize = FontSize
            }; //Unity does not allow calling GUI functions outside of OnGUI(). FML

            GUILayout.BeginVertical();

            //toggle the DisableWindWhenStationary setting
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Settings.DisableWindWhenStationary ? Localizer.Format("#LOC_AATR_enablewind") : Localizer.Format("#LOC_AATR_disablewind"), button))
            {
                Settings.buttondisablewindstationary = !Settings.buttondisablewindstationary;
            }
            GUILayout.EndHorizontal();

            //toggle the wind-adjusted prograde indicators
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Settings.AdjustedIndicatorsEnabled ? Localizer.Format("#LOC_AATR_disableindicators") : Localizer.Format("#LOC_AATR_enableindicators"), button))
            {
                Settings.buttonindicatorsenabled = !Settings.buttonindicatorsenabled;
            }
            GUILayout.EndHorizontal();

            Vessel Activevessel = FlightGlobals.ActiveVessel;
            AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(Activevessel);

            if (Activevessel != null && Activevessel.mainBody != null && VH != null)
            {
                CelestialBody mainbody = Activevessel.mainBody;
                bool inatmo = mainbody.atmosphere && Activevessel.staticPressurekPa > 0.0;
                string altitude = string.Format(Math.Abs(Activevessel.altitude) > 1000000d ? "{0:0.#####E+00} {1}" : "{0:F2} {1}", Activevessel.altitude, Localizer.Format("#LOC_AATR_meter"));
                
                Vector3 craftdragvector = Activevessel.srf_velocity;
                Vector3 craftdragvectorwind = Activevessel.srf_velocity - VH.InternalAppliedWind;
                Vector3 craftdragvectortransformed = VH.LocalToWorld.inverse * craftdragvector;

                double alpha = 0.0;
                double slip = 0.0;
                Vector3 totaldrag = Vector3.zero;
                Vector3 totallift = Vector3.zero;
                double liftforce = 0.0;
                double dragforce = 0.0;
                double liftdragratio = 0.0;
                double liftinduceddrag = 0.0;
                string bodyname = mainbody.displayName.Split('^')[0];

                double grndspd = Math.Sqrt(Math.Pow(craftdragvectortransformed.x, 2) + Math.Pow(craftdragvectortransformed.z, 2));
                string groundspeed = inatmo ? string.Format("{0:F1} {1}", grndspd, Speedunit) : Localizer.Format("#LOC_AATR_na");
                string TAS = inatmo ? string.Format("{0:F1} {1}", craftdragvectorwind.magnitude, Speedunit) : Localizer.Format("#LOC_AATR_na");
                string mach = inatmo ? string.Format("{0:F2}", Activevessel.mach) : Localizer.Format("#LOC_AATR_na");
                double trk = craftdragvector.magnitude > 0.0 ? UtilMath.WrapAround(Math.Atan2(craftdragvectortransformed.z, craftdragvectortransformed.x) * UtilMath.Rad2Deg, 0.0, 360.0) : 0.0;
                string track = inatmo && craftdragvector.magnitude > 0.1 ? string.Format("{0:F1} {1}", trk, Degreesstr) : Localizer.Format("#LOC_AATR_na");

                string windspeed = string.Format("{0:F1} {1}", VH.RawWind.magnitude, Speedunit);
                string v_windspeed = string.Format("{0:F1} {1}", VH.RawWind.y, Speedunit);
                string h_windspeed = string.Format("{0:F1} {1}", Math.Sqrt(Math.Pow(VH.RawWind.x, 2) + Math.Pow(VH.RawWind.z, 2)), Speedunit);

                bool istherewind = VH.RawWind.x != 0.0 || VH.RawWind.z != 0.0;
                double heading = istherewind ? UtilMath.WrapAround((Math.Atan2(VH.RawWind.z, VH.RawWind.x) * UtilMath.Rad2Deg) + 180.0, 0.0, 360.0) : 0.0;

                string windheading = istherewind ? string.Format("{0:F1} {1}", heading, Degreesstr) : Localizer.Format("#LOC_AATR_na");
                string winddirection = istherewind ? cardinaldirs[(int)((heading / 22.5) + .5) % 16] : Localizer.Format("#LOC_AATR_na");

                string statictemp = string.Format("{0:F1} {1}", Activevessel.atmosphericTemperature, Tempunit);
                string exttemp = string.Format("{0:F1} {1}", Activevessel.externalTemperature, Tempunit);
                string staticpress = string.Format("{0:F3} {1}", Activevessel.staticPressurekPa, Pressunit);
                string dynamicpress = string.Format("{0:F3} {1}", Activevessel.dynamicPressurekPa, Pressunit);
                string density = string.Format("{0:F3} {1}", Activevessel.atmDensity, DensityUnit);
                string soundspeed = string.Format("{0:F1} {1}", Activevessel.speedOfSound, Speedunit);

                if (craftdragvectorwind.magnitude > 0.01)
                {
                    Vector3d nvel = (Activevessel.srf_velocity - VH.InternalAppliedWind).normalized;
                    Vector3d forward = (Vector3d)Activevessel.transform.forward;
                    Vector3d vector3d = Vector3d.Exclude((Vector3d)Activevessel.transform.right, nvel);
                    Vector3d normalized1 = vector3d.normalized;
                    alpha = Math.Asin(Vector3d.Dot(forward, normalized1)) * UtilMath.Rad2Deg;
                    alpha = double.IsNaN(alpha) ? 0.0 : alpha;

                    Vector3d up = (Vector3d)Activevessel.transform.up;
                    vector3d = Vector3d.Exclude(forward, nvel);
                    Vector3d normalized2 = vector3d.normalized;
                    slip = Math.Acos(Vector3d.Dot(up, normalized2)) * UtilMath.Rad2Deg;
                    slip = double.IsNaN(slip) ? 0.0 : slip;

                    if (Activevessel.atmDensity > 0.0)
                    {
                        foreach (Part p in Activevessel.Parts)
                        {
                            totaldrag.Add(p.dragScalar * -p.dragVectorDir);
                            if (!p.hasLiftModule)
                            {
                                totallift.Add(Vector3.ProjectOnPlane(p.transform.rotation * (p.bodyLiftScalar * p.DragCubes.LiftForce), -p.dragVectorDir));
                            }
                            foreach (var m in p.Modules)
                            {
                                if (m is ModuleLiftingSurface wing)
                                {
                                    totallift.Add(wing.liftForce);
                                    totaldrag.Add(wing.dragForce);
                                }
                            }
                        }
                        Vector3d totalforce = totallift + totaldrag;
                        Vector3d normalized = Vector3d.Exclude(nvel, totallift).normalized;
                        liftforce = Vector3d.Dot(totalforce, normalized);
                        dragforce = Vector3d.Dot(totalforce, -nvel);
                        liftinduceddrag = Vector3d.Dot(totallift, -nvel);
                        liftdragratio = Math.Abs(liftforce) > 0.0001 ? liftforce / dragforce : 0.0;
                    }
                }

                string aoa = string.Format("{0:F2} {1}", alpha, Degreesstr);
                string sideslip = string.Format("{0:F2} {1}", slip, Degreesstr);
                string lift = string.Format("{0:F2} {1}", liftforce, Forceunit);
                string drag = string.Format("{0:F2} {1}", dragforce, Forceunit);
                string lid = string.Format("{0:F2} {1}", liftinduceddrag, Forceunit);
                string ldratio = string.Format("{0:F2}", liftdragratio);

                //Ground Track
                DrawHeader(Localizer.Format("#LOC_AATR_grdtrk"));
                DrawElement(Localizer.Format("#LOC_AATR_body"), Localizer.Format(bodyname));
                //TODO: add biome info (?)
                DrawElement(Localizer.Format("#LOC_AATR_lon"), DegreesString(Activevessel.longitude, 1)); //east/west
                DrawElement(Localizer.Format("#LOC_AATR_lat"), DegreesString(Activevessel.latitude, 0)); //north/south
                DrawElement(Localizer.Format("#LOC_AATR_alt"), altitude);

                //Velocity Information
                DrawHeader(Localizer.Format("#LOC_AATR_vel"));
                DrawElement(Localizer.Format("#LOC_AATR_tas"), TAS);
                DrawElement(Localizer.Format("#LOC_AATR_gs"), groundspeed);
                DrawElement(Localizer.Format("#LOC_AATR_mach"), mach);
                DrawElement(Localizer.Format("#LOC_AATR_track"), track);

                //Wind Information
                DrawHeader(Localizer.Format("#LOC_AATR_windinfo"));
                if (inatmo)
                {
                    DrawElement(Localizer.Format("#LOC_AATR_windspd"), windspeed);
                    DrawElement(Localizer.Format("#LOC_AATR_windvert"), v_windspeed);
                    DrawElement(Localizer.Format("#LOC_AATR_windhoriz"), h_windspeed);
                    DrawElement(Localizer.Format("#LOC_AATR_heading"), windheading);
                    DrawElement(Localizer.Format("#LOC_AATR_cardinal"), winddirection);
                }
                else
                {
                    DrawCentered(Localizer.Format("#LOC_AATR_invac"));
                    GUILayout.FlexibleSpace();
                }

                //aerodynamics
                DrawHeader(Localizer.Format("#LOC_AATR_aero"));
                DrawElement(Localizer.Format("#LOC_AATR_staticpress"), staticpress);
                DrawElement(Localizer.Format("#LOC_AATR_dynamicpress"), dynamicpress);
                DrawElement(Localizer.Format("#LOC_AATR_density"), density);
                DrawElement(Localizer.Format("#LOC_AATR_statictemp"), statictemp);
                DrawElement(Localizer.Format("#LOC_AATR_exttemp"), exttemp);
                DrawElement(Localizer.Format("#LOC_AATR_soundspeed"), soundspeed);

                DrawCentered("----------"); //gap between pressure/temperature and aero forces

                DrawElement(Localizer.Format("#LOC_AATR_aoa"), aoa);
                DrawElement(Localizer.Format("#LOC_AATR_sideslip"), sideslip);
                DrawElement(Localizer.Format("#LOC_AATR_lift"), lift);
                DrawElement(Localizer.Format("#LOC_AATR_drag"), drag);
                DrawElement(Localizer.Format("#LOC_AATR_LID"), lid);
                DrawElement(Localizer.Format("#LOC_AATR_lifttodrag"), ldratio);
            }
            else
            {
                DrawCentered(Localizer.Format("#LOC_AATR_NoVessel"));
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        //GUILayout functions because things look neater this way.
        private void DrawHeader(string tag)
        {
            GUILayout.BeginHorizontal();
            GUI.skin.label.margin = new RectOffset(5, 5, 5, 5);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label(tag);
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUILayout.EndHorizontal();
            GUI.skin.label.margin = new RectOffset(2, 2, 2, 2);
        }
        private void DrawElement(string tag, string value)
        {
            GUILayout.BeginHorizontal();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label(tag);
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUILayout.Label(value);
            GUILayout.EndHorizontal();
        }
        private void DrawCentered(string tag)
        {
            GUILayout.BeginHorizontal();
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(tag);
            GUILayout.EndHorizontal();
        }

        private void RemoveToolbarButton() //Remove from toolbar
        {
            if (toolbarButtonAdded)
            {
                toolbarController.OnDestroy();
                Destroy(toolbarController);
                toolbarButtonAdded = false;
            }
        }

        private void ToolbarButtonOnTrue() => GUIEnabled = true;
        private void ToolbarButtonOnFalse() => GUIEnabled = false;

        //display the longitude and latitude information as either degrees or degrees, minutes, and seconds + direction
        private static string DegreesString(double deg, int axis)
        {
            double degrees = Math.Floor(Math.Abs(deg));
            double minutes = Math.Abs((deg % 1) * 60.0);
            double seconds = Math.Floor(Math.Abs(((deg % 1) * 3600.0) % 60.0));
            string dir = directions[(2 * axis) + (deg < 0.0 ? 1 : 0)];
            switch (Settings.Minutesforcoords)
            {
                case Settings.DegreesDisplay.DegreesMinutesSeconds:
                    return string.Format("{0:F0}{1} {2:F0}{3} {4:F0}{5} {6}", degrees, Degreesstr, Math.Floor(minutes), Minutesstr, seconds, Secondsstr, dir);
                case Settings.DegreesDisplay.DegreesMinutes:
                    return string.Format("{0:F0}{1} {2:F1}{3} {4}", degrees, Degreesstr, minutes, Minutesstr, dir);
                default:
                    return string.Format("{0:F2}{1}", deg, Degreesstr);
            }
        }
    }
}
