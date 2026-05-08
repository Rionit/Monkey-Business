using System.Collections;
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonkeyBusiness.Misc
{
    public class MusicController : MonoBehaviour
    {
        private enum MusicState
        {
            None,
            Main,
            Perk
        }

        [Header("Music")]
        [SerializeField] private SoundSource mainMusic;
        [SerializeField] private SoundSource perkLoopMusic;
        [SerializeField] private SoundSource perkTransitionSound;

        [Header("Tempo")]
        [SerializeField] private float bpm = 120f;

        [Header("Transition")]
        [SerializeField] private float perkTransitionDuration = 2.5f;

        private MusicState currentState = MusicState.None;
        private bool transitioning;

        public void PlayMain()
        {
            if (transitioning || currentState == MusicState.Main)
                return;

            StartCoroutine(PlayMainRoutine());
        }

        public void PlayPerkLoop()
        {
            if (transitioning || currentState == MusicState.Perk)
                return;

            StartCoroutine(PlayPerkRoutine());
        }

        private IEnumerator PlayMainRoutine()
        {
            transitioning = true;

            // Nothing playing yet
            if (currentState == MusicState.None)
            {
                mainMusic.Play();

                currentState = MusicState.Main;
                transitioning = false;
                yield break;
            }

            // PERK -> MAIN
            if (currentState == MusicState.Perk)
            {
                // Start riser on beat
                yield return WaitForNextBeat();

                perkTransitionSound.Play();

                // Wait for riser to finish
                yield return new WaitForSeconds(perkTransitionDuration);

                // Hard cut perk
                perkLoopMusic.Stop();

                // Start main immediately
                mainMusic.Play();

                currentState = MusicState.Main;
                transitioning = false;
                yield break;
            }

            transitioning = false;
        }

        private IEnumerator PlayPerkRoutine()
        {
            transitioning = true;

            // MAIN -> PERK
            if (currentState == MusicState.Main)
            {
                yield return WaitForNextBeat();

                mainMusic.Stop();
            }

            // NONE -> PERK
            if (currentState == MusicState.None)
            {
                yield return WaitForNextBeat();
            }

            perkLoopMusic.Play();

            currentState = MusicState.Perk;
            transitioning = false;
        }

        private IEnumerator WaitForNextBeat()
        {
            double beatDuration = 60.0 / bpm;
            double dspTime = AudioSettings.dspTime;

            double nextBeat =
                Mathf.Ceil((float)(dspTime / beatDuration)) * beatDuration;

            while (AudioSettings.dspTime < nextBeat)
                yield return null;
        }
    }
}