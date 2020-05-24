using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerRemote : MonoBehaviour
{
    public void DestroyComponent(Component target)
    {
        Destroy(target);
    }

    public void DestroyGameObject(GameObject target)
    {
        Destroy(target);
    }

    public void DestroyGameObjectsWithTag(string tag)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < targets.Length; i++)
        {
            Destroy(targets[i]);
        }
    }
}
