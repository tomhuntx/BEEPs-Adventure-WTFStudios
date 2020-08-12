using UnityEngine;

public class Box : MonoBehaviour
{
    public enum Type { Cardboard, Explosive, Heavy }
    [SerializeField] private Type boxType;
    public Type TypeOf { get { return boxType; } }
}
