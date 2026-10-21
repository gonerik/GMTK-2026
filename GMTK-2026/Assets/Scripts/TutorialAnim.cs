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
        // Unscaled, because the game is paused while the tutorial is up.
        private float dismissibleAt;

        [Inject] private DefaultActions _defaultActions;
        [Inject] private TutorialGate _tutorialGate;

        private void Awake()
        {
            currentAnimation = 0;

            // The game is paused (timeScale 0) while the tutorial is up, so the clips must ignore it.
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
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
            Time.timeScale = 0f;
            _tutorialGate?.Show();

            if (_defaultActions != null)
            {
                _defaultActions.Level.Drag.performed += OnDragPerformed;
            }
        }

        // Presses during the lock are ignored, and the gate keeps them off the board too.
        public void BlockDismissFor(float seconds)
        {
            dismissibleAt = Time.unscaledTime + seconds;
        }

        private void OnDragPerformed(InputAction.CallbackContext obj)
        {
            if (Time.unscaledTime < dismissibleAt) return;
            PlayNext();
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
            _tutorialGate?.Hide();

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