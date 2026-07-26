using UnityEngine;
using Zenject;

namespace Audio
{
    public class MainGameMusicSwitcher: MonoBehaviour
    {
        [Inject] MusicService musicService;

        private void Start()
        {
            musicService.PlayGameMusic();
        }
    }
}