using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
        var nodeIDMap = new Dictionary<INode, string>();

        foreach (var node in editorGraph.GetNodes())
        {
            nodeIDMap[node] = Guid.NewGuid().ToString();
        }

        var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
        if (startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if (entryPort != null) {
                runtimeGraph.EntryNodeId = nodeIDMap[entryPort.GetNode()];
            }
        }

        foreach (var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue;


            var runtimeNode = new RuntimeDialogueNode { NodeId = nodeIDMap[iNode] };
            switch(iNode)
            {
                case DialogueNode dialogueNode:
                    ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
                    break;
                case ChoiceNode choiceNode:
                    ProcessChoiceNode(choiceNode, runtimeNode, nodeIDMap);
                    break;
            }

            runtimeGraph.AllNodes.Add(runtimeNode);
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        SpineData left = new SpineData();
        left.Char = GetPortValue<CharacterData>(node.GetInputPortByName("LeftChar"));
        left.Animation = GetPortValue<AnimationReferenceAsset>(node.GetInputPortByName("LeftAnimation"));
        left.Duration = GetPortValue<float>(node.GetInputPortByName("LeftDuration"));

        SpineData right = new SpineData();
        right.Char = GetPortValue<CharacterData>(node.GetInputPortByName("RightChar"));
        right.Animation = GetPortValue<AnimationReferenceAsset>(node.GetInputPortByName("RightAnimation"));
        right.Duration = GetPortValue<float>(node.GetInputPortByName("RightDuration"));

        runtimeNode.LeftChar = left;
        runtimeNode.RightChar = right;
        runtimeNode.SpeakerLocation = GetPortValue<DialogueSide>(node.GetInputPortByName("SpeakerLocation"));

        runtimeNode.DialogueText = GetPortValue<MultiLineString>(node.GetInputPortByName("Dialogue")).str;

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
            runtimeNode.NextNodeId = nodeIDMap[nextNodePort.GetNode()];
    }

    private void ProcessChoiceNode(ChoiceNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        SpineData left = new SpineData();
        left.Char = GetPortValue<CharacterData>(node.GetInputPortByName("LeftChar"));
        left.Animation = GetPortValue<AnimationReferenceAsset>(node.GetInputPortByName("LeftAnimation"));
        left.Duration = GetPortValue<float>(node.GetInputPortByName("LeftDuration"));

        SpineData right = new SpineData();
        right.Char = GetPortValue<CharacterData>(node.GetInputPortByName("RightChar"));
        right.Animation = GetPortValue<AnimationReferenceAsset>(node.GetInputPortByName("RightAnimation"));
        right.Duration = GetPortValue<float>(node.GetInputPortByName("RightDuration"));

        runtimeNode.LeftChar = left;
        runtimeNode.RightChar = right;
        runtimeNode.SpeakerLocation = GetPortValue<DialogueSide>(node.GetInputPortByName("SpeakerLocation"));

        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));

        foreach (var choicePort in choiceOutputPorts)
        {
            var index = choicePort.name.Substring("Choice ".Length);
            var textPort = node.GetInputPortByName($"Choice Text {index}");

            var choiceData = new ChoiceData
            {
                ChoiceText = GetPortValue<string>(textPort),
                DestinationNode = choicePort.firstConnectedPort != null ? nodeIDMap[choicePort.firstConnectedPort.GetNode()] : null
            };

            runtimeNode.Choices.Add(choiceData);
        }
    }

    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if (port.isConnected)
        {
            if (port.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.variable.TryGetDefaultValue(out T value);
                return value;
            }
        }

        port.TryGetValue(out T fallbackValue);
        return fallbackValue;
    }
}
