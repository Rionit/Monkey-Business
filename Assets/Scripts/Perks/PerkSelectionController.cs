using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MonkeyBusiness.Managers;
using TMPro;
using Random = UnityEngine.Random;

namespace MonkeyBusiness.Perks
{
    public class PerkSelectionController : MonoBehaviour
    {
        public UnityEvent OnPerkSelected = new();
        public UnityEvent<string, bool> OnPerkAdded = new();
        public UnityEvent OnNegativePerkRemoved = new();

        [SerializeField] private SoundSource rollingSound;
        
        [BoxGroup("Setup")]
        [SerializeField] private GameObject perkPrefab;

        [BoxGroup("Setup")]
        [SerializeField] private RectTransform perkSelectionUI;

        [BoxGroup("Setup")]
        [SerializeField] private RectTransform leftAnchor;

        [BoxGroup("Setup")]
        [SerializeField] private RectTransform centerAnchor;

        [BoxGroup("Perk Pools")]
        [SerializeField] private List<PerkSO> positivePerks = new();

        [BoxGroup("Perk Pools")]
        [SerializeField] private List<PerkSO> negativePerks = new();

        [SerializeField] private Image background;
        [SerializeField] private Image arrow_up;
        [SerializeField] private Image arrow_down;
        [SerializeField] private TextMeshProUGUI positivePerkText;
        [SerializeField] private TextMeshProUGUI negativePerkText;

        private readonly List<GameObject> activePerks = new();

        [SerializeField] private List<Perk> permanentPerks = new();
        [SerializeField] private List<Perk> temporaryPerks = new();

        [SerializeField] private List<PerkSO> availableNegativePerks = new();
        [SerializeField] private List<PerkSO> usedNegativePerks = new();

        private Perk selectedPerk;
        private Perk negativePerk;

        private bool waitingForPositiveConfirm;
        private bool waitingForNegativeConfirm;

        private Coroutine positiveTimerRoutine;
        private Coroutine negativeTimerRoutine;

        private void Awake()
        {
            SetArrowAlpha(0f);
            SetBackgroundAlpha(0f);
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveDefeated.AddListener(ResetTemporaryPerks);
            }

            InitNegativePool();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveDefeated.RemoveListener(ResetTemporaryPerks);
            }
        }

        private void InitNegativePool()
        {
            availableNegativePerks.Clear();
            usedNegativePerks.Clear();
            availableNegativePerks.AddRange(negativePerks);
        }

        [Button]
        public void RandomizeNewPerks()
        {
            ClearPerks();

            List<PerkSO> selectedPerks = new();

            while (selectedPerks.Count < 3 && selectedPerks.Count < positivePerks.Count)
            {
                var perk = GetRandomPositivePerk();
                if (!selectedPerks.Contains(perk) && !(perk.isUnique && StatsManager.Instance._perksUsage.ContainsKey(perk) && StatsManager.Instance._perksUsage[perk]))
                    selectedPerks.Add(perk);
            }

            foreach (var perk in selectedPerks)
                activePerks.Add(InstantiatePerk(perk));
            
            positivePerkText.gameObject.SetActive(true);
        }

        private GameObject InstantiatePerk(PerkSO perkSO)
        {
            var go = Instantiate(perkPrefab, perkSelectionUI);

            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero;

            StartCoroutine(ScaleTween(rt, Vector3.one, 0.25f));

            var perk = go.GetComponent<Perk>();
            perk.Setup(perkSO);
            perk.SetNeutral();

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (selectedPerk == null)
                        SelectPerk(perk);
                    else if (selectedPerk == perk && waitingForPositiveConfirm)
                        ConfirmPositivePerk();
                });
            }

            return go;
        }

        private void SelectPerk(Perk perk)
        {
            if (selectedPerk != null) return;

            selectedPerk = perk;

            StopPositiveTimer();

            foreach (var obj in activePerks)
            {
                if (obj.GetComponent<Perk>() != selectedPerk)
                    StartCoroutine(FadeOut(obj));
            }

            RectTransform rt = selectedPerk.GetComponent<RectTransform>();
            rt.SetParent(transform, true);

            waitingForPositiveConfirm = true;

            StartCoroutine(MoveAndReveal(rt, leftAnchor.position, true));

            StartPositiveUI();
            positiveTimerRoutine = StartCoroutine(AutoConfirmPositive());
        }

        private IEnumerator AutoConfirmPositive()
        {
            yield return new WaitForSeconds(5f);
            ConfirmPositivePerk();
        }

        private IEnumerator AutoConfirmNegative()
        {
            yield return new WaitForSeconds(5f);
            ConfirmNegativePerk();
        }

        private void StopPositiveTimer()
        {
            if (positiveTimerRoutine != null)
                StopCoroutine(positiveTimerRoutine);
            positiveTimerRoutine = null;
        }

        private void StopNegativeTimer()
        {
            if (negativeTimerRoutine != null)
                StopCoroutine(negativeTimerRoutine);
            negativeTimerRoutine = null;
        }

        private void StartPositiveUI()
        {
            StopCoroutine(nameof(FadeArrows));
            StopCoroutine(nameof(FadeBackground));
            
            // show ONLY positive text
            positivePerkText.gameObject.SetActive(true);
            negativePerkText.gameObject.SetActive(false);

            arrow_up.gameObject.SetActive(true);
            arrow_down.gameObject.SetActive(false);

            SetArrowAlpha(0f, true);

            StartCoroutine(FadeBackground(
                new Color(0f, 1f, 0f, 0f),
                new Color(0f, 1f, 0f, 0.5f),
                0.25f
            ));

            StartCoroutine(FadeArrowIn(arrow_up));
        }

        private void StartNegativeUI()
        {
            StopCoroutine(nameof(FadeArrows));
            StopCoroutine(nameof(FadeBackground));

            // show ONLY negative text
            negativePerkText.gameObject.SetActive(true);
            positivePerkText.gameObject.SetActive(false);
            
            arrow_up.gameObject.SetActive(false);
            arrow_down.gameObject.SetActive(true);

            SetArrowAlpha(0f, false);

            StartCoroutine(FadeBackground(
                new Color(1f, 0f, 0f, 0f),
                new Color(1f, 0f, 0f, 0.5f),
                0.25f
            ));

            StartCoroutine(FadeArrowIn(arrow_down));
        }

        private IEnumerator FadeBackground(Color from, Color to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                background.color = Color.Lerp(from, to, t / duration);
                yield return null;
            }
            background.color = to;
        }

        private IEnumerator FadeArrows(bool positive)
        {
            float duration = 0.25f;
            float t = 0f;

            Color upFrom = arrow_up.color;
            Color downFrom = arrow_down.color;

            Color upTo = positive ? new Color(1,1,1,1) : new Color(1,1,1,0.2f);
            Color downTo = positive ? new Color(1,1,1,0.2f) : new Color(1,1,1,1);

            Vector2 upStart = arrow_up.rectTransform.anchoredPosition;
            Vector2 downStart = arrow_down.rectTransform.anchoredPosition;

            Vector2 upEnd = upStart + Vector2.up * 15f;
            Vector2 downEnd = downStart + Vector2.down * 15f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = t / duration;

                arrow_up.color = Color.Lerp(upFrom, upTo, k);
                arrow_down.color = Color.Lerp(downFrom, downTo, k);

                arrow_up.rectTransform.anchoredPosition = Vector2.Lerp(upStart, upEnd, k);
                arrow_down.rectTransform.anchoredPosition = Vector2.Lerp(downStart, downEnd, k);

                yield return null;
            }

            arrow_up.color = upTo;
            arrow_down.color = downTo;
        }

        private IEnumerator MoveAndReveal(RectTransform rt, Vector3 target, bool positive)
        {
            yield return MoveTweenWorld(rt, target, 0.4f);
            rt.GetComponent<Perk>().ForceSelect();
        }

        private void ConfirmPositivePerk()
        {
            if (!waitingForPositiveConfirm || selectedPerk == null) return;

            StopPositiveTimer();

            waitingForPositiveConfirm = false;

            selectedPerk.ApplyEffect();
            permanentPerks.Add(selectedPerk);
            OnPerkAdded.Invoke(selectedPerk.perkSO.effect.GetDescription(), true);

            StartCoroutine(HandlePositiveConfirmed());
        }

        private IEnumerator HandlePositiveConfirmed()
        {
            yield return new WaitForSeconds(0.15f);

            yield return FadeOutUI();

            if (selectedPerk != null)
                StartCoroutine(FadeOut(selectedPerk.gameObject));

            yield return new WaitForSeconds(0.25f);

            SpawnNegativeRollingPerk();
        }

        private void SpawnNegativeRollingPerk()
        {
            negativePerkText.gameObject.SetActive(true);
            
            var go = Instantiate(perkPrefab, perkSelectionUI);

            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.zero;
            rt.anchoredPosition = centerAnchor.anchoredPosition;

            negativePerk = go.GetComponent<Perk>();
            negativePerk.Setup(negativePerks[Random.Range(0, negativePerks.Count)]);
            negativePerk.SetNeutral();
            negativePerk.SetInteractable(false);

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (waitingForNegativeConfirm)
                        ConfirmNegativePerk();
                });
            }

            StartCoroutine(RollAnimation(negativePerk, rt));
        }

        private IEnumerator RollAnimation(Perk perk, RectTransform rt)
        {
            perk.SetInteractable(false);

            yield return ScaleTween(rt, Vector3.one, 0.25f);

            float duration = 1.2f;
            float elapsed = 0f;
            
            rollingSound.Play();

            while (elapsed < duration)
            {
                var random = negativePerks[Random.Range(0, negativePerks.Count)];

                perk.Setup(random);
                perk.SetNeutral();

                float step = Mathf.Lerp(0.05f, 0.2f, elapsed / duration);

                yield return new WaitForSeconds(step);
                elapsed += step;
            }

            perk.Setup(GetRandomNegativePerk());
            perk.ForceSelect();

            waitingForNegativeConfirm = true;

            StartNegativeUI();

            yield return MoveAndReveal(rt, leftAnchor.position, false);

            negativeTimerRoutine = StartCoroutine(AutoConfirmNegative());

            perk.SetInteractable(true);
        }

        private void ConfirmNegativePerk()
        {
            if (!waitingForNegativeConfirm || negativePerk == null) return;

            StopNegativeTimer();

            negativePerk.ApplyEffect();
            temporaryPerks.Add(negativePerk);

            OnPerkAdded.Invoke(negativePerk.perkSO.effect.GetDescription(), false);

            waitingForNegativeConfirm = false;

            StartCoroutine(FadeOutUI());
            StartCoroutine(FadeOut(negativePerk.gameObject));

            negativePerk = null;

            OnPerkSelected.Invoke();
        }

        private IEnumerator FadeOutUI()
        {
            float t = 0f;
            float duration = 0.25f;

            Color bgStart = background.color;

            Image activeArrow = arrow_up.gameObject.activeSelf ? arrow_up : arrow_down;

            Color arrowStart = activeArrow.color;

            Vector2 arrowStartPos = activeArrow.rectTransform.anchoredPosition;

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = t / duration;

                background.color = new Color(bgStart.r, bgStart.g, bgStart.b, Mathf.Lerp(bgStart.a, 0f, k));

                activeArrow.color = new Color(arrowStart.r, arrowStart.g, arrowStart.b, Mathf.Lerp(arrowStart.a, 0f, k));

                activeArrow.rectTransform.anchoredPosition = Vector2.Lerp(
                    arrowStartPos,
                    arrowStartPos + Vector2.down * 10f,
                    k
                );

                yield return null;
            }

            SetArrowAlpha(0f, true);
            SetArrowAlpha(0f, false);
            
            positivePerkText.gameObject.SetActive(false);
            negativePerkText.gameObject.SetActive(false);

            arrow_up.gameObject.SetActive(false);
            arrow_down.gameObject.SetActive(false);
        }
        
        private IEnumerator FadeArrowIn(Image arrow)
        {
            arrow.gameObject.SetActive(true);

            float t = 0f;
            float duration = 0.25f;

            Color start = new Color(arrow.color.r, arrow.color.g, arrow.color.b, 0f);
            Color end = new Color(arrow.color.r, arrow.color.g, arrow.color.b, 1f);

            Vector2 pos = arrow.rectTransform.anchoredPosition;

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = t / duration;

                arrow.color = Color.Lerp(start, end, k);
                arrow.rectTransform.anchoredPosition = Vector2.Lerp(
                    pos + Vector2.down * 10f,
                    pos,
                    k
                );

                yield return null;
            }

            arrow.color = end;
        }

        private void SetArrowAlpha(float a)
        {
            // default: hide both
            var up = arrow_up.color;
            arrow_up.color = new Color(up.r, up.g, up.b, a);

            var down = arrow_down.color;
            arrow_down.color = new Color(down.r, down.g, down.b, a);
        }
        
        private void SetArrowAlpha(float a, bool isUp)
        {
            if (isUp)
            {
                var c = arrow_up.color;
                arrow_up.color = new Color(c.r, c.g, c.b, a);
            }
            else
            {
                var c = arrow_down.color;
                arrow_down.color = new Color(c.r, c.g, c.b, a);
            }
        }

        private void SetBackgroundAlpha(float a)
        {
            var c = background.color;
            background.color = new Color(c.r, c.g, c.b, a);
        }

        private IEnumerator FadeOut(GameObject go)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();

            RectTransform rt = go.GetComponent<RectTransform>();

            float t = 0f;
            float d = 0.25f;

            Vector3 start = rt.localScale;

            while (t < d)
            {
                t += Time.deltaTime;
                float k = t / d;

                cg.alpha = 1f - k;
                rt.localScale = Vector3.Lerp(start, Vector3.zero, k);

                yield return null;
            }

            Destroy(go);
        }

        private IEnumerator MoveTweenWorld(RectTransform rt, Vector3 target, float duration)
        {
            Vector3 start = rt.position;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                rt.position = Vector3.Lerp(start, target, EaseOutBack(t / duration));
                yield return null;
            }
        }

        private IEnumerator ScaleTween(RectTransform rt, Vector3 target, float duration)
        {
            Vector3 start = rt.localScale;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                rt.localScale = Vector3.Lerp(start, target, EaseOutBack(t / duration));
                yield return null;
            }
        }

        private float EaseOutBack(float x)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
        }

        private void ClearPerks()
        {
            StopPositiveTimer();
            StopNegativeTimer();

            foreach (var p in activePerks)
                if (p) Destroy(p);

            activePerks.Clear();

            selectedPerk = null;
            negativePerk = null;

            waitingForPositiveConfirm = false;
            waitingForNegativeConfirm = false;

            SetArrowAlpha(0f);
            SetBackgroundAlpha(0f);
        }

        private void ResetTemporaryPerks()
        {
            foreach (var perk in temporaryPerks)
            {
                if (perk != null)
                    perk.Reset();

                OnNegativePerkRemoved.Invoke();
            }

            temporaryPerks.Clear();
        }

        private PerkSO GetRandomPositivePerk()
        {
            return positivePerks[Random.Range(0, positivePerks.Count)];
        }

        private PerkSO GetRandomNegativePerk()
        {
            if (availableNegativePerks.Count == 0)
                availableNegativePerks.AddRange(negativePerks);

            int i = Random.Range(0, availableNegativePerks.Count);
            var p = availableNegativePerks[i];

            availableNegativePerks.RemoveAt(i);
            usedNegativePerks.Add(p);

            return p;
        }

        [Button]
        private void ApplyPerk(PerkSO perkSO)
        {
            var go = Instantiate(perkPrefab, transform);
            go.SetActive(false);

            var perk = go.GetComponent<Perk>();
            if (perk == null) return;

            perk.Setup(perkSO);
            perk.ApplyEffect();

            if (perkSO.perkAlignment == PerkAlignment.Positive)
                permanentPerks.Add(perk);
            else
            {
                temporaryPerks.Add(perk);
                OnPerkAdded.Invoke(perkSO.effectName, false);
            }
        }
    }
}