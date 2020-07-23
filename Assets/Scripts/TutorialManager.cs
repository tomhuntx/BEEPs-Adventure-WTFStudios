using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	// bools to track progress
	private bool grabbedBox = false;
	private bool placedBox = false;
	private bool thrownBox = false;
	private bool punchedBox = false;

	// tooltips - separate objects for icons
	public GameObject grabBoxTooltip;
	public GameObject placeBoxTooltip;
	public GameObject throwBoxTooltip;
	public GameObject punchTooltip;
	public GameObject currentTooltip;

	// Start is called before the first frame update
	void Start()
	{
		currentTooltip = grabBoxTooltip;
	}

    // Update is called once per frame
    void Update()
    {
		if (grabbedBox && placedBox && thrownBox && punchedBox)
		{
			// ready to leave level

		}
    }

	private void ShowTooltip()
	{
		currentTooltip.SetActive(true);
	}

	private void HideTooltip()
	{
		currentTooltip.SetActive(false);
	}

	private void NewTooltip(GameObject tooltip, bool show)
	{
		HideTooltip();
		currentTooltip = tooltip;

		if (show)
		{
			ShowTooltip();
		}
	}

	// Register if a box is hovered over
	public void Hover()
	{
		if (!grabbedBox)
		{
			ShowTooltip();
		}

		if (placedBox && thrownBox && !punchedBox)
		{
			NewTooltip(punchTooltip, true);
		}
	}

	// Register if a box is hovered over
	public void NotHover()
	{
		if (!grabbedBox)
		{
			HideTooltip();
		}

		if (placedBox && thrownBox && !punchedBox)
		{
			HideTooltip();
		}
	}

	// Register if a box is picked up
	public void Grab()
	{
		if (!grabbedBox)
		{
			grabbedBox = true;

			// place box next
			NewTooltip(placeBoxTooltip, true);
		}
		else if (!thrownBox)
		{
			NewTooltip(throwBoxTooltip, true);
		}
	}

	// Register if a box is placed
	public void Place()
	{
		if (!placedBox)
		{
			placedBox = true;

			if (!thrownBox)
			{
				NewTooltip(throwBoxTooltip, false);
			}
			else
			{
				NewTooltip(punchTooltip, false);
			}
		}
		HideTooltip();
	}

	// Register if a box is thrown
	public void Throw()
	{
		if (!thrownBox && placedBox)
		{
			thrownBox = true;

			if (!placedBox)
			{
				NewTooltip(placeBoxTooltip, false);
			}
			else
			{
				NewTooltip(punchTooltip, false);
			}
		}
		HideTooltip();
	}

	// Register if a box is punched
	public void Punch()
	{
		if (!punchedBox && thrownBox && placedBox)
		{
			punchedBox = true;

			HideTooltip();
		}
	}
}
