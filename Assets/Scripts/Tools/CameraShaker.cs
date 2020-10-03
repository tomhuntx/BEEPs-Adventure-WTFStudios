using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static List<CameraShaker> Instances = new List<CameraShaker>();

    [SerializeField] private float shakeFalloff = 2.0f;
    [Range(0, 2)] [SerializeField] private float intensity = 1.0f;
    private float currentMagnitude = 0;
    private Vector3 originalLocalPos;
    private int previousIndex = 0;
    private static Vector3[] shakeDir = new Vector3[8]
    {
        new Vector2 (1, 0),
        new Vector2 (0, 1),
        new Vector2 (-1, 0),
        new Vector2 (0, -1),
        new Vector2 (1, 1),
        new Vector2 (-1, -1),
        new Vector2 (1, -1),
        new Vector2 (-1, 1),
    };


    private void Awake()
    {
        if (!Instances.Contains(this)) Instances.Add(this);
    }

    private void Start()
    {
        originalLocalPos = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMagnitude > 0)
        {
            int newIndex = Random.Range(0, shakeDir.Length);
            while(previousIndex == newIndex) newIndex = Random.Range(0, shakeDir.Length);
            this.transform.localPosition = originalLocalPos + shakeDir[newIndex] * currentMagnitude;
            currentMagnitude -= shakeFalloff * Time.deltaTime;
            previousIndex = newIndex;

            if (currentMagnitude < 0)
            {
                currentMagnitude = 0;
                this.transform.localPosition = originalLocalPos;
            }
        }
    }



    public void DoExplosionShake(float magnitude, Vector3 explosionSourcePosition)
    {
        if (!this.isActiveAndEnabled) return;
        float distance = Vector3.Distance(Player.Instance.transform.position, explosionSourcePosition);
        currentMagnitude += magnitude / distance;
        currentMagnitude = Mathf.Clamp(currentMagnitude, 0, intensity);
    }
}
