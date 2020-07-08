using UnityEngine;

/// <summary>
/// Representation of directions in 3D.
/// </summary>
[System.Serializable]
public struct Direction
{
    #region Variables
    Vector3 originPos;
    Vector3 normDir;
    Vector3 scaledDir;
    #endregion



    #region Accessors
    /// <summary>
    /// The world space this direction is at.
    /// </summary>
    public Vector3 relativePosition { get { return originPos; } set { originPos = value; } }

    /// <summary>
    /// The normalized direction in local space.
    /// </summary>
    public Vector3 localDirection { get { return normDir; } }

    /// <summary>
    /// The normalized direction in world space.
    /// </summary>
    public Vector3 worldDirection { get { return originPos + normDir; } }

    /// <summary>
    /// The scaled or non-normalized direction in local space.
    /// </summary>
    public Vector3 localScaledDirection { get { return normDir * scaledDir.magnitude; } }

    /// <summary>
    /// The scaled or non-normalized direction in world space.
    /// </summary>
    public Vector3 worldScaledDirection { get { return originPos + scaledDir; } }
    #endregion



    /// <summary>
    /// Create a new direction with the given Vector3 values.
    /// </summary>
    /// <param name="from">The point of origin.</param>
    /// <param name="to">The position where the direction is facing.</param>
    public Direction(Vector3 from, Vector3 to)
    {
        originPos = from;
        scaledDir = to - from;
        normDir = Vector3.Normalize(scaledDir);
    }



    #region Public Methods
    /// <summary>
    /// Overwrites the current direction values with the given Vector3 values.
    /// </summary>
    /// <param name="from">The point of origin.</param>
    /// <param name="to">The position where the direction is facing.</param>
    public void SetDirection(Vector3 from, Vector3 to)
    {
        originPos = from;
        scaledDir = to - from;
        normDir = Vector3.Normalize(scaledDir);
    }

    /// <summary>
    /// Tweak the current direction without affecting its relative position.
    /// </summary>
    /// <param name="to">The position where the direction is facing.</param>
    public void SetDirection(Vector3 to)
    {
        SetDirection(originPos, to);
    }
    #endregion
}
