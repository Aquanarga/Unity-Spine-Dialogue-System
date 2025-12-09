using Codice.Client.Common;
using Spine.Unity;
using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class MultiLineString
{
    [TextArea(8, 20)] public string str;
}

// As far as I can tell CustomPropertyDrawer, has some hardcoded stuff, which does not play nice with Graph Tool Kit (layout wise)
// But at least I managed to make something that doesn't look completly awefull
[CustomPropertyDrawer(typeof(MultiLineString))]
public class MultiLineStringDrawer : PropertyDrawer
{
    private float nodeHeight = 100f;
    private float inspectorHeight = 200f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // For the node the label is none, but for the graph inspector it isn't, and the graph inspector needs an off-set
        if (label != GUIContent.none)
        {
            // Draw Label
            position.x -= 3;
            position.width += 3;
            EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        }

        // For potential future debuging
        // EditorGUI.DrawRect(position, Color.blue);
        EditorGUI.PropertyField(position, property.FindPropertyRelative("str"), GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (label != GUIContent.none)
            return inspectorHeight;
        return nodeHeight;
    }
}

[Serializable]
public class StartNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddOutputPort("out").Build();
    }
}

[Serializable]
public class EndNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
    }
}

[Serializable]
public class DialogueNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();

        context.AddInputPort<CharacterData>("LeftChar").Build();
        context.AddInputPort<AnimationReferenceAsset>("LeftAnimation").WithDisplayName("L Animation").Build();
        context.AddInputPort<float>("LeftDuration").WithDisplayName("L Duration").Build();
        context.AddInputPort<CharacterData>("RightChar").Build();
        context.AddInputPort<AnimationReferenceAsset>("RightAnimation").WithDisplayName("R Animation").Build();
        context.AddInputPort<float>("RightDuration").WithDisplayName("R Duration").Build();
        context.AddInputPort<DialogueSide>("SpeakerLocation").WithDisplayName("Speaker Side").Build();

        context.AddInputPort<MultiLineString>("Dialogue").Build();
    }
}

[Serializable]
public class ChoiceNode : Node
{
    const string optionID = "portCount";

    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();

        context.AddInputPort<CharacterData>("LeftChar").Build();
        context.AddInputPort<AnimationReferenceAsset>("LeftAnimation").WithDisplayName("L Animation").Build();
        context.AddInputPort<float>("LeftDuration").WithDisplayName("L Duration").Build();
        context.AddInputPort<CharacterData>("RightChar").Build();
        context.AddInputPort<AnimationReferenceAsset>("RightAnimation").WithDisplayName("R Animation").Build();
        context.AddInputPort<float>("RightDuration").WithDisplayName("R Duration").Build();
        context.AddInputPort<DialogueSide>("SpeakerLocation").WithDisplayName("Speaker Side").Build();

        var option = GetNodeOptionByName(optionID);
        option.TryGetValue(out int portCount);
        for (int i = 0; i < portCount; i++)
        {
            context.AddInputPort<string>($"Choice Text {i}").Build();
            context.AddOutputPort($"Choice {i}").Build();
        }
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(optionID).WithDisplayName("Choice Count").WithDefaultValue(2).Delayed();
    }
}