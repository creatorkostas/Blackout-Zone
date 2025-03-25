using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Animator animator;
    private static readonly Vector3 _JumpVector = new Vector3(0f, 1f, 0f);
    private static readonly Vector3 _RightWalkVector = new Vector3(1f, 0f, 0f);
    private static readonly Vector3 _LeftWalkVector = new Vector3(-1f, 0f, 0f);
    private static readonly Vector3 _BackWalkVector = new Vector3(0f, 0f, -1f);
    private static readonly Vector3 _LeftForwardWalkVector = new Vector3(-0.71f, 0f, 0.71f);
    private static readonly Vector3 _RightForardWalkVector = new Vector3(0.71f, 0f, 0.71f);
    private static readonly Vector3 _LeftBackWalkVector = new Vector3(-0.71f, 0f, -0.71f);
    private static readonly Vector3 _RightBackWalkVector = new Vector3(0.71f, 0f, -0.71f);
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void MakeAnimation(bool isGrounded, Vector3 _direction, bool isRunning){
        if (isGrounded && _direction == _JumpVector) 
            animator.SetInteger("state", AnimationState.jump.GetHashCode());
        else if (_direction == Vector3.zero)
            animator.SetInteger("state", AnimationState.idle.GetHashCode());
        else if (_direction == _RightWalkVector)
            animator.SetInteger("state", AnimationState.walk_right.GetHashCode());       
        else if (_direction == _LeftWalkVector)
            animator.SetInteger("state", AnimationState.walk_left.GetHashCode());
        else if (_direction == _LeftForwardWalkVector) 
            animator.SetInteger("state", AnimationState.forward_walk_left.GetHashCode());
        else if (_direction == _RightForardWalkVector) 
            animator.SetInteger("state", AnimationState.forward_walk_right.GetHashCode());
        else if (_direction == _LeftBackWalkVector) 
            animator.SetInteger("state", AnimationState.back_walk_left.GetHashCode());
        else if (_direction == _RightBackWalkVector) 
            animator.SetInteger("state", AnimationState.back_walk_right.GetHashCode());
        else if (_direction == _BackWalkVector) 
            animator.SetInteger("state", AnimationState.back_walk.GetHashCode());
        else if (isRunning)
            animator.SetInteger("state", AnimationState.run.GetHashCode());
        else
            animator.SetInteger("state", AnimationState.walk.GetHashCode());
    }

}
