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

    void OnTriggerStay(Collider other)
    {
        //workaround for real time checking
        if (!other.GetComponent<Collider>().isTrigger)
		    checkerTimer = Time.time + 0.1f;
    }
}
