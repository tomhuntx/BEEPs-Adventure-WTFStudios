using UnityEngine;

public static class TrajectoryTarget
{
    /// <summary>
    /// Calculates the trajectory angle based on a pre-determined initial and final position.
    /// </summary>
    /// <param name="initialPos">Where the object will be initially thrown.</param>
    /// <param name="finalPos">Where the object should land.</param>
    /// <param name="speed">How fast the object travels.</param>
    /// <param name="gravity">How much gravity will be applied to the object.</param>
    /// <returns>The optimal angle for the object to be thrown at to land on the given position.</returns>
    public static float GetTrajectoryAngle(Vector3 initialPos, Vector3 finalPos, float speed, float gravity = 9.81f)
    {
        float distance = Vector3.Distance(initialPos, finalPos);
        float angle = 0.5f * (Mathf.Asin((gravity * distance) / (speed * speed)));
        return angle * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Calculates the trajectory angle based on the given distance.
    /// </summary>
    /// <param name="distance">How far the object will be thrown.</param>
    /// <param name="speed">How fast the object travels.</param>
    /// <param name="gravity">How much gravity will be applied to the object.</param>
    /// <returns>The optimal angle for the object to be thrown at to land on the given distance.</returns>
    public static float GetTrajectoryAngle(float distance, float speed, float gravity = 9.81f)
    {
        float angle = 0.5f * (Mathf.Asin((gravity * distance) / (speed * speed)));
        return angle * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Rotates the target transform to the optimal angle for the object to be thrown at to land on the given position.
    /// </summary>
    /// <param name="target">The transform to be oriented to the trajectory angle.</param>
    /// <param name="initialPos">Where the object will be initially thrown.</param>
    /// <param name="finalPos">Where the object should land.</param>
    /// <param name="speed">How fast the object travels.</param>
    /// <param name="gravity">How much gravity will be applied to the object.</param>
    public static void RotateToTrajectory(Transform target, Vector3 initialPos, Vector3 finalPos, float speed, float gravity = 9.81f)
    {
        float dist = Vector3.Distance(initialPos, finalPos);
        float distance = dist;

        //Here we assign the rotation
        Vector3 relativePos = finalPos - target.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos);
        target.rotation = rotation;
        var tempRot = target.eulerAngles;

        //This line of code is so that we can point torwards the target position, while also pointing to the firing angle
        tempRot.x = target.eulerAngles.x - GetTrajectoryAngle(distance, speed, gravity);

        //Case: the target distance is too far and the speed is too low - set to 45 deg
        tempRot.x = float.IsNaN(tempRot.x) ? -45 : tempRot.x;

        target.eulerAngles = tempRot;
    }

    /// <summary>
    /// Rotates the target game object to the optimal angle for the object to be thrown at to land on the given position.
    /// </summary>
    /// <param name="target">The game object to be oriented to the trajectory angle.</param>
    /// <param name="initialPos">Where the object will be initially thrown.</param>
    /// <param name="finalPos">Where the object should land.</param>
    /// <param name="speed">How fast the object travels.</param>
    /// <param name="gravity">How much gravity will be applied to the object.</param>
    public static void RotateToTrajectory(GameObject target, Vector3 initialPos, Vector3 finalPos, float speed, float gravity = 9.81f)
    {
        RotateToTrajectory(target.transform, initialPos, finalPos, speed, gravity);
    }
}
