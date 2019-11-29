using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIHandler : MonoBehaviour
{
    private const int targetFrameRate = 24; // FPS der Anwendung
    private float currentFPS; // derzeitige FPS
    // Tracking Screen
    [SerializeField]
    private Image trackingQualityIcon; // UI Icon Referenz (UnityEngine.Image)
    [SerializeField]
    private Color badQuality; // UI Farbreferenz (UnityEngine.Color)
    [SerializeField]
    private Color goodQuality; // UI Farbreferenz(UnityEngine.Color)
    [SerializeField]
    private GameObject[] visualObjects; // Referenz zu den GameObjects, die visuallisiert werden sollen (Karte und AR Überblendungen)
    [SerializeField]
    private Image highlightImage; // Referenz zu dem UnityEngine.Image, welches aufleuchtet, wenn ein Marker erkannt wurde
    [SerializeField]
    private Color highlgihtedScreenColor; // Farbreferenz in die das highlgihtedScreenColor verändert werden soll
    [SerializeField]
    private Color invisibleOverlay; // Farbreferenz - 100% Transparenz
    private bool foundTracker = false; // Flag ob ein Marker gefunden wurde
    // Map Elements
    [SerializeField]
    private GameObject mapGO; // Referenz zum GameObject des Kartenoverlays
    [SerializeField]
    private RectTransform myPositionErrorRT; // Referenz zum UI Element der eigenen GeoPosition auf der Karte
    [SerializeField]
    private Vector2 topLeftBorder; // Lat, Lon Begrenzung der Karte
    [SerializeField]
    private Vector2 bottomRightBorder;// Lat, Lon Begrenzung der Karte
    [SerializeField]
    private GameObject outOfBordersNotification; // Referenz zum GameObject, dass darauf hinweist, dass man nicht im Museumsgelände ist
    private bool outOfBorders = false; // Flag ob sich die GPS Koordinaten innerhalb der Grenzen befinden
    private static bool forceCenter = true; // flag ob die eigene Position Zentrum der Karte sein soll
    private static Vector2 myCurrentPos = new Vector2(); // derzeitige Lat,Lon Position
    private static Vector2 myError = new Vector2(); // GPS Genauigkeits-Vektor (in Meter)
    private static Vector2 myPrevPos = new Vector2(); // meine Position im letzten Frame
    public float zoomFactor = 1f; // Zoomstufe der Karte
    public RectTransform content; // Referenz zur ScrollView der Karte
    private float meterPerPixel = 0.155f; // Meter/Pixel Auflösung der Karte
  


    // Start is called before the first frame update
    void Start()
    {
        // Setzen der maximalen Framerate
        Application.targetFrameRate = targetFrameRate;
    }

    // Update is called once per frame
    void Update()
    {
        // Berechnung der derzeitigen Framerate
        currentFPS = (1f / Time.deltaTime);

        // Karte nur Updaten, wenn diese genutzt wird
        if (!mapGO.activeSelf)
            return;

        // Karte der Zoomstufe anpassen
        content.transform.localScale = new Vector3(zoomFactor, zoomFactor, zoomFactor);

        // UI darstellen lassen wie genau das GPS gerade die Position bestimmen kann
        myPositionErrorRT.sizeDelta = new Vector2(myError.x / meterPerPixel, myError.y / meterPerPixel);

        // blauer Positionsmarker entgegengestetzt der Karte skalieren
        myPositionErrorRT.transform.GetChild(0).localScale = Vector2.one * (2f / (zoomFactor));

        // wenn sich die Position des nutzers nicht verändert hat, braucht es keiner weiteren Updates
        if (myCurrentPos == myPrevPos)
            return;

        // prüfen ob sich der Nutzer innerhalb der GPS Grenzen aufhält
        BorderCheck();


        // Wenn die Position des nutzers zentriert dargestellt werden soll, muss die Karte entsprechend gescrollst werden
        if (forceCenter)
        {
            float w = Screen.width / 2f;
            float h = Screen.height / 2f;
            Debug.Log(w + ", " + h);
            Vector2 cVector = new Vector2(-(myPositionErrorRT.anchoredPosition.x * zoomFactor) + w, -(myPositionErrorRT.anchoredPosition.y * zoomFactor) - h);
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, cVector, Time.deltaTime * 10f);
            Debug.Log(cVector + " vs " + myPositionErrorRT.anchoredPosition * zoomFactor);
        }

        //  neues Setzen der alten Position für nächsten Frame
        myPrevPos = myCurrentPos;
    }


    /// <summary>
    /// Methode die prüft ob sich der Nutzer innerhalb der definierten Grenzen befindet.
    /// Wenn nicht wird ein UI Overlay aktiviert, welches dem Nutzer das mitteilt.
    /// </summary>
    private void BorderCheck()
    {
        if (myCurrentPos.x < topLeftBorder.x || myCurrentPos.x > bottomRightBorder.x || myCurrentPos.y < bottomRightBorder.y || myCurrentPos.y > topLeftBorder.y)
        {
            outOfBordersNotification.SetActive(true);
        }
        else
        {
            outOfBordersNotification.SetActive(false);
        }
    }

    /// <summary>
    /// Methode, die die Karte und GPS-Tracking Aufruft oder Abschaltet und entsprechend anders herum die Kamera ansteuert
    /// </summary>
    public void ShowMap()
    {
        mapGO.SetActive(!mapGO.activeSelf);
        if (!mapGO.activeSelf)
        {
            Vuforia.CameraDevice.Instance.Start();
            FindObjectOfType<DevicePositionHandler>().StopLocationTracking();
        }

        else
        {
            Vuforia.CameraDevice.Instance.Stop();
            FindObjectOfType<DevicePositionHandler>().StartLocationTracking();
        }

    }
    /// <summary>
    /// Derzeitige GPS Koordinaten und Genauigkeit setzen
    /// </summary>
    /// <param name="position">Lat, Lon Position</param>
    /// <param name="error">Genauigkeit in m´Meter</param>
    public void SetMyCurrentPosition(Vector2 position, Vector2 error)
    {
        myCurrentPos = position;
        myError = error;
    }

    /// <summary>
    /// Karten-Zoom-Funktion
    /// </summary>
    public void ZoomOut()
    {

        if (zoomFactor <= 0.1f)
            return;
        zoomFactor -= 0.1F;
    }

    /// <summary>
    /// Karten-Zoom-Funktion
    /// </summary>
    public void ZoomIn()
    {
        if (zoomFactor >= 1f)
            return;
        zoomFactor += 0.1F;
    }

    /// <summary>
    /// Zentrieren der Karte auf die Position des Nutzers
    /// </summary>
    public void CentreMapScreen()
    {
        forceCenter = !forceCenter;
    }


    /// <summary>
    /// Das Icon, welches die Trackingqualität darstellen soll anhand des Qualitätsparameters setzen.
    /// </summary>
    /// <param name="q">Qualitätsparameter 0<q<1 </param>
    public void SetTrackingQualityIcon(float q)
    {
        // Qualtitätsparameter anhand der relativen FPS Performance anpassen
        q *= (currentFPS / targetFrameRate);
        if (q > 1f)
            q = 1f;
        if (q == 0)
        {
            trackingQualityIcon.color = Color.black;
        }
        else if (q < 0)
            trackingQualityIcon.color = badQuality;
        else
            trackingQualityIcon.color = Color.Lerp(badQuality, goodQuality, q);
    }

    /// <summary>
    /// Anzeigen/Ausblenden der AR Overlays aus dem Array "visualObjects", alle anderen werden deaktiviert.
    /// </summary>
    /// <param name="elementNumber"></param>
    public void ShowElement(int elementNumber)
    {
        if (elementNumber > visualObjects.Length)
            return;
        for (int i = 0; i < visualObjects.Length; i++)
        {
            if (i == elementNumber)
            {
                if (visualObjects[i].activeSelf)
                    visualObjects[i].SetActive(false);
                else
                    visualObjects[i].SetActive(true);
            }

            else
            {
                visualObjects[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Starten der Animation, dass ein Marker gefunden wurde.
    /// </summary>
    public void FoundTrackerHighlighting()
    {
        if (!foundTracker)
        {
            foundTracker = true;
            StartCoroutine(HighlightAnimation());
        }

    }

    /// <summary>
    /// Coroutine zum highlighten
    /// </summary>
    /// <returns></returns>
    private IEnumerator HighlightAnimation()
    {
        float counter = 0f;
        while (counter < 1)
        {
            yield return new WaitForEndOfFrame();
            highlightImage.color = Color.Lerp(invisibleOverlay, highlgihtedScreenColor, counter);
            counter += 0.2f;
        }
        while (counter > 0)
        {
            yield return new WaitForEndOfFrame();
            highlightImage.color = Color.Lerp(invisibleOverlay, highlgihtedScreenColor, counter);
            counter -= 0.25f;
        }
        highlightImage.color = invisibleOverlay;
        foundTracker = false;
    }
}
