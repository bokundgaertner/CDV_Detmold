using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasse zur Handhabung des GPS
/// </summary>
public class DevicePositionHandler : MonoBehaviour
{
    private LocationService deviceLocation; // Referenz zur GPS-Tracking Instanz
    public float updateTime = 1f; // GPS Update Zeit in Sekunden
    private float time = 0f; // Zeit Counter

    // Start is called before the first frame update
    void Start()
    {
        // Contructor der GPS-Tracking Instanz
        deviceLocation = new LocationService();
    }

    /// <summary>
    /// Methode die das GPS-Tracking startet
    /// </summary>
    public void StartLocationTracking()
    {
        deviceLocation.Start(3f, 6f);
        StartCoroutine(UpdatePosition());
    }

    /// <summary>
    /// Methode die das GPS-Tracking stoppt
    /// </summary>
    public void StopLocationTracking()
    {
        StopAllCoroutines();
        deviceLocation.Stop();
    }

    /// <summary>
    /// Coroutine, die im "updateTime" Intervall die GPS-Position und Genauigkeit abfragt und diese an den UIHandler übermittelt
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdatePosition()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(updateTime);
            Vector2 p = new Vector2(deviceLocation.lastData.latitude, deviceLocation.lastData.longitude);
            Vector2 e = new Vector2(deviceLocation.lastData.verticalAccuracy, deviceLocation.lastData.horizontalAccuracy);
            FindObjectOfType<UIHandler>().SetMyCurrentPosition(p, e);
        }
    }
}
