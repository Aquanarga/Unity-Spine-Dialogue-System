using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
public class CharacterData : ScriptableObject
{
    public readonly string uuid = Guid.NewGuid().ToString();
    [SerializeField] public string characterName;
    [SerializeField] public string description;
}
