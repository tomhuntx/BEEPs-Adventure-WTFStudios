using UnityEngine;

public class BoxPlacementChecker : MonoBehaviour
{
    public bool isPlacable = true;
    private float checkerTimer;

    private void Update()
    {
        if (checkerTimer < Time.time)
        {
            isPlacable = true;
        }
        else
        {
            isPlacable = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //workaround for real time checking
        checkerTimer = Time.time + 0.1f;
    }
}
