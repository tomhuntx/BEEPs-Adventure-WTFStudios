using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodySleepingManager : MonoBehaviour
{
    public Rigidbody rb;
    public bool disableSleeping = false;

    private void Awake()
    {
        TryGetRigidbodyComponent();
        if (rb == null)
            Debug.LogError(string.Format("[{0} || {1}] RigidbodySleepingManager component didn't detect any rigidbodies. " +
                                         "Please attach one before playing!", 
                                          this.gameObject.GetInstanceID(), 
                                          this.gameObject.name));
    }

    private void Start()
    {
        if (!disableSleeping) rb.Sleep();
    }

    private void FixedUpdate()
    {
        if (disableSleeping) rb.WakeUp();
    }

    public void DoDisableSleeping(bool state)
    {
        disableSleeping = state;
        if (!disableSleeping) rb.Sleep();
    }

    public void TryGetRigidbodyComponent()
    {
        rb = this.GetComponentInChildren<Rigidbody>();
    }
}

/*
[CustomEditor(typeof(RigidbodySleepingManager))]
public class RigidbodySleepingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RigidbodySleepingManager component = target as RigidbodySleepingManager;
        if (!component.disableSleeping &&
            GUILayout.Button("Disable Rigidbody Sleeping"))
        {
            component.TryGetRigidbodyComponent();
            component.DoDisableSleeping(true);
        }
        else if (component.disableSleeping &&
                 GUILayout.Button("Enable Rigidbody Sleeping"))
        {
            component.TryGetRigidbodyComponent();
            component.DoDisableSleeping(false);
        }
    }
}
*/