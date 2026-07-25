using UnityEngine;
using System;

public enum TrashType
{
    Red,
    Blue,
    Yellow,
    Green
}

public class Trash : MonoBehaviour
{
    public TrashType trashType = TrashType.Green;
    public float cleanUpTime = 2f;
    public int points = 1;

    public Action onTrashDestroyed;

    void OnDestroy()
    {
        if (onTrashDestroyed != null)
            onTrashDestroyed.Invoke();
    }
}