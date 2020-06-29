using UnityEngine;
using System;

public static class SearchForParent
{
    /// <summary>
    /// Tries to get the main parent of the given game object.
    /// </summary>
    /// <param name="childedGameObject">The childed game object.</param>
    /// <param name="targetComponent">The type of component of the desired parent transform has.</param>
    /// <returns>The main parent transform. 
    /// If it didn't exist, returns the given game object's transform.</returns>
    public static Transform GetParentTransform(GameObject childedGameObject, string targetComponent = null)
    {
        Transform childGO = childedGameObject.transform;

        //The given game object has a parent
        //Search upwards in the heirarchy for another parent transform
        if (childGO.parent != null)
        {
            //The the parent of the given game object
            Transform parent = childGO.parent;

            //The parent of the given game object's parent exist
            //Continue searching up
            while (parent.parent != null)
            {
                //Get the parent's parent transform
                parent = parent.parent;

                //Return the current parent transform if the target component is found
                if (targetComponent != null &&
                    parent.TryGetComponent(Type.GetType(targetComponent), out Component currentComponent))
                {
                    return parent;
                }
            }
            return parent;
        }
        //The given game object has no parent
        //Return the game object's transform component
        else
        {
            return childGO;
        }
    }

    /// <summary>
    /// Checks if the given childed game object has a main parent.
    /// </summary>
    /// <param name="childedGameObject">The childed game object.</param>
    /// <param name="targetComponent">The type of component of the desired parent transform has.</param>
    /// <returns>True if the main parent transform isn't the same as the given game object's transform.</returns>
    public static bool HasParent(GameObject childedGameObject, string targetComponent = null)
    {
        Transform parentTransform = GetParentTransform(childedGameObject, targetComponent);
        return parentTransform != childedGameObject.transform;
    }
}