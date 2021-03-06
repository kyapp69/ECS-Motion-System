﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using MotionSystem.Components;
using Unity.Collections;
using Unity.Jobs;

namespace MotionSystem.System
{

   [DisableAutoCreation]
    public class AnimatorUpdate : ComponentSystem
    {

        EntityQueryDesc GroundChecker = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(CharControllerE), typeof(Transform), typeof(Animator), typeof(Rigidbody) }
        };
        const float k_Half = 0.5f;

        protected override void OnUpdate()
        {


            Entities.ForEach((ref CharControllerE control, Transform transform, Animator Anim, Rigidbody RB) =>
            {
                float m_TurnAmount;
                float m_ForwardAmount;


                Anim.applyRootMotion = control.IsGrounded;
                control.Move = Vector3.ProjectOnPlane(control.Move, control.GroundNormal);

                m_TurnAmount = Mathf.Atan2(control.Move.x, control.Move.z);
                m_ForwardAmount = control.Move.z;

                float turnSpeed = Mathf.Lerp(control.m_StationaryTurnSpeed, control.m_MovingTurnSpeed, m_ForwardAmount);
                transform.Rotate(0, m_TurnAmount * turnSpeed * Time.fixedDeltaTime, 0);


                if (control.IsGrounded)
                {
                    if (control.Jump && !control.Crouch)
                    {
                        if (Anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded") || Anim.GetCurrentAnimatorStateInfo(0).IsName("Reaper locomation Grounded"))
                        {
                            // jump!
                            Debug.Log("Jump");
                            RB.velocity = new Vector3(RB.velocity.x, control.m_JumpPower, RB.velocity.z);
                            control.IsGrounded = false;
                            Anim.applyRootMotion = false;
                            control.GroundCheckDistance = 0.1f;
                        }
                    }
                }
                else
                {
                    Vector3 extraGravityForce = (Physics.gravity * control.m_GravityMultiplier) - Physics.gravity;
                    RB.AddForce(extraGravityForce);

                    control.GroundCheckDistance = RB.velocity.y < 0 ? control.m_OrigGroundCheckDistance : 0.1f;
                }

                //ScaleCapsules Collider

                //AutoCrouch 


                // Animator Updater

                // update the animator parameters
                Anim.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.fixedDeltaTime);
                Anim.SetFloat("Turn", m_TurnAmount, 0.1f, Time.fixedDeltaTime);
                Anim.SetBool("Crouch", control.Crouch);
                Anim.SetBool("OnGround", control.IsGrounded);

                    if (!control.IsGrounded)
                {
                    Anim.SetFloat("Jump", RB.velocity.y);
                }
                Anim.SetBool("Weapon Drawn", control.EquipWeapon);

                // calculate which leg is behind, so as to leave that leg trailing in the jump animation
                // (This code is reliant on the specific run cycle offset in our animations,
                // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
                float runCycle =
                    Mathf.Repeat(
                        Anim.GetCurrentAnimatorStateInfo(0).normalizedTime + control.m_RunCycleLegOffset, 1);
                float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
                if (control.IsGrounded)
                {
                    Anim.SetFloat("JumpLeg", jumpLeg);
                }

                // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
                // which affects the movement speed because of the root motion.
                if (control.IsGrounded && control.Move.magnitude > 0)
                {
                    Anim.speed = control.m_AnimSpeedMultiplier;
                }
                else
                {
                    // don't use that while airborne
                    Anim.speed = 1;
                }

                control.Jump = false;



            });
            Entities.ForEach((ref CharControllerE Control, InputQueuer QueueInput, Animator Anim) => {
                if (!Control.CombatCapable)
                    return;
                Anim.SetBool("Block", Control.block);
                if (Control.block)
                {
                    Control.TimerForEquipReset = Control.EquipResetTimer;
                }
                if (QueueInput.InputQueue.Count > 0) {
                    string Dequeue = QueueInput.InputQueue.Dequeue() as string;
                    if (!Control.EquipWeapon){
                        Control.EquipWeapon = true;
                    }
                    Anim.SetTrigger(Dequeue);
                    Control.TimerForEquipReset = Control.EquipResetTimer; 
                }

                if (Anim.GetCurrentAnimatorStateInfo(0).IsTag("EndCombo")) {
                    Anim.ResetTrigger("Light Attack");
                }
            });
                Entities.ForEach((ref CharControllerE Control, CapsuleCollider capsule) =>
            {
                capsule.center = Control.CapsuleCenter;
                capsule.height = Control.CapsuleHeight;

            }
      );
        }



    }
}