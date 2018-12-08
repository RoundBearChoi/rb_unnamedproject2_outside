﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveTransitionStates
{
    NONE,
    WALK,
    RUN,
    JUMP
}

namespace roundbeargames
{
    public class Move : StateComponent
    {
        public float AirMaxSpeed;
        public float AirSpeedGain;
        private float AirMomentumDecreaseMultiplier;
        private PathFinder pathFinder;

        public MoveTransitionStates GetMoveTransition()
        {
            if (characterData == null)
            {
                return MoveTransitionStates.NONE;
            }

            CharacterMovementData moveData = characterData.characterMovementData;

            if (moveData == null)
            {
                return MoveTransitionStates.NONE;
            }

            if (moveData.Jump)
            {
                return MoveTransitionStates.JUMP;
            }

            if (moveData.MoveForward)
            {
                if (moveData.Run)
                {
                    return MoveTransitionStates.RUN;
                }
                else
                {
                    return MoveTransitionStates.WALK;
                }
            }

            if (moveData.MoveBack)
            {
                if (moveData.Run)
                {
                    return MoveTransitionStates.RUN;
                }
                else
                {
                    return MoveTransitionStates.WALK;
                }
            }

            return MoveTransitionStates.NONE;
        }

        public float GetTurn()
        {
            if (controlMechanism == null)
            {
                return 0;
            }

            if (characterData == null)
            {
                return 0;
            }

            CharacterMovementData movementData = characterData.characterMovementData;
            Transform characterTransform = controlMechanism.transform;

            if (controlMechanism.IsFacingForward())
            {
                if (movementData.MoveBack && !movementData.MoveForward)
                {
                    return 180;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (!movementData.MoveBack && movementData.MoveForward)
                {
                    return 0;
                }
                else
                {
                    return 180;
                }
            }
        }

        public void MoveForward(float Speed, float yAngle)
        {
            if (controlMechanism == null)
            {
                return;
            }

            Transform characterTransform = controlMechanism.transform;

            characterTransform.rotation = Quaternion.Euler(0, yAngle, 0);

            if (!characterData.IsTouchingGeneralObject(TouchDetectorType.FRONT))
            {
                characterTransform.Translate(Vector3.right * Speed * Time.deltaTime);
            }
        }

        public void SimpleMove(float Speed, bool goForward)
        {
            if (controlMechanism == null)
            {
                return;
            }

            Transform characterTransform = controlMechanism.transform;

            if (goForward)
            {
                if (!characterData.IsTouchingGeneralObject(TouchDetectorType.FRONT))
                {
                    characterTransform.Translate(Vector3.right * Speed * Time.deltaTime);
                }
            }
            else
            {
                if (!characterData.IsTouchingGeneralObject(TouchDetectorType.BACK))
                {
                    characterTransform.Translate(-Vector3.right * Speed * Time.deltaTime);
                }
            }
        }

        public void MoveWithoutTurning(float Speed, float turn)
        {
            if (controlMechanism == null)
            {
                return;
            }

            Transform characterTransform = controlMechanism.transform;

            if (characterData.characterMovementData.MoveForward || characterData.characterMovementData.MoveBack)
            {
                if (characterTransform.rotation.eulerAngles.y != turn)
                {
                    SimpleMove(Speed, false);
                }
                else
                {
                    SimpleMove(Speed, true);
                }
            }
        }

        public void AirMove()
        {
            if (controlMechanism == null)
            {
                return;
            }

            Transform characterTransform = controlMechanism.transform;

            if (movementData.MoveBack)
            {
                if (movementData.AirMomentum <= AirMaxSpeed * -1f)
                {
                    movementData.AirMomentum = Mathf.Lerp(movementData.AirMomentum, AirMaxSpeed * -1f, Time.deltaTime * AirSpeedGain * 0.5f);
                }
                else if (movementData.AirMomentum > (AirMaxSpeed * -1f))
                {
                    movementData.AirMomentum -= Time.deltaTime * AirSpeedGain;
                }
            }

            if (movementData.MoveForward)
            {

                if (movementData.AirMomentum >= AirMaxSpeed)
                {
                    movementData.AirMomentum = Mathf.Lerp(movementData.AirMomentum, AirMaxSpeed, Time.deltaTime * AirSpeedGain * 0.5f);
                }
                else if (movementData.AirMomentum < (AirMaxSpeed))
                {
                    movementData.AirMomentum += Time.deltaTime * AirSpeedGain;
                }
            }

            if (!movementData.MoveForward && !movementData.MoveBack)
            {
                movementData.AirMomentum = Mathf.Lerp(movementData.AirMomentum, 0f, Time.deltaTime * AirSpeedGain * 0.5f);
            }

            if (movementData.AirMomentum > 0f)
            {
                SimpleMove(movementData.AirMomentum, controlMechanism.IsFacingForward());
            }
            else if (movementData.AirMomentum < 0f)
            {
                SimpleMove(movementData.AirMomentum, controlMechanism.IsFacingForward());
            }
        }

        public void CheckFall()
        {
            if (characterData == null)
            {
                return;
            }

            if (Mathf.Abs(controlMechanism.RIGIDBODY.velocity.y) > 0.025f)
            {
                if (!characterData.characterMovementData.IsGrounded)
                {
                    controlMechanism.RIGIDBODY.useGravity = true;
                    characterStateController.ChangeState((int)PlayerState.FallALoop);
                }
            }
        }

        public List<WayPoint> FindClosestPathTo(WayPoint from, WayPoint to)
        {
            if (pathFinder == null)
            {
                pathFinder = GameObject.FindObjectOfType<PathFinder>();
            }
            return pathFinder.FindPath(from, to);
        }
    }
}