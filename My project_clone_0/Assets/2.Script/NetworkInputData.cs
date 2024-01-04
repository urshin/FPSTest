using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;




    public NetworkButtons buttons;
    public Vector3 direction;

    public Vector3 moveDirection;

    public float mouseX;
    public float mouseY;

    
    public enum NetworkInputButtons
    {
        Jump
    }

}