using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MonkeyBusiness.Perks
{
    public class PerkSelectionController : MonoBehaviour
    {
        public UnityEvent OnPerkSelected = new();

        [BoxGroup("Setup")] [SerializeField] private GameObject perkPrefab;
        [BoxGroup("Setup")] [SerializeField] private RectTransform perkSelectionUI;
        [BoxGroup("Setup")] [SerializeField] private RectTransform leftAnchor;
        [BoxGroup("Setup")] [SerializeField] private RectTransform centerAnchor;

        [BoxGroup("Setup")] [SerializeField] private List<PerkSO> perks;

        private readonly List<GameObject> activePerks = new();

        private Perk selectedPerk;
        private Perk negativePerk;

        private bool waitingForPositiveConfirm;
        private bool waitingForNegativeConfirm;
        private bool negativeRevealed;

        [Button]
        public void RandomizeNewPerks()
        {
            ClearPerks();

            for (int i = 0; i < 3; i++)
                activePerks.Add(InstantiatePerk(GetRandomPerk()));
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
                    {
                        SelectPerk(perk);
                    }
                    else if (selectedPerk == perk && waitingForPositiveConfirm)
                    {
                        ConfirmPerk();
                    }
                });
            }

            return go;
        }

        private void SelectPerk(Perk perk)
        {
            if (selectedPerk != null) return;

            selectedPerk = perk;

            foreach (var obj in activePerks)
            {
                if (obj.GetComponent<Perk>() != selectedPerk)
                    StartCoroutine(FadeOut(obj));
            }

            RectTransform rt = selectedPerk.GetComponent<RectTransform>();
            rt.SetParent(transform, true);

            StartCoroutine(MoveAndRevealPositive(rt));

            waitingForPositiveConfirm = true;
        }

        private IEnumerator MoveAndRevealPositive(RectTransform rt)
        {
            yield return MoveTweenWorld(rt, leftAnchor.position, 0.4f);
            selectedPerk.ForceResult(true);
        }

        private void ConfirmPerk()
        {
            // POSITIVE CONFIRM
            if (waitingForPositiveConfirm && selectedPerk != null)
            {
                waitingForPositiveConfirm = false;
                selectedPerk.ApplyEffect();
                StartCoroutine(HandlePositiveConfirmed());
                return;
            }

            // NEGATIVE REVEAL -> CONFIRM (SECOND CLICK)
            if (negativePerk != null && negativeRevealed)
            {
                negativePerk.ApplyEffect();
                
                negativePerk.SetInteractable(false);
                StartCoroutine(FadeOut(negativePerk.gameObject));

                negativePerk = null;
                negativeRevealed = false;
                waitingForNegativeConfirm = false;

                OnPerkSelected.Invoke();
            }
        }

        private IEnumerator HandlePositiveConfirmed()
        {
            yield return new WaitForSeconds(0.25f);

            if (selectedPerk != null)
                StartCoroutine(FadeOut(selectedPerk.gameObject));

            yield return new WaitForSeconds(0.25f);

            SpawnNegativeRollingPerk();
        }

        private void SpawnNegativeRollingPerk()
        {
            var go = Instantiate(perkPrefab, perkSelectionUI);
            var rt = go.GetComponent<RectTransform>();

            rt.localScale = Vector3.zero;
            rt.anchoredPosition = centerAnchor.anchoredPosition;

            negativePerk = go.GetComponent<Perk>();
            negativePerk.Setup(GetRandomPerk());
            negativePerk.SetNeutral();

            // MUST stay disabled during rolling
            negativePerk.SetInteractable(false);

            negativeRevealed = false;
            waitingForNegativeConfirm = true;

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (!waitingForNegativeConfirm || negativePerk == null) return;

                    if (!negativeRevealed)
                    {
                        negativePerk.ForceResult(false);
                        negativePerk.SetInteractable(true); // enable AFTER reveal only
                        negativeRevealed = true;
                        return;
                    }

                    if (negativeRevealed)
                    {
                        ConfirmPerk();
                    }
                });
            }

            StartCoroutine(RollAnimation(negativePerk, rt));
        }


        private IEnumerator RollAnimation(Perk perk, RectTransform rt)
        {
            // keep disabled during animation
            perk.SetInteractable(false);

            yield return ScaleTween(rt, Vector3.one, 0.25f);

            float duration = 1.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                var random = GetRandomPerk();
                perk.Setup(random);
                perk.SetNeutral();

                float step = Mathf.Lerp(0.05f, 0.2f, elapsed / duration);
                yield return new WaitForSeconds(step);

                elapsed += step;
            }

            perk.Setup(GetRandomPerk());
            perk.SetNeutral();

            perk.SetInteractable(true);
        }

        private IEnumerator FadeOut(GameObject go)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();

            RectTransform rt = go.GetComponent<RectTransform>();

            float t = 0;
            float d = 0.25f;

            Vector3 startScale = rt.localScale;

            while (t < d)
            {
                t += Time.deltaTime;
                float lerp = t / d;

                cg.alpha = 1 - lerp;
                rt.localScale = Vector3.Lerp(startScale, Vector3.zero, lerp);

                yield return null;
            }

            Destroy(go);
        }

        private IEnumerator MoveTweenWorld(RectTransform rt, Vector3 target, float duration)
        {
            Vector3 start = rt.position;
            float t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float eased = EaseOutBack(t / duration);
                rt.position = Vector3.Lerp(start, target, eased);
                yield return null;
            }
        }

        private IEnumerator ScaleTween(RectTransform rt, Vector3 target, float duration)
        {
            Vector3 start = rt.localScale;
            float t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float eased = EaseOutBack(t / duration);
                rt.localScale = Vector3.Lerp(start, target, eased);
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
            foreach (var p in activePerks)
                if (p) Destroy(p);

            activePerks.Clear();

            selectedPerk = null;
            negativePerk = null;
            negativeRevealed = false;
            waitingForPositiveConfirm = false;
            waitingForNegativeConfirm = false;
        }

        private PerkSO GetRandomPerk()
        {
            return perks[Random.Range(0, perks.Count)];
        }
    }
}