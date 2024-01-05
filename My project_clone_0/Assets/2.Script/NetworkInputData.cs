using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;
    public const byte MOUSEBUTTON1 = 2;




    public NetworkButtons buttons;
    public Vector3 direction;

    public Vector3 moveDirection;
    public Quaternion mouseDirection;

    public float mouseX;
    public float mouseY;

    
    public enum NetworkInputButtons
    {
        Jump
    }

}