using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CubeMapGenerator : MonoBehaviour
{
    [Tooltip("The cubemap to be exported to.")]
    [SerializeField] private Cubemap output;

    public void GenerateCubeMap()
    {
        if (output != null)
        {
            this.GetComponent<Camera>().RenderToCubemap(output);
            Debug.Log("Cubemap Exported!");
        }
        else
        {
            Debug.LogError("Output target is empty!");
        }
    }
}


[CustomEditor(typeof(CubeMapGenerator))]
public class CubeMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CubeMapGenerator generator = (CubeMapGenerator) target;
        if (GUILayout.Button ("Generate Cube Map"))
        {
            generator.GenerateCubeMap();
        }
    }
}
