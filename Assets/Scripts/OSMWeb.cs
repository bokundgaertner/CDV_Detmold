using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class OSMWeb : MonoBehaviour
{
    private bool loadingMap = false;

    [SerializeField]
    private RawImage mapImage;

    private Vector2 prevPos = new Vector2();
    // Start is called before the first frame update

    public void SetPosition(Vector2 pos)
    {
        float[] renderSpace = GetMapBoundaries(pos.x, pos.y);
        /*
        if (Vector2.Distance(pos, prevPos) <= 0.0001f)
            return;
            */
        prevPos = new Vector2(pos.x, pos.y);
        StartCoroutine(GetOSMap(renderSpace));
    }
    private float[] GetMapBoundaries(float lon, float lat)
    {
        float[] boundarybox = new float[4];
        boundarybox[0] = lon - 0.0069f;
        boundarybox[1] = lat - 0.02221f;
        boundarybox[2] = lon + 0.0069f;
        boundarybox[3] = lat + 0.02221f;

        return boundarybox;
    }



    private IEnumerator GetOSMap(float[] box)
    {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");
        string url = "https://www.wms.nrw.de/geobasis/wms_nw_dtk25?REQUEST=GetMap&VERSION=1.3.0&LAYERS=nw_dtk25_col,nw_dtk25_info&STYLES=&CRS=EPSG:4326" +
            "&BBOX=" + box[0] +"," + box[1] +"," + box[2]+","+box[3] +"&WIDTH=800&HEIGHT=800&FORMAT=image/png";
        loadingMap = true;
        WWW www = new WWW(url);
        while (www.progress != 1)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("finished " + www.size);
        loadingMap = false;
        mapImage.texture = www.texture;
        yield break;
    }
}
