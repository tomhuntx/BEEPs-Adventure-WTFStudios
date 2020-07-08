using UnityEngine;

public static class CopyOver
{
    /// <summary>
    /// Copy over a component to a given game object.
    /// </summary>
    /// <typeparam name="T">The type of component.</typeparam>
    /// <param name="original">The component to be copied over.</param>
    /// <param name="destination">The game object where the copied component will go to.</param>
    /// <returns>The copied component.</returns>
    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }
}
