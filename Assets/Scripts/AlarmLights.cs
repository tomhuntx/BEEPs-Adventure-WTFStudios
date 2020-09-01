using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmLights : MonoBehaviour
{
	[Tooltip("Whether the lights flash (true) or do not flash (false).")]
	[SerializeField] private bool alarmsFlashing = false;
	private GameObject lights;
	private Renderer[] renderers;

	void Start()
    {
		// Get light animator object even if it is inactive (default)
		lights = this.GetComponentInChildren<Animator>(true).gameObject;

		// Set light active/inactive based on set boolean
		lights.SetActive(alarmsFlashing);

		renderers = GetComponentsInChildren<Renderer>();
	}

	/// <summary>
	/// Activate or deactivate alarm lights
	/// </summary>
	/// <param name="state">Activate (true) or deactivate (false)</param>
	public void AlarmSwitch(bool state)
	{
		lights.SetActive(state);
		alarmsFlashing = state;

		foreach (Renderer rend in renderers)
		{
			rend.material.SetColor("_EmissionColor", Color.red);
			rend.UpdateGIMaterials();
		}
	}

	/// <summary>
	/// Activate ALL lights in the scene - USE CAREFULLY AND ONLY ONCE!
	/// </summary>
	/// <param name="state">Activate (true) or deactivate (false)</param>
	public void AllAlarmSwitch(bool state)
	{
		AlarmLights[] al = GameObject.FindObjectsOfType<AlarmLights>();
		foreach (AlarmLights light in al)
		{
			light.AlarmSwitch(state);
		}
	}
}
