using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Presentation.Animations
{
    public class FlipAnimTest : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Sprite backSprite, frontSprite;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float scaleSpeed = 0.12f, moveSpeed = 0.12f, moveValue = 20f;

        [ContextMenu("Flip to Front")]
        public void FlipToFront()
        {
            Sequence seq = DOTween.Sequence();

            var startPos = transform.localPosition;

            seq.Join(transform.DOScaleX(0f, scaleSpeed));
            seq.Join(transform.DOLocalMoveX(startPos.x + moveValue, moveSpeed));
            seq.AppendCallback(() => { spriteRenderer.sprite = frontSprite; });

            seq.Append(transform.DOScaleX(1f, scaleSpeed));
            seq.Join(transform.DOLocalMoveX(startPos.x, moveSpeed));
        }

        [ContextMenu("Flip to Back")]
        public void FlipToBack()
        {
            Sequence seq = DOTween.Sequence();

            var startPos = transform.localPosition;

            seq.Join(transform.DOScaleX(0f, scaleSpeed));
            seq.Join(transform.DOLocalMoveX(startPos.x - moveValue, moveSpeed));

            seq.AppendCallback(() => { spriteRenderer.sprite = backSprite; });

            seq.Append(transform.DOScaleX(1f, scaleSpeed));
            seq.Join(transform.DOLocalMoveX(startPos.x, moveSpeed));
        }

        [ContextMenu("Flip to Front Clean")]
        public void FlipToFrontClean()
        {
            var startPos = transform.localPosition;
            var startScale = transform.localScale;

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOScaleX(0.08f, 0.10f).SetEase(Ease.InSine));
            seq.Join(transform.DOLocalMoveX(startPos.x + 10f, 0.10f).SetEase(Ease.InSine));

            seq.AppendCallback(() => { spriteRenderer.sprite = frontSprite; });

            seq.Append(transform.DOScaleX(startScale.x, 0.12f).SetEase(Ease.OutSine));
            seq.Join(transform.DOLocalMoveX(startPos.x, 0.12f).SetEase(Ease.OutSine));
        }

        [ContextMenu("Flip to Back Clean")]
        public void FlipToBackClean()
        {
            var startPos = transform.localPosition;
            var startScale = transform.localScale;

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOScaleX(0.08f, 0.10f).SetEase(Ease.InSine));
            seq.Join(transform.DOLocalMoveX(startPos.x - 10f, 0.10f).SetEase(Ease.InSine));

            seq.AppendCallback(() => { spriteRenderer.sprite = backSprite; });

            seq.Append(transform.DOScaleX(startScale.x, 0.12f).SetEase(Ease.OutSine));
            seq.Join(transform.DOLocalMoveX(startPos.x, 0.12f).SetEase(Ease.OutSine));
        }

        private Vector3 _cachedStartPosition;

        private void Awake()
        {
            _cachedStartPosition = transform.position;
        }

        [SerializeField] private Transform target;
        [SerializeField] private float arcHeight = 15f;
        [SerializeField] private float moveToBTime = 0.25f;
        [SerializeField] private float moveToCTime = 0.18f;
        [SerializeField] private float rotateAmount = 180f;
        [SerializeField] private float squeezeTime = 0.08f;

        [ContextMenu("Play A B C")]
        public void PlayABC()
        {
            Vector3 a = transform.position;
            Vector3 c = target.position;
            Vector3 b = new Vector3(
                c.x,
                c.y + arcHeight,
                a.z
            );

            float rotateDir = a.x < c.x ? -1f : 1f;
            Vector3 startScale = transform.localScale;

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOMove(b, moveToBTime).SetEase(Ease.OutQuad));
            seq.Join(transform.DORotate(
                new Vector3(0f, 0f, 360f * rotateDir),
                moveToBTime,
                RotateMode.FastBeyond360
            ).SetEase(Ease.OutQuad));

            seq.Append(transform.DOMove(c, moveToCTime).SetEase(Ease.InQuad));

            seq.Append(transform.DOScale(
                new Vector3(startScale.x * 1.12f, startScale.y * 0.88f, startScale.z),
                squeezeTime
            ).SetEase(Ease.OutQuad));

            seq.Append(transform.DOScale(startScale, squeezeTime).SetEase(Ease.OutBack));

            seq.AppendCallback(() =>
            {
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.identity;
            });
        }

        [ContextMenu("Play C B A")]
        public void PlayCBA()
        {
            Vector3 c = target.position;
            Vector3 a = _cachedStartPosition;

            Vector3 b = new Vector3(
                c.x,
                c.y + arcHeight,
                c.z
            );

            float rotateDir = c.x < a.x ? -1f : 1f;
            Vector3 startScale = transform.localScale;

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOMove(b, moveToBTime).SetEase(Ease.OutQuad));
            seq.Join(transform.DORotate(
                new Vector3(0f, 0f, 360f * rotateDir),
                moveToBTime,
                RotateMode.FastBeyond360
            ).SetEase(Ease.OutQuad));

            seq.Append(transform.DOMove(a, moveToCTime).SetEase(Ease.InQuad));

            seq.Append(transform.DOScale(
                new Vector3(startScale.x * 1.12f, startScale.y * 0.88f, startScale.z),
                squeezeTime
            ).SetEase(Ease.OutQuad));

            seq.Append(transform.DOScale(startScale, squeezeTime).SetEase(Ease.OutBack));

            seq.AppendCallback(() =>
            {
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.identity;
            });
        }

        private bool x = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            XX();
        }

        public void XX()
        {
            if (!x)
            {
                x = true;
                PlayABC();
            }
            else
            {
                x = false;
                PlayCBA();
            }
        }
    }
}