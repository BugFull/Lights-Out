using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GameEvent", menuName ="GameEvent")]
public class GameEvent : ScriptableObject
{
    public Action<Component, object> ActionEvent;   
}
