﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmLights : MonoBehaviour
{
	[Tooltip("Whether the lights flash (true) or do not flash (false).")]
	[SerializeField] private bool alarmsFlashing = false;
	private GameObject lights;

    void Start()
    {
		// Get light animator object even if it is inactive (default)
		lights = this.GetComponentInChildren<Animator>(true).gameObject;

		// Set light active/inactive based on set boolean
		lights.SetActive(alarmsFlashing);
    }

	/// <summary>
	/// Activate or deactivate alarm lights
	/// </summary>
	/// <param name="state">Activate (true) or deactivate (false)</param>
	public void AlarmSwitch(bool state)
	{
		lights.SetActive(state);
		alarmsFlashing = state;
	}
}
