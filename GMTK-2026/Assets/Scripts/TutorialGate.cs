using UnityEngine;

namespace DefaultNamespace
{
    // Whether a tutorial is on screen. While one is, the board and the pause key ignore the player, so the
    // press that hides a tutorial never also grabs or selects a cell hidden behind it.
    public class TutorialGate
    {
        private int showing;
        private int hiddenOnFrame = -1;

        public bool IsShowing => showing > 0;

        // Also true for the rest of the frame a tutorial was hidden in: every Level/Drag listener hears the
        // same press, and the tutorial's own may run first and hide it.
        public bool BlocksInput => showing > 0 || Time.frameCount == hiddenOnFrame;

        public void Show()
        {
            showing++;
        }

        public void Hide()
        {
            showing = Mathf.Max(0, showing - 1);
            hiddenOnFrame = Time.frameCount;
        }
    }
}
