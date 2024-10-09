using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;


public class Player : NetworkBehaviour
{
    [SerializeField]
    private SimpleKCC _simpleKcc;

    [SerializeField]
    private float _speed = 5f;

    [SerializeField]
    private float _jumpImpulse = 10f;

    [Networked]
    private NetworkButtons _previousButtons { get; set; }

    public override void Spawned()
    {
        _simpleKcc.SetGravity(Physics.gravity.y * 2f);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            Vector3 worldDirection = _simpleKcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
            float jump = 0f;

            if (input.Buttons.WasPressed(_previousButtons, InputButton.Jump) && _simpleKcc.IsGrounded)
            {
                jump = _jumpImpulse;
            }

            _simpleKcc.Move(worldDirection.normalized * _speed, jump);
            _previousButtons = input.Buttons;
        }
    }
}
