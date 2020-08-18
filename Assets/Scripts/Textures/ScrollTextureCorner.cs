using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTextureCorner : MonoBehaviour
{
	public float scrollX = 0.5f;
	public float scrollY = 0.5f;

	// Make multiple only if using multiple materials
	public int materialIndex;

	Vector2 startpos;
	Material mat;

	public float resetAfter;
	private float offsetX = 0;
	private float offsetY = 0;


	float scrollSpeed = 0.5f;
	Renderer rend;

	private void Start()
	{
		rend = GetComponent<Renderer>();
	}

	// Update is called once per frame
	void Update()
    {
		float offset = Time.time * scrollSpeed;
		rend.materials[materialIndex].SetTextureOffset("_MainTex", new Vector2(offset, 0));
	}
}
