using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectVisibilityManager : MonoBehaviour
{
    //[Tooltip("This is not used in the code and soley used for putting in notes.")]
    //[SerializeField] [TextArea] private string editorNotes = "Put any notes here...";

    [SerializeField] private GameObject[] targets;

    [Header("Events")]
    public UnityEvent onTargetsDisable;
    public UnityEvent onTargetsEnable;
    public UnityEvent onGameObjectDisable;


    public void TargetsSetActive(bool state)
    {
        //Exit immediately when the targets array is empty.
        if (targets.Length <= 0)
        {
            Debug.LogWarning(string.Format("{0} [{1}] - Gameobject array \"targets\" is empty! " +
                                           "Unity events associated with this method call was not invoked...",
                                           this, this.gameObject.GetInstanceID()));
            return;
        }

        if (state)
            onTargetsEnable.Invoke();
        else
            onTargetsDisable.Invoke();

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].SetActive(state);
        }
    }

    private void OnDisable()
    {
        //Prevents execution when the editor is paused
        #if UNITY_EDITOR
            if (Application.isEditor &&
                Application.isPlaying)
            {
                onGameObjectDisable.Invoke();
            }
        #else
            onGameObjectDisable.Invoke();
        #endif
    }
}
