using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(UnityEventsHandler))]
public class ButtonPressPrompt : MonoBehaviour
{
    public const string RESOURCE_PREFAB = "Button Press Prompt/PREFAB_PRESET_Prompt";
    private Transform prefabInstance;
    private TextMeshProUGUI messageText;
    private TextMeshProUGUI buttonText;

    [Header("Identification")]
    [SerializeField] private string promptMessage = "[INPUT MESSAGE HERE]";

    [Header("Properties")]
    [Tooltip("Tick this if you want the button prompt game object to be visible upon start.")]
    [SerializeField] private bool showOnAwake = false;
    [Tooltip("Tick this if you want to automatically setup to show prompts upon trigger enter.")]
    public bool autoShowPrompt = true;
    [SerializeField] private KeyCode assignedButton = KeyCode.E;
    public UnityEvent onButtonPress;

    private UnityEventsHandler colliderEvents;


    // Start is called before the first frame update
    void Start()
    {
        GameObject instance = Instantiate(Resources.Load(RESOURCE_PREFAB) as GameObject);
        instance.name = string.Format("Button Prompt - {0}", assignedButton.ToString());
        prefabInstance = instance.transform;
        prefabInstance.SetParent(this.transform);
        TextMeshProUGUI[] texts = prefabInstance.GetComponentsInChildren<TextMeshProUGUI>();
        messageText = texts[0];
        buttonText = texts[1];
        prefabInstance.gameObject.SetActive(showOnAwake);

		Debug.Assert(this.GetComponent<Collider>() != null, 
                     string.Format("There is no collider attached on {0}... Please attach one before playing!", this.gameObject));

        if (autoShowPrompt)
        {
            colliderEvents = this.GetComponent<UnityEventsHandler>();
            colliderEvents.onTriggerStay.AddListener(ShowPrompt);
            colliderEvents.onTriggerExit.AddListener(HidePrompt);
            onButtonPress.AddListener(HidePrompt);
        }

        messageText.text = promptMessage;
        buttonText.text = assignedButton.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (prefabInstance.gameObject.activeSelf &&
            Input.GetKeyDown(assignedButton))
        {
            onButtonPress.Invoke();
        }
    }



    public void TogglePromptVisibility(bool state)
    {
        prefabInstance.gameObject.SetActive(state);
    }


    #region Auto Added Unity Events Listener Methods
    private void ShowPrompt()
    {
        TogglePromptVisibility(true);
    }

    private void HidePrompt()
    {
        TogglePromptVisibility(false);
    }
    #endregion
}
