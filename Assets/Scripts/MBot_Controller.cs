using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MBot_Controller : MonoBehaviour
{
	private Vector3 startPosition;
	private Quaternion startRotation;
	private NavMeshAgent agent;

	// This Manager's Hat
	public GameObject hardhat;
	public GameObject hatdhatStartLoc;

	// Events
	public UnityEvent onDisturb;

	// The Bot's Animator (all 3 are required unfortunately)
	public Animator anim;
	public Animator animFace;
	public Animator animScreen;

	// Player
	public Player player;

	// Place to stand by at the end of the game
	public GameObject angryLocation;

	private bool lookingForHat = false;
	private bool nearHat = true;
	private bool lookAtPlayerForever = false;
	private bool explosionLook;

	[Header("Face Changing")]
	public Renderer faceRender;
	public int faceMatIndex = 0;
	public Texture normal;
	public Texture disturbed;
	public Texture angry;
	public bool changeColour = false;
	public Color normalCol;
	public Color angryCol;

	// Start is called before the first frame update
	void Start()
    {
		startPosition = transform.position;
		startRotation = transform.rotation;
		agent = GetComponent<NavMeshAgent>();
		agent.SetDestination(startPosition);
		faceRender.materials[faceMatIndex].EnableKeyword("_NORMALMAP");
	}

    // Update is called once per frame
    void Update()
    {
		if (explosionLook)
		{
			GameObject player =	Player.Instance.gameObject;
			if (player != null)
			{
				RotateTo(player.transform.position);
			}
		}
		else if (Vector3.Distance(transform.position, hardhat.transform.position) < 2.0f && !lookAtPlayerForever)
		{
			nearHat = true;

			if (lookingForHat)
			{
				TryGrabHat();
			}
		}
		else if (!lookAtPlayerForever)
		{
			nearHat = false;
			agent.SetDestination(hardhat.transform.position);
			RotateTo(hardhat.transform.position);
		}

		// Detect if at start position or not
		if (!lookingForHat && Vector3.Distance(transform.position, startPosition) < 1.5f)
		{
			if (!anim.GetBool("isHome"))
			{ // Nested if ensures bools are only set once (performance reasons)
				anim.SetBool("isHome", true);
				animFace.SetBool("isHome", true);
				animScreen.SetBool("isHome", true);

				GameObject player = GameObject.FindGameObjectWithTag("Player");
				if (player != null && Vector3.Distance(transform.position, player.transform.position) < 2f)
				{
					float pow = 15f / Vector3.Distance(transform.position, player.transform.position);

					Player.Instance.PlayerMovementControls.ApplyForce((
						Player.Instance.transform.position - this.transform.position).normalized * pow,
						PlayerCharacterController.ConvertFromForceMode(ForceMode.Impulse));
				}

				if (!lookAtPlayerForever)
				{
					// Return anims
					faceRender.materials[faceMatIndex].SetTexture("_MainTex", normal);
					if (changeColour)
						faceRender.materials[faceMatIndex].SetColor("_Color", normalCol);
				}


			}
			else if (!lookingForHat && !lookAtPlayerForever && nearHat)
			{
				RotateTo(startRotation);
			}
		}
		else 
		{
			if (anim.GetBool("isHome"))
			{
				anim.SetBool("isHome", false);
				animFace.SetBool("isHome", false);
				animScreen.SetBool("isHome", false);
			}
		}

		if (lookAtPlayerForever && player != null)
		{
			if (Vector3.Distance(this.transform.position, angryLocation.transform.position) < 1f)
			{
				agent.isStopped = true;
				if (Vector3.Distance(this.transform.position, player.transform.position) < 10.0f)
				{
					RotateTo(player.transform.position);
				}
			}
			else
			{
				agent.SetDestination(angryLocation.transform.position);
			}
		}
    }

	public void HatMoved()
	{
		// Start angry pause
		StartCoroutine(AngryPause());

		// Angry animation
		anim.SetTrigger("hatGrabbed");
		animFace.SetTrigger("hatGrabbed");
		animScreen.SetTrigger("hatGrabbed");
	}

	public void GetBlownUp()
	{
		// Start angry pause
		StartCoroutine(AngryPause());

		// Angry animation
		anim.SetTrigger("hatGrabbed");
		animFace.SetTrigger("hatGrabbed");
		animScreen.SetTrigger("hatGrabbed");

		// Look at player
		explosionLook = true;
	}

	private void RotateTo(Vector3 target)
	{
		//GameObject child = this.transform.GetChild(0).gameObject;
		Vector3 direction = (target - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		this.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
	}

	private void RotateTo(Quaternion target)
	{
		//GameObject child = this.transform.GetChild(0).gameObject;
		this.transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 1f);
	}

	private void TryGrabHat()
	{
		if (nearHat)
		{
			// Start interact animation
			anim.SetBool("isPickingUp", true);
			animFace.SetBool("isPickingUp", true);
			animScreen.SetBool("isPickingUp", true);

			// Stop agent before waiting
			agent.isStopped = true;

			StartCoroutine(GrabPause(2.0f));
		}
	}

	// Bot successfully grabbed hat
	private void GrabHat()
	{
		// Return to base with hat
		if (!lookAtPlayerForever)
		{
			agent.SetDestination(startPosition);
		}
		lookingForHat = false;

		// Remove hat from hands
		if (true) // INSERT PREVENTION OF REMOVING NON-HARDHAT OBJECTS
		{
			player.RemoveGrabbedObject();
			hardhat.GetComponent<GrabbableObject>().DetachFromParent();
		}

		// Put hat back on head
		hardhat.transform.SetParent(hatdhatStartLoc.transform);
		hardhat.transform.position = hatdhatStartLoc.transform.position;
		hardhat.transform.rotation = hatdhatStartLoc.transform.rotation;
		Rigidbody rb = hardhat.GetComponent<Rigidbody>();
		rb.isKinematic = true;
	}

	private IEnumerator GrabPause(float seconds)
	{
		yield return new WaitForSeconds(seconds);

		agent.isStopped = false;

		// Start interact animation
		anim.SetBool("isPickingUp", false);
		animFace.SetBool("isPickingUp", false);
		animScreen.SetBool("isPickingUp", false);

		// If found hat, grab it
		if (nearHat)
		{
			GrabHat();
		}
	}


	// Pause and then start looking for hat - prevents instant attempts at grabbing
	private IEnumerator AngryPause()
	{
		// Face animations
		faceRender.materials[faceMatIndex].SetTexture("_MainTex", angry);
		if (changeColour)
			faceRender.materials[faceMatIndex].SetColor("_Color", angryCol);

		agent.isStopped = true;

		yield return new WaitForSeconds(2f);

		agent.isStopped = false;
		lookingForHat = true;

		if (!lookAtPlayerForever)
		{
			agent.SetDestination(hardhat.transform.position);

			// Face colour stop 
			if (changeColour)
				faceRender.materials[faceMatIndex].SetColor("_Color", normalCol);
		}

		explosionLook = false;
	}

	// Become angry forever (when chute is destroyed)
	public void AngryForever()
	{
		anim.SetBool("angryForever", true);
		animFace.SetBool("angryForever", true);
		animScreen.SetBool("angryForever", true);

		lookAtPlayerForever = true;

		// Stop agent
		agent.SetDestination(angryLocation.transform.position);

		// Face animations
		faceRender.materials[faceMatIndex].SetTexture("_MainTex", angry);
		if (changeColour)
			faceRender.materials[faceMatIndex].SetColor("_Color", angryCol);
	}
}
