using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON1 = 1;
    public const byte MOUSEBUTTON2 = 2;
     
    
    
    public float mouseX;
    public float mouseY;


    public byte buttons;

  
    public Vector3 direction;

}