using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ResponseHandler : MonoBehaviour
{
    [SerializeField] private DialogueUi dialogueUi;
    [SerializeField] private RectTransform responseContainer;
    [SerializeField] private GameObject responseTemplate;

    private List<GameObject> _tempButtons = new List<GameObject>();

    public void HandleResponse(List<ChoiceData> responses)
    {
        foreach (ChoiceData response in responses)
        {
            GameObject responseButton = Instantiate(responseTemplate, responseContainer);
            responseButton.SetActive(true);
            responseButton.GetComponent<TMP_Text>().text = response.ChoiceText;
            responseButton.GetComponent<Button>().onClick.AddListener(() => OnPickedResponse(response));
            
            _tempButtons.Add(responseButton);
        }
        
        gameObject.SetActive(true);
    }

    private void OnPickedResponse(ChoiceData response)
    {
        dialogueUi.ContinueDialogue(response.DestinationNode);
        foreach (GameObject button in _tempButtons)
        {
            Destroy(button);
        }
        _tempButtons.Clear();
        
        gameObject.SetActive(false);
    }
}
