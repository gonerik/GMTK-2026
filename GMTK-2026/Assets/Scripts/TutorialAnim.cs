using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace DefaultNamespace
{
    public class TutorialAnim : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        [SerializeField] private List<AnimationClip> animations;
        private int currentAnimation;
        
        [Inject] private DefaultActions _defaultActions;

        private void Awake()
        {
            currentAnimation = 0;
        }

        private void Start()
        {
            if (animator != null && animations != null && animations.Count > 0)
            {
                animator.Play(animations[currentAnimation].name);
            }
        }
        
        private void OnEnable()
        {
            if (_defaultActions != null)
            {
                _defaultActions.Level.Drag.performed += OnDragPerformed;
            }
        }

        private void OnDragPerformed(InputAction.CallbackContext obj)
        {
            PlayNext();
        }

        private void OnDisable()
        {
            if (_defaultActions != null)
            {
                _defaultActions.Level.Drag.performed -= OnDragPerformed;
            }
        }
        
        private void PlayNext()
        {
            currentAnimation++;
            if (animations == null || currentAnimation >= animations.Count)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (animator != null)
            {
                animator.Play(animations[currentAnimation].name);
            }
        }
    }
}