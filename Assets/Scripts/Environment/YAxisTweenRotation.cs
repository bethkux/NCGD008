namespace VerdantBrews
{
    using UnityEngine;
    using DG.Tweening;

    /// <summary>
    /// Smoothly rotates the object left and right on the Y axis only.
    /// </summary>
    public class YAxisTweenRotation : MonoBehaviour
    {
        public float rotationRange = 30f;
        public float speed = 60f;

        private Tween rotationTween;

        void Start()
        {
            Vector3 start = transform.localEulerAngles;
            float duration = (rotationRange * 2f) / speed;

            rotationTween = transform.DOLocalRotate(
                    new Vector3(start.x, start.y + rotationRange, start.z),
                    duration / 2f,
                    RotateMode.Fast
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        void OnDestroy()
        {
            rotationTween?.Kill();
        }
    }
}