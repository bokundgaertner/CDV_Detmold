using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;


/// <summary>
/// Klasse die das Objekt-Tracking (Marker) und das Positionieren der 3D-Modelle handhabt
/// </summary>
public class PositionRegression : MonoBehaviour
{
    [SerializeField]
    private Transform building; // Referenz zur Position des 3D-Modells
    private building savedBuildings; // Referenz zur Klasse der gespeicherten Modelle

    private List<ImageTargetBehaviour> targets; // Referenzliste zu allen bild-Markern

    private Tracker trackerDevice; // Referenz zur Instanz des Vuforia Tracking Moduls

    private float timeTillLastTrack = 0f; // Counter der verstrichenen Zeit zum letzten erfolgreichen Erfassens eines Markers

    [SerializeField]
    private const float maximumDelay = 15f; // Maximaler Dauer der extrapolation der Position eines Markers

    // Start is called before the first frame update
    void Start()
    {
        // starten des Vuforia Tracking Moduls
        trackerDevice = TrackerManager.Instance.InitTracker<PositionalDeviceTracker>();

        // Referenzbildung der Marker
        savedBuildings = FindObjectOfType<building>();
        targets = savedBuildings.GetAllTargets();

        // 3D-Modelle deaktieren
        building.gameObject.SetActive(false);
    }



    /// <summary>
    /// Erfassen und Verfolgen der Marker, sowie anpassen der Position und Rotation der 3D-Modelle
    /// </summary>
    void LateUpdate()
    {
        // resetten der Tracking Qualität
        float trackingQuality = 0f;

        // resetten der derzeit erfassten Marker und ihrer Tracking-Stati
        List<ImageTargetBehaviour> trackedObjects = new List<ImageTargetBehaviour>();
        List<TrackableBehaviour.Status> statList = new List<TrackableBehaviour.Status>();


        // Liste der erfassten Marker durchgehen
        for (int i = 0; i < targets.Count; i++)
        {
            // Falls die Referenz verloren geht, soll diese übersprungen werden
            if (targets[i] == null)
                continue;
            // Setzen des Marker Status
            TrackableBehaviour.Status targetStatus = targets[i].CurrentStatus;

            // Falls der Marker erfasst oder extrapoliert wird ...
            if (targetStatus == TrackableBehaviour.Status.TRACKED || targetStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                // Marker wird der derzeitigen Liste erfasster Marker hinzugefügt und auch deren Status gespeichert
                trackedObjects.Add(targets[i]);
                statList.Add(targetStatus);

                // Wenn die Position des Markers erfasst (und nicht extrapoliert) wurde, wird die Qualitäts-Counter hochgesetzt
                if (targetStatus == TrackableBehaviour.Status.TRACKED)
                {
                    trackingQuality += 0.2f;

                    // Sonderregel für die Erfassung der vollen Fassade, da diese als besonders guter Marker gewertet wird
                    if (targets[i].gameObject.name.Contains("full"))
                    {
                        trackingQuality += 0.2f;
                    }

                }
                // bei der Extrapolation wird die Qualität heruntergestuft
                else
                    trackingQuality -= 0.1f;
            }

        }

        // Setzen des UI Symbols für die Tracking-Qualität
        FindObjectOfType<UIHandler>().SetTrackingQualityIcon(trackingQuality);

        // Flag ob mindestens ein Objekt erfasst wurde, oder ob nur extrapoliert wird
        bool isTracking = statList.Contains(TrackableBehaviour.Status.TRACKED);


        if (trackedObjects.Count == 0 && !trackerDevice.IsActive)
        {
            trackerDevice.Start();
            timeTillLastTrack += Time.deltaTime;
            building.gameObject.SetActive(false);
            return;
        }
        // Falls kein Objekt getrackt wird, werden die 3D-Modelle deaktiviert, der Zeitcounter hochgesetzt und die Methode verlassen
        else if (trackedObjects.Count == 0)
        {
            timeTillLastTrack += Time.deltaTime;
            building.gameObject.SetActive(false);
            return;
        }
        // Falls nur extrapoliert wird, wird der Zeitcounter hochgesetzt und nach einem "maximumDelay" die Trackerinstanz resettet
        else if(!isTracking)
        {
            timeTillLastTrack += Time.deltaTime;
            if (timeTillLastTrack >= maximumDelay)
            {
                trackerDevice.Stop();
                building.gameObject.SetActive(false);
                return;
            }
        }
        // Wenn etwas gefunden wurde, soll das UI kurz aufleuchten (highlighting) und das 3D Modell aktiviert werden
        else
        {
            if (timeTillLastTrack > 0f)
                FindObjectOfType<UIHandler>().FoundTrackerHighlighting();
            timeTillLastTrack = 0f;
            building.gameObject.SetActive(true);
        }

        // resetten der Referenz der relativen Position und Rotation des 3D-Modells
        Vector3 renderPosition = new Vector3(0f, 0f, 0f);
        Quaternion renderRotation = Quaternion.identity;

        // Gewichtung der einzelnen Marker zur Bestimmung der Position und Rotation
        float weight = 1f / (float)trackedObjects.Count;

        // Positions- und Rotationsreferenz anhand der gewichteten relativen Werte anpassen (Aufsummieren)
        for (int i = 0; i < trackedObjects.Count; i++)
        {
            Vector3 pos = savedBuildings.GetRelativePosition(trackedObjects[i]);
            Vector3 rot = savedBuildings.GetRelativeRotation(trackedObjects[i]);

            renderPosition += (trackedObjects[i].transform.position + ((trackedObjects[i].transform.rotation * pos)));
            renderRotation *= Quaternion.Slerp(Quaternion.identity, (trackedObjects[i].transform.rotation * Quaternion.Euler(rot)), weight);
        }

        // Mitteln der Position des 3D-Modells
        renderPosition = renderPosition / trackedObjects.Count;

        // Um ein Springen des 3D-Modells entgegenzuwirken, wird dessen Position und Rotation zwischen vorheriger und neuer Position interpoliert
        building.transform.position = Vector3.Lerp(building.transform.position, renderPosition, Time.deltaTime * 5f);
        building.transform.rotation = Quaternion.Lerp(building.transform.rotation, renderRotation, Time.deltaTime * 5f);
    }

}
