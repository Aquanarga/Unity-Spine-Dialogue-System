using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TypeWriter))]
public class DialogueUi : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject historyBox;
    [SerializeField] private TMP_Text historyText;
    [SerializeField] private ResponseHandler responseHandler;
    [SerializeField] private SpineHandler spineHandler;
    [SerializeField] private RuntimeDialogueGraph initialDialogue;

    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private TypeWriter _typeWriter;
    
    private void Awake()
    {
        dialogueText.text = string.Empty;
        historyText.text = string.Empty;
        dialogueBox.SetActive(false);
        historyBox.SetActive(false);
        _typeWriter = GetComponent<TypeWriter>();
        CloseDialogueBox();
    }

    private void Start()
    {
        ShowDialogue(initialDialogue);
    }

    public void ShowDialogue(RuntimeDialogueGraph dialogueGraph)
    {
        foreach (var node in dialogueGraph.AllNodes)
        {
            _nodeLookup[node.NodeId] = node;
        }

        ContinueDialogue(dialogueGraph.EntryNodeId);
    }

    public void ContinueDialogue(string nextNodeID)
    {
        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(nextNodeID));
    }

    private IEnumerator StepThroughDialogue(string entryNodeID)
    {
        if (string.IsNullOrEmpty(entryNodeID) || !_nodeLookup.ContainsKey(entryNodeID))
        {
            CloseDialogueBox();
            yield break;
        }
        RuntimeDialogueNode currentNode = _nodeLookup[entryNodeID];

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            spineHandler.ProcessDialogueNode(currentNode);
            // Display text
            //yield return _typeWriter.Run(currentNode.DialogueText, dialogueText);
            yield return RunTypingEffect(currentNode.DialogueText);

            dialogueText.text = currentNode.DialogueText;

            yield return null; // Prevents the next yield return from reading the same input as RunTypingEffect
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            historyBox.SetActive(true);
            historyText.text = currentNode.DialogueText;

            // Is next ID/node valid?
            string nextNodeID = currentNode.NextNodeId;
            if (string.IsNullOrEmpty(nextNodeID) || !_nodeLookup.ContainsKey(nextNodeID)) break;

            currentNode = _nodeLookup[nextNodeID];

            // Is it a choiceNode?
            if (currentNode.Choices.Count > 0)
            {
                dialogueText.text = string.Empty;
                spineHandler.ProcessDialogueNode(currentNode);
                responseHandler.HandleResponse(currentNode.Choices);
                break;
            }
        }
        
        // If last node wasn't a choiceNode, end Dialogue
        if (currentNode.Choices.Count == 0)
            CloseDialogueBox();
    }

    private IEnumerator RunTypingEffect(string dialogue)
    {
        _typeWriter.Run(dialogue, dialogueText);

        while (_typeWriter.IsRunning)
        {
            yield return null;

            if (Input.GetKeyDown(KeyCode.Space))
                _typeWriter.Stop();
        }
    }
    
    private void CloseDialogueBox()
    {
        spineHandler.OnCloseDialogueBox();
        dialogueBox.SetActive(false);
        dialogueText.text = string.Empty;
        historyBox.SetActive(false);
        historyText.text = string.Empty;

        _nodeLookup.Clear();
    }
}
