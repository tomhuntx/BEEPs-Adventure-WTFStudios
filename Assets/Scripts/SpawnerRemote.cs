using UnityEngine;

public class SpawnerRemote : MonoBehaviour
{
    /// <summary>
    /// Spawns prefab to this game object as child.
    /// </summary>
    public void SpawnToObject(GameObject prefab)
    {
        Instantiate(prefab, this.transform.localPosition, this.transform.localRotation, this.transform);
    }

    /// <summary>
    /// Spawns prefab to where this game object is at.
    /// </summary>
    /// <param name="prefab"></param>
    public void SpawnToPoint(GameObject prefab)
    {
        Instantiate(prefab, this.transform.position, this.transform.rotation);
    }
}
