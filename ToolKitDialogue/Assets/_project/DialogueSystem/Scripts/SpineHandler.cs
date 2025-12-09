using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpineHandler : MonoBehaviour
{
    [Serializable] private class charSpineLink
    {
        public CharacterData Char;
        public SkeletonGraphic Left;
        public SkeletonGraphic Right;
        public AnimationReferenceAsset DefaultAnimation;
    }
    [SerializeField] private List<charSpineLink> charSpineLinker = new List<charSpineLink>();
    private charSpineLink activeLeft;
    private charSpineLink activeRight;
    

    void Start()
    {
        foreach (var link in charSpineLinker)
        {
            link.Left.gameObject.SetActive(false);
            link.Right.gameObject.SetActive(false);

            if (link.DefaultAnimation)
            {
                link.Left.AnimationState.SetAnimation(0, link.DefaultAnimation, true);
                link.Right.AnimationState.SetAnimation(0, link.DefaultAnimation, true);
            }
        }
    }

    void Update()
    {
        
    }

    public void OnCloseDialogueBox()
    {
        foreach (var link in charSpineLinker)
        {
            link.Left.gameObject.SetActive(false);
            link.Right.gameObject.SetActive(false);

            if (link.DefaultAnimation)
            {
                link.Left.AnimationState.SetAnimation(0, link.DefaultAnimation, true);
                link.Right.AnimationState.SetAnimation(0, link.DefaultAnimation, true);
            }
        }

        activeLeft = null;
        activeRight = null;
    }

    public void ProcessDialogueNode(RuntimeDialogueNode node)
    {
        // Update the active characters
        activeLeft?.Left.gameObject.SetActive(false);
        activeRight?.Right.gameObject.SetActive(false);

        foreach (var link in charSpineLinker)
        {
            if (node.LeftChar.Char == link.Char)
                activeLeft = link;
            if (node.RightChar.Char == link.Char)
                activeRight = link;
        }

        activeLeft.Left.gameObject.SetActive(true);
        activeRight.Right.gameObject.SetActive(true);


        // Spine animations are paused, not reset, while the gameobject is inactive
        // Left Animation
        if (node.LeftChar.Animation != null)
            activeLeft.Left.AnimationState.SetAnimation(0, node.LeftChar.Animation, true);
        else if (activeLeft.DefaultAnimation != null)
            activeLeft.Left.AnimationState.SetAnimation(0, activeLeft.DefaultAnimation, true);

        // Right Animation
        if (node.RightChar.Animation != null)
            activeRight.Right.AnimationState.SetAnimation(0, node.RightChar.Animation, true);
        else if (activeRight.DefaultAnimation != null)
            activeRight.Right.AnimationState.SetAnimation(0, activeRight.DefaultAnimation, true);
    }

}
