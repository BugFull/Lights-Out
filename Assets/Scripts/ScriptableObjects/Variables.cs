using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Variables<T> : ScriptableObject
{
    [SerializeField]
    private T _value;
    public event Action<T> OnValueChanged;
    public T Value
    {
        get { return _value; }
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(value);
            }
        }
    }

}
