using UnityEngine;

public class PathManager : MonoBehaviour
{
    [Header("Assign the BoardPath parent object here")]
    public Transform pathParent;

    [HideInInspector] public Transform[] pathPoints;

    void Awake()
    {
        if (pathParent == null)
        {
            Debug.LogError("PathManager: pathParent is NOT assigned in Inspector!");
            return;
        }

        int count = pathParent.childCount;
        if (count == 0)
        {
            Debug.LogWarning("PathManager: pathParent has NO children. Please check hierarchy.");
            return;
        }

        pathPoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            pathPoints[i] = pathParent.GetChild(i);
        }
    }
    public int GetIndexOfTransform(Transform target)
    {
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == target)
                return i;
        }
        return -1; // Not found
    }
}
