using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace PironGames.MLLib.Samples.QLearning
{
    public class QActor : MonoBehaviour
    {
        public float TransitionDuration = 0.35f;

        private bool m_isTransitioning = false;

        public bool IsTransitioning => m_isTransitioning;

        private Sequence _transitionSequence;

        public void Transition(GameObject target, bool instant = false)
        {
            if (target == null)
            {
                return;
            }

            if (instant)
            {
                if (_transitionSequence != null)
                {
                    _transitionSequence.Complete();
                    _transitionSequence = null;
                }

                transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
            }
            else
            {
                m_isTransitioning = true;

                _transitionSequence = transform.DOJump(target.transform.position, 1f, 1, TransitionDuration).SetEase(Ease.InOutBounce).OnComplete(() => 
                { 
                    m_isTransitioning = false; 
                });
            }
        }
    }
}