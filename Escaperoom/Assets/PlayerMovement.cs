using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    public float PlayersMovementSPeed; //this is players movement speed.
    public float PlayerJumpingForce; //this is players jumping force.
    private float _playersMovementDirection = 0; //this will give the direction of the players movement.
    private Input _inputActionReference; // reference of the generated c# script form the input action
    private Rigidbody2D _playersRigidBody; //reference of the players rigid body.


    private void Start()
    {
        //Getting the reference of the players rigid body.
        _playersRigidBody ??= GetComponent<Rigidbody2D>();

        _inputActionReference = new InputSystem_Actions();
        //enabling the Input actions
        _inputActionReference.Enable();
        //reading the values of the player movement direction for the players movement.
        _inputActionReference.Ground.Move.performed += moving =>
        {
            _playersMovementDirection = moving.ReadValue<float>();
        };

        //Jumping the player
        _inputActionReference.Ground.Jump.performed += jumping => { JumpThePlayer(); };
    }


    private void FixedUpdate()
    {
        //Moving player using player rigid body.
        _playersRigidBody.linearVelocity =
            new Vector3(_playersMovementDirection * PlayersMovementSPeed, _playersRigidBody.linearVelocity.y);
    }


    private void JumpThePlayer()
    {
        //Moving player using player rigid body.
        _playersRigidBody.linearVelocity = Vector3.up * PlayerJumpingForce;
    }
}