using System.Collections;
using Ami.BroAudio;
using DG.Tweening;
using Sirenix.OdinInspector;
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

        [SerializeField,MinMaxSlider( 1f, 3f)] private Vector2 mainMusicPitchRange = new Vector2(1f, 1.3f);

        [Header("Tempo")]
        [SerializeField] private float bpm = 120f;

        private float _lastBpm = DEFAULT_BPM;
        
        const float DEFAULT_BPM = 120f;

        [Header("Transition")]
        [SerializeField] private float perkTransitionDuration = 2.5f;

        private MusicState currentState = MusicState.None;
        private bool transitioning;

        private Sequence _pitchTweenSequence;

        private float _mainMusicPitch = 1f;
        private float _MainMusicPitch
        {
            get => _mainMusicPitch;
            set
            {
                _mainMusicPitch = value;
                mainMusic.SetPitch(value);
            }
        }

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

            bpm = _lastBpm; // Restore BPM to the last value before perk transition

            transitioning = false;
        }

        private IEnumerator PlayPerkRoutine()
        {
            transitioning = true;
            _lastBpm = bpm;

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
            bpm = DEFAULT_BPM; // Reset BPM to default for the transition

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


        // TODO: Fix so it changes pitch more than once
        public void ChangePitch(float ratio)
        {
            Debug.Log("Changing pitch with ratio " + ratio);
            if(_pitchTweenSequence != null && _pitchTweenSequence.active)
            {
                _pitchTweenSequence.Kill();
            }

            var newPitch = Mathf.Lerp(mainMusicPitchRange.x, mainMusicPitchRange.y, 1f - ratio);

            Debug.Log("New pitch " + newPitch);
            /*mainMusic.CurrentPlayer.AudioSource.pitchnewPitch, 0.5f);
            bpm = DEFAULT_BPM * ratio;*/
            _pitchTweenSequence = DOTween.Sequence();

            mainMusic.CurrentPlayer.AudioSource.pitch = newPitch;

            Debug.Assert(mainMusic.CurrentPlayer.AudioSource.pitch == newPitch, "Pitch was not set correctly on AudioSource!");
            //mainMusic.SetPitch(newPitch, 0.5f);
            bpm = DEFAULT_BPM * ratio;
        }
    }
}