using SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacentDetector : MonoBehaviour
{
    [SerializeField] private RaySensor r_up;
    [SerializeField] private RaySensor r_down;
    [SerializeField] private RaySensor r_left;
    [SerializeField] private RaySensor r_right;
    [SerializeField] private RaySensor r_forward;
    public GameObject NearestObject { get; private set; }
    private RaySensor[] sensors;
    private Vector3 offset;
    private static Vector3 zeroVector = Vector3.zero;
    private List<GameObject> detectedGameObjects = new List<GameObject>();

    #region Accessors
    public Vector3 ActualOffset 
    {
        get 
        {
            if (r_forward.DetectedObjects.Count > 0)
            {
                return zeroVector;
            }
            else
            {
                return new Vector3(Mathf.Clamp(offset.x, -1, 1),
                                   Mathf.Clamp(offset.y, -1, 1),
                                   0);
            }
        } 
    }

    public bool HasOffset { get { return offset != zeroVector; } }
    public GameObject ObjectOnLeft { get { return GetDetectedObjectOnSensor(r_left); } }
    public GameObject ObjectOnRight { get { return GetDetectedObjectOnSensor(r_right); } }
    public GameObject ObjectOnBottom { get { return GetDetectedObjectOnSensor(r_down); } }
    public GameObject ObjectAbove { get { return GetDetectedObjectOnSensor(r_up); } }
    public List<GameObject> DetectedGameObject { get { return detectedGameObjects; } }
    #endregion


    private void Start()
    {
        sensors = new RaySensor[5]
        {
            r_up,
            r_down,
            r_left,
            r_right,
            r_forward
        };
    }

    void LateUpdate()
    {
        foreach(RaySensor sensor in sensors)
        {
            sensor.Pulse();
        }

        if (detectedGameObjects.Count > 0)
        {
            detectedGameObjects.Sort(delegate(GameObject a, GameObject b)
            {
				if (a == null || b == null)
				{
					return -1;
				}
                return Vector3.Distance(a.transform.position, this.transform.position).CompareTo
                       (Vector3.Distance(b.transform.position, this.transform.position));
            });
            NearestObject = detectedGameObjects[0];
        }
        else
        {
            if (NearestObject != null)
                NearestObject = null;
        }
    }

    public void AddHorizontalOffset()
    {
        AdjustOffset(1, 0);
    }

    public void MinusHorizontalOffset()
    {
        AdjustOffset(-1, 0);
    }

    public void AddVerticalOffset()
    {
        AdjustOffset(0, 1);
    }

    public void MinusVerticalOffset()
    {
        AdjustOffset(0, -1);
    }

    public void ClearOffset()
    {
        offset = zeroVector;
    }

    public void AddDetectedObjectToList(RaySensor sensor)
    {
        if (!detectedGameObjects.Contains(sensor.DetectedObjects[0]) ||
            detectedGameObjects.Count == 0)
            detectedGameObjects.Add(sensor.DetectedObjects[0]);
    }

    public void RefreshObjectsList()
    {
        detectedGameObjects.Clear();
        foreach (RaySensor sensor in sensors)
        {
            if (sensor.DetectedObjects.Count > 0)
                AddDetectedObjectToList(sensor);
        }
    }


    private void AdjustOffset(float offsetX = 0, float offsetY = 0)
    {
        //offset.x = Mathf.Clamp(offset.x + offsetX, -1, 1);
        //offset.y = Mathf.Clamp(offset.y + offsetY, -1, 1);
        offset.x += offsetX;
        offset.y += offsetY;
    }

    private GameObject GetDetectedObjectOnSensor(RaySensor sensor)
    {
        if (sensor.DetectedObjects.Count > 0)
        {
            return sensor.DetectedObjects[0];
        }
        return null;
    }
}
