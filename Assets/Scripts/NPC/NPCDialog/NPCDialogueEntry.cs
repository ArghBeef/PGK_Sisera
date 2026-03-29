using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NPCDialogueEntry
{
    [Tooltip("Unique NPC id of the other NPC")]
    public string targetNpcId;

    [Tooltip("If true, this conversation can only happen once")]
    public bool oneTimeOnly = true;

    public NPCDialoguePriority priority = NPCDialoguePriority.Low;

    [Tooltip("Lines this NPC says to that specific target NPC")]
    public List<NPCDialogueLine> lines = new List<NPCDialogueLine>();
}