using System;
using UnityEngine;

[Serializable]
public class NPCDialogueLine
{
    [TextArea(2, 4)]
    public string text;

    [Min(0f)]
    public float duration = 2f;
}