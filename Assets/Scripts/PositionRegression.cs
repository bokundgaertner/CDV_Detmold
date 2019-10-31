using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;

public class PositionRegression : MonoBehaviour
{
    [SerializeField]
    private Transform frontTransfrom;
    [SerializeField]
    private Transform sideTransform;
    [SerializeField]
    private Transform backTransform;

    public Vector3 offSet;
    [SerializeField]
    private Transform building;

    private ImageTargetBehaviour frontImage;
    private ImageTargetBehaviour sideImage;
    private ImageTargetBehaviour backImage;

    [SerializeField]
    private Vector3 relPosToFront;
    [SerializeField]
    private Vector3 relRotToFront;
    [SerializeField]
    private Vector3 relPosToSide;
    [SerializeField]
    private Vector3 relRotToSide;

    private LocationService location;
    private CameraDevice myDevice;
    //Debug:
    public Text fpsText;

    public Text posText;

    public Button camOnOff;
    private bool camState = true;
    private Vector2 myPosition = new Vector2();
    void Start()
    {
        myDevice = CameraDevice.Instance;
        SwitchCamState();
        
        int targetFps = VuforiaRenderer.Instance.GetRecommendedFps(VuforiaRenderer.FpsHint.POWEREFFICIENCY);
        Application.targetFrameRate = 15;
        if (frontTransfrom != null)
            if(frontTransfrom.GetComponent<ImageTargetBehaviour>() != null)
                frontImage = frontTransfrom.GetComponent<ImageTargetBehaviour>();
        if (sideTransform != null)
            if (sideTransform.GetComponent<ImageTargetBehaviour>() != null)
                sideImage = sideTransform.GetComponent<ImageTargetBehaviour>();
        if (backTransform != null)
            if (backTransform.GetComponent<ImageTargetBehaviour>() != null)
                backImage = backTransform.GetComponent<ImageTargetBehaviour>();
        building.gameObject.SetActive(false);
        location = new LocationService();
        location.Start();
        camOnOff.onClick.AddListener(() => { SwitchCamState(); });
        StartCoroutine(UpdateMap());
    }

    private IEnumerator UpdateMap()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(5f);
            OSMWeb osm = FindObjectOfType<OSMWeb>();
            if (osm != null)
                osm.SetPosition(myPosition);
        }

    }
    private void SwitchCamState()
    {
        if(camState)
        {
            myDevice.Stop();
            camOnOff.transform.GetChild(0).GetComponent<Text>().text = "off";
            camState = false;
        }
        else
        {
            myDevice.Start();
            camOnOff.transform.GetChild(0).GetComponent<Text>().text = "on";
            camState = true;
        }
    }
    private IEnumerator StartCameraDelayed(float t)
    {
        yield return new WaitForSeconds(t);
        SwitchCamState();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        fpsText.text = (1f/Time.deltaTime) + "FPS";
        myPosition = new Vector2(location.lastData.latitude, location.lastData.longitude);
        posText.text = myPosition.x + " : " + myPosition.y;
        if ((frontImage.CurrentStatus == TrackableBehaviour.Status.TRACKED) && (sideImage.CurrentStatus == TrackableBehaviour.Status.TRACKED))
        {
            Vector3 targetPositionFront = (frontTransfrom.position + ((frontTransfrom.rotation * relPosToFront) * frontTransfrom.localScale.x));
            Vector3 targetPositionSide = (sideTransform.position + ((sideTransform.rotation * relPosToSide) * sideTransform.localScale.x));
            Vector3 averagePosition = (targetPositionFront + targetPositionSide) / 2f;
            building.transform.position = averagePosition;
            Quaternion averageQuat = Quaternion.Lerp((sideTransform.rotation * Quaternion.Euler(relRotToSide)), (frontTransfrom.rotation * Quaternion.Euler(relRotToFront)), 0.5f);
            building.transform.rotation = averageQuat;
            /*
            Debug.DrawLine(frontTransfrom.position, targetPositionFront, Color.white);
            Debug.DrawLine(sideTransform.position, targetPositionSide, Color.white);
            Debug.DrawLine(averagePosition, targetPositionSide, Color.green);
            Debug.DrawLine(averagePosition, targetPositionFront, Color.red);
            */
            if (!building.gameObject.activeSelf)
                building.gameObject.SetActive(true);
        }
        else if(frontImage.CurrentStatus == TrackableBehaviour.Status.TRACKED)
        {
            Vector3 targetPositionFront = (frontTransfrom.position + ((frontTransfrom.rotation * relPosToFront) * frontTransfrom.localScale.x));
            Debug.DrawLine(frontTransfrom.position, targetPositionFront, Color.green);
            building.transform.position = targetPositionFront;
            building.transform.rotation = frontTransfrom.rotation * Quaternion.Euler(relRotToFront);
            if (!building.gameObject.activeSelf)
                building.gameObject.SetActive(true);
        }
        else if(sideImage.CurrentStatus == TrackableBehaviour.Status.TRACKED)
        {
            Vector3 targetPositionSide = (sideTransform.position + ((sideTransform.rotation * relPosToSide) * sideTransform.localScale.x));
            Debug.DrawLine(sideTransform.position, targetPositionSide, Color.green);
            building.transform.position = targetPositionSide;
            building.transform.rotation = sideTransform.rotation * Quaternion.Euler(relRotToSide);
            if (!building.gameObject.activeSelf)
                building.gameObject.SetActive(true);
        }
        else
        {
            if (building.gameObject.activeSelf)
                building.gameObject.SetActive(false);
        }
    }

}
