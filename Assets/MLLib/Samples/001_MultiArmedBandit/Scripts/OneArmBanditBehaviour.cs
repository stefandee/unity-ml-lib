using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PironGames.MLLib.Samples.MultiArmBandit
{
    public class OneArmBanditBehaviour : MonoBehaviour
    {
        public TMP_Text SuccessRate;
        public TMP_Text ObservedSuccessRate;
        public Image Icon;
        public int FloatDecimals = 2;

        private Sequence m_Seq;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnDestroy()
        {
            // TODO
        }

        public void Reset(float successRate, float observedSuccessRate)
        {
            UpdateSuccessRate(successRate);
            UpdateObservedSuccessRate(observedSuccessRate, false);
        }

        public void UpdateSuccessRate(float rate)
        {
            SuccessRate.text = rate.ToString($"N{FloatDecimals}");
        }

        public void UpdateObservedSuccessRate(float rate, bool animate = true)
        {
            if (float.IsNaN(rate))
            {
                ObservedSuccessRate.text = "n/a";
            }
            else
            {
                ObservedSuccessRate.text = rate.ToString($"N{FloatDecimals}");
            }

            if (animate)
            {
                AnimateIcon();
            }
        }

        private void AnimateIcon()
        {
            var t = Icon.transform;

            //if (m_Seq == null)
            {
                m_Seq = DOTween.Sequence()
                            .Append(t.DOScale(new Vector2(1.2f, 1.2f), 0.025f).SetDelay(0))
                            .Append(t.DOScale(Vector2.one, 0.025f))
                            .SetLoops(1, LoopType.Restart)
                            .SetAutoKill(true);
            }
        }
    }
}
