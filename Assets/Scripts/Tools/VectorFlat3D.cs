using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representation of 3D vector and points with one axis set to 0.
/// </summary>
[System.Serializable]
public struct VectorFlat3D
{
    public enum Axis { x, y, z }
    /// <summary>
    /// Gets the distance of two Vector3 positions with the targeted axis value zeroed out.
    /// </summary>
    /// <param name="flattenedAxis">The axis where its value will be zeroed out.</param>
    public static float GetFlattenedDistance(Vector3 pos1, Vector3 pos2, Axis flattenedAxis)
    {
        pos1 = FlattenVector(pos1, flattenedAxis);
        pos2 = FlattenVector(pos2, flattenedAxis);
        return Vector3.Distance(pos1, pos2);
    }

    /// <summary>
    /// Sets an axis's value to zero.
    /// </summary>
    /// <param name="flattenedAxis">The axis where its value will be zeroed out.</param>
    public static Vector3 FlattenVector(Vector3 vector, Axis flattenedAxis)
    {
        switch (flattenedAxis)
        {
            case Axis.x:
                vector.x = 0;
                break;

            case Axis.y:
                vector.y = 0;
                break;

            case Axis.z:
                vector.z = 0;
                break;
        }
        return vector;
    }
}
