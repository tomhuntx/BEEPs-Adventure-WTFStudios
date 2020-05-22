using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagFilterer : MonoBehaviour
{
    [Header("Tag Filterer Options")]
    [Tooltip("Tick this box to make the following tags to be accepted instead of ignored.")]
    [SerializeField] private bool enableIgnore = true;

    [Tooltip("Tags of gameobjects that will be ignored. Leave blank if everything will be detected.")]
    [SerializeField] private List<string> ignoreTags = new List<string>();

    public bool DoIgnore(string tag)
    {
        if (ignoreTags.Count == 0) return false;

        bool result;
        if (ignoreTags.Contains(tag))
            result = true;
        else
            result = false;

        if (!enableIgnore)
            result = !result;

        return result;
    }
}
