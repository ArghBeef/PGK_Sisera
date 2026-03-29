using System.Collections.Generic;
using UnityEngine;

public static class NPCDialogueMemory
{
    private static readonly HashSet<string> completedConversations = new HashSet<string>();

    public static bool HasCompleted(string npcA, string npcB)
    {
        return completedConversations.Contains(BuildKey(npcA, npcB));
    }

    public static void MarkCompleted(string npcA, string npcB)
    {
        completedConversations.Add(BuildKey(npcA, npcB));
    }

    private static string BuildKey(string a, string b)
    {
        if (string.CompareOrdinal(a, b) < 0)
            return a + "|" + b;

        return b + "|" + a;
    }

    public static void ClearAll()
    {
        completedConversations.Clear();
    }
}