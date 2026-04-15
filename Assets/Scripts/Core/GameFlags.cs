using System.Collections.Generic;
using UnityEngine;

public class GameFlags : MonoBehaviour
{
    public static GameFlags Instance;

    private HashSet<string> flags = new ();

    void Awake() => Instance = this;

    public bool HasFlag(string flag) => flags.Contains(flag);
    public void SetFlag(string flag) => flags.Add(flag);
}
