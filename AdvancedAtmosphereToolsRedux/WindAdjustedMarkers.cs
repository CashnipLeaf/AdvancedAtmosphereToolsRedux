using KSP.UI.Screens.Flight;
using UnityEngine;

namespace AdvancedAtmosphereToolsRedux
{
    // wind adjusted prograde and retrograde markers
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class WindAdjustedMarkers : MonoBehaviour
    {
        private NavBall navBall;
        private GameObject progradewind;
        private GameObject retrogradewind;
        private Vector3 navBallLocalScale = new Vector3(44, 44, 44);

        private Material progrademat;
        private Material retrogrademat;

        void Start()
        {
            Settings.CheckGameSettings();
            GameEvents.onUIScaleChange.Add(ResizeIndicators);
        }

        void LateUpdate()
        {
            if (FlightGlobals.fetch != null && FlightGlobals.ready && FlightGlobals.speedDisplayMode == FlightGlobals.SpeedDisplayModes.Surface && Settings.AdjustedIndicatorsEnabled)
            {
                Vessel activevessel = FlightGlobals.ActiveVessel;
                AtmoToolsRedux_VesselHandler VH = FlightSceneHandler.GetVesselHandler(activevessel);
                if (activevessel != null && VH != null)
                {
                    Vector3 windvec = VH.InternalAppliedWind;
                    if (activevessel.mainBody.atmosphere && activevessel.altitude <= activevessel.mainBody.atmosphereDepth && windvec.IsFinite() && windvec.magnitude >= 0.5f)
                    {
                        if (navBall == null || progradewind == null || retrogradewind == null)
                        {
                            navBall = FindObjectOfType<NavBall>();

                            //set up the indicators.
                            progradewind = Instantiate(navBall.progradeVector.gameObject);
                            progradewind.transform.parent = navBall.progradeVector.parent;
                            progradewind.transform.position = navBall.progradeVector.position;

                            retrogradewind = Instantiate(navBall.retrogradeVector.gameObject);
                            retrogradewind.transform.parent = navBall.retrogradeVector.parent;
                            retrogradewind.transform.position = navBall.retrogradeVector.position;

                            progrademat = progradewind.GetComponent<MeshRenderer>().materials[0];
                            retrogrademat = retrogradewind.GetComponent<MeshRenderer>().materials[0];
                        }
                        ResizeIndicators();

                        progradewind.transform.localScale = navBallLocalScale;
                        retrogradewind.transform.localScale = navBallLocalScale;

                        Vector3 srfv = FlightGlobals.ship_srfVelocity;
                        Vector3 displayV = srfv - windvec;
                        Vector3 displayVnormalized = displayV / displayV.magnitude;

                        bool vthresholdmet = srfv.magnitude > navBall.VectorVelocityThreshold;

                        float opacity1 = Mathf.Clamp01(Vector3.Dot(progradewind.transform.localPosition.normalized, Vector3.forward));
                        progrademat.SetFloat("_Opacity", opacity1);
                        progrademat.SetColor("_TintColor", Settings.ProgradeMarkerColor);
                        progradewind.SetActive(progradewind.transform.localPosition.z > navBall.VectorUnitCutoff && vthresholdmet);
                        progradewind.transform.localPosition = navBall.attitudeGymbal * (displayVnormalized * navBall.VectorUnitScale);

                        float opacity2 = Mathf.Clamp01(Vector3.Dot(retrogradewind.transform.localPosition.normalized, Vector3.forward));
                        retrogrademat.SetFloat("_Opacity", opacity2);
                        retrogrademat.SetColor("_TintColor", Settings.ProgradeMarkerColor);
                        retrogradewind.SetActive(retrogradewind.transform.localPosition.z > navBall.VectorUnitCutoff && vthresholdmet);
                        retrogradewind.transform.localPosition = navBall.attitudeGymbal * (-displayVnormalized * navBall.VectorUnitScale);

                        return;
                    }
                }
            }

            progradewind?.SetActive(false);
            retrogradewind?.SetActive(false);
        }

        void OnDestroy()
        {
            if (progradewind != null)
            {
                Destroy(progradewind);
            }
            if (retrogradewind != null)
            {
                Destroy(retrogradewind);
            }
            GameEvents.onUIScaleChange.Remove(ResizeIndicators);
        }

        void ResizeIndicators()
        {
            float navballDefaultSize = 44f * GameSettings.UI_SCALE_NAVBALL;
            navBallLocalScale = new Vector3(navballDefaultSize, navballDefaultSize, navballDefaultSize);
        }
    }
}
