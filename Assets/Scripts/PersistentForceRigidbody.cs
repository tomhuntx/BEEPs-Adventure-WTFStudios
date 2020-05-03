﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentForceRigidbody : MonoBehaviour
{
    private enum Direction { Forward, Right, Up, Omnidirectional }

    [Tooltip("Which direction of this transform would the force be applied. NOTE: Omnidirectional applies force from this transform's centner")]
    [SerializeField] private Direction forceDirection = Direction.Forward;

    [Tooltip("How much force is applied.")]
    [SerializeField] private float force = 20.0f;

    [Tooltip("Type of force applied to rigidbodies.")]
    [SerializeField] private ForceMode forceType = ForceMode.Force;

    [Tooltip("Instantly destroys after applying force.")]
    [SerializeField] private bool isOneShot = false;

    private Collider trigger;
    private List<Rigidbody> rbs = new List<Rigidbody>();

    
    
    // Start is called before the first frame update
    void Start()
    {
        if (this.GetComponent<Collider>() != null) trigger = this.GetComponent<Collider>();
        if (trigger != null && !trigger.isTrigger) trigger.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (rbs.Count > 0)
        {
            ApplyForce();
            if (isOneShot) Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<Rigidbody>())
        {
            Rigidbody reference = other.transform.GetComponent<Rigidbody>();
            if (!rbs.Contains(reference)) rbs.Add(reference);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.GetComponent<Rigidbody>())
        {
            Rigidbody reference = other.transform.GetComponent<Rigidbody>();
            if (rbs.Contains(reference)) rbs.Remove(reference);
        }
    }


    /// <summary>
    /// Applies appropriate force to rigidbodies in the trigger.
    /// </summary>
    private void ApplyForce()
    {
        switch (forceDirection)
        {
            case Direction.Omnidirectional:
                foreach (Rigidbody rb in rbs)
                {
                    if (rb == null) rbs.Remove(rb);
                    rb.AddExplosionForce(force, this.transform.position, Vector3.Magnitude(trigger.bounds.size) / 2, default, forceType);
                }
                break;

            default:
                foreach (Rigidbody rb in rbs)
                {
                    if (rb == null) rbs.Remove(rb);
                    rb.AddForce(GetDirection(forceDirection) * force, forceType);
                }
                break;
        }
    }

    /// <summary>
    /// Translates from Direction enum into Vector 3
    /// </summary>
    private Vector3 GetDirection(Direction direction)
    {
        switch(direction)
        {
            case Direction.Forward:
                return this.transform.forward;
            case Direction.Right:
                return this.transform.right;
            case Direction.Up:
                return this.transform.up;
            default:
                return Vector3.zero;
        }
    }
}