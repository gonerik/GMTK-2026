using UnityEngine;
using Zenject;

namespace Audio
{
    public class MainGameMusicSwitcher: MonoBehaviour
    {
        [Inject] MusicService musicService;

        private void Start()
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Go To 2", 0);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Go To 3", 0);
            musicService.PlayGameMusic();
        }
    }
}