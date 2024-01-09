using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkInputData;

public class PlayerMovementHandler : NetworkBehaviour
{
    NetworkCharacterController _cc;

    //�ִϸ��̼�
    [SerializeField] Animator bodyAnime;

    //���� ��.
    private Vector3 _forward = Vector3.forward;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);
            transform.rotation = data.mouseDirection;

            if (data.buttons.IsSet(NetworkInputButtons.Jump))
            {
                _cc.Jump();
            }
            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;



            //�� �ִϸ��̼�
            Vector2 runVector = new Vector2(_cc.Velocity.x, _cc.Velocity.z);
            runVector.Normalize();
            float speed = Mathf.Sqrt(runVector.magnitude);
            bodyAnime.SetFloat("Direction", speed);




        }
    }
}

