using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;


public class building : MonoBehaviour
{

    [SerializeField]
    private List<trackerInfo> frontTrackers; // Liste der Marker der Front Fassade

    [SerializeField]
    private List<trackerInfo> sideTrackers; // Liste der Marker der seitlichen Fassade

    [System.Serializable]
    public struct trackerInfo // struct beinhaltet die relevanten Marker Informationen
    {
        public ImageTargetBehaviour imageTarget; // AR Marker Verhalten
        public Vector3 relativePosition; // relative Position des Markers gegenüber dem 3D-Modell Mittelpunkt
        public Vector3 relativeRotation; // relative Rotation des Markers gegenüber dem 3D-Modell Mittelpunkt
    }
    
    /// <summary>
    /// Auslesen aller genutzten Marker
    /// </summary>
    /// <returns>Liste aller verwendeter Marker</returns>
    public List<ImageTargetBehaviour> GetAllTargets()
    {
        List<ImageTargetBehaviour> returnTargets = new List<ImageTargetBehaviour>();
        for(int i=0; i<frontTrackers.Count; i++)
            if (frontTrackers[i].imageTarget != null)
                returnTargets.Add(frontTrackers[i].imageTarget);
        for (int i = 0; i < sideTrackers.Count; i++)
            if (sideTrackers[i].imageTarget != null)
                returnTargets.Add(sideTrackers[i].imageTarget);
        return returnTargets;
    }


    /// <summary>
    /// Getter der relativen Position eines Markers
    /// </summary>
    /// <param name="target">ImageTargetBehaviour des Markers</param>
    /// <returns>relative Position zum 3D-Modell</returns>
    public Vector3 GetRelativePosition(ImageTargetBehaviour target)
    {
        for(int i=0;i<frontTrackers.Count; i++)
        {
            if (frontTrackers[i].imageTarget == target)
                return frontTrackers[i].relativePosition;
        }
        for (int i = 0; i < sideTrackers.Count; i++)
        {
            if (sideTrackers[i].imageTarget == target)
                return sideTrackers[i].relativePosition;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Getter der relativen Rotation eines Markers
    /// </summary>
    /// <param name="target">ImageTargetBehaviour des Markers</param>
    /// <returns>relative Rotation (Vector3) zum 3D-Modell</returns>
    public Vector3 GetRelativeRotation(ImageTargetBehaviour target)
    {
        for (int i = 0; i < frontTrackers.Count; i++)
        {
            if (frontTrackers[i].imageTarget == target)
                return frontTrackers[i].relativeRotation;
        }
        for (int i = 0; i < sideTrackers.Count; i++)
        {
            if (sideTrackers[i].imageTarget == target)
                return sideTrackers[i].relativeRotation;
        }
        return Vector3.zero;
    }

}
