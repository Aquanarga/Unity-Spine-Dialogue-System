using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using Spine.Unity;

public class RuntimeDialogueGraph : ScriptableObject
{
    public string EntryNodeId;
    public List<RuntimeDialogueNode> AllNodes = new List<RuntimeDialogueNode>();
}

public enum DialogueSide { Left, Right }

[Serializable]
public class RuntimeDialogueNode
{
    public string NodeId;

    public SpineData LeftChar;
    public SpineData RightChar;
    public DialogueSide SpeakerLocation;

    public string DialogueText;
    public List<ChoiceData> Choices = new List<ChoiceData>();
    public string NextNodeId;
}

[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNode;
}

[Serializable]
public class SpineData
{
    public CharacterData Char;
    public AnimationReferenceAsset Animation;
    public double Duration;
}
