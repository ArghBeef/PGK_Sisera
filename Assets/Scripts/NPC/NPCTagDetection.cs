using System;
using UnityEngine;

[Serializable]
public class NPCTagDetection
{
    public string targetTag;
    [Min(0f)] public float detectionTime = 1f;
    public NPCReactionType reactionType = NPCReactionType.Neutral;
}