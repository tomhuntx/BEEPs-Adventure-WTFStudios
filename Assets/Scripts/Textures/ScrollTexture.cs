using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
	public float scrollX = 0.5f;
	public float scrollY = 0.5f;

	// Make multiple only if using multiple materials
	public int materialIndex;

	Vector2 startpos;
	Material mat;

	private void Start()
	{
		mat = GetComponent<Renderer>().materials[materialIndex];
		startpos = mat.mainTextureOffset;
	}

	// Update is called once per frame
	void Update()
    {
		float offsetX = Time.time * scrollX;
		float offsetY = Time.time * scrollY;

		// Lerping from previous position to new position
		mat.mainTextureOffset = Vector2.Lerp(mat.mainTextureOffset, 
			new Vector2(offsetX, offsetY), Mathf.PingPong(Time.time, 1));
	}
}
