using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public Vector2Int Position;
    [HideInInspector] public bool IsMoving;

    private Vector3 _transPosition;
    public Vector3 TransPosition
    {
        get
        {
            float cellWidth = _bgSprite.sprite.bounds.size.x;
            float cellHeight = _bgSprite.sprite.bounds.size.y;

            _transPosition.x = Position.x * cellWidth;
            _transPosition.y = Position.y * cellHeight;

            return _transPosition;
        }
    }

    public float SpriteWidth => _bgSprite.sprite.bounds.size.x;
    public float SpriteHeight => _bgSprite.sprite.bounds.size.y;

    public bool IsStartTweenPlaying => startAnimation.IsActive();
    public bool IsStartMovePlaying => startMoveAnimation.IsActive();
    public bool hasSelectedMoveFinished => !selectedMoveAnimation.IsActive();
    public bool hasMoveFinished => !moveAnimation.IsActive();

    public bool IsAnimating
    {
        get
        {
            return startAnimation.IsActive() ||
                startMoveAnimation.IsActive() ||
                selectedMoveAnimation.IsActive() ||
                moveAnimation.IsActive();
        }
    }

    [SerializeField] private SpriteRenderer _bgSprite;
    [SerializeField] private SpriteRenderer _borderSprite;
    [SerializeField] private BoxCollider2D _boxCollider;
    [SerializeField] private float _imgScale = 0.75f;
    [SerializeField] private float _startScaleDelay = 0.04f;
    [SerializeField] private float _startScaleTime = 0.2f;
    [SerializeField] private float _startMoveAnimationTime = 0.32f;
    [SerializeField] private float _selectedMoveAnimationTime = 0.16f;
    [SerializeField] private float _moveAnimationTime = 0.32f;

    private Tween startAnimation;
    private Tween spriteAnimation;
    private Tween startMoveAnimation;
    private Tween selectedMoveAnimation;
    private Tween moveAnimation;

    private const int FRONT = 1;
    private const int BACK = 0;

    public void Init(Sprite sprite, int x, int y, float posX, float posY)
    {
        _bgSprite.sprite = sprite;
        _boxCollider.size = _bgSprite.bounds.size;
        _bgSprite.transform.localScale = Vector3.one * _imgScale;
        _borderSprite.size = _bgSprite.bounds.size;

        Position = new Vector2Int(x, y);
        transform.localPosition = new Vector3(posX, posY, 0);
        transform.localScale = Vector3.zero;
        float delay = (x + y) * _startScaleDelay;
        startAnimation = transform.DOScale(1f, _startScaleTime);
        startAnimation.SetEase(Ease.OutExpo);
        startAnimation.SetDelay(0.5f + delay);
        startAnimation.Play();
    }

    public void GameFinished()
    {
        transform.localScale = Vector3.one;
        spriteAnimation = _bgSprite.transform.DOScale(1f, _startScaleTime);
        spriteAnimation.SetEase(Ease.OutSine);
        spriteAnimation.Play();

        float delay = (Position.x + Position.y) * _startScaleDelay;
        startAnimation = transform.DOScale(0.8f, _startScaleTime);
        startAnimation.SetLoops(2, LoopType.Yoyo);
        startAnimation.SetEase(Ease.InOutExpo);
        startAnimation.SetDelay(0.5f + delay);
        startAnimation.Play();
    }

    public void AnimateStartPosition()
    {
        CompleteAllAnimations();
        startMoveAnimation = transform.DOLocalMove(
            new Vector3(TransPosition.x, TransPosition.y, 0), _startMoveAnimationTime);
        startMoveAnimation.SetEase(Ease.InSine);
        startMoveAnimation.Play();
    }

    public void SelectedMoveStart()
    {
        //Debug.Log("SelectedMoveStart");
        IsMoving = true;
        CompleteAllAnimations();
        _bgSprite.sortingOrder = FRONT;
        startMoveAnimation = transform.DOScale(1.1f, 0.2f);
        startMoveAnimation.SetEase(Ease.OutSine);
        startMoveAnimation.Play();
    }

    public void SelectedMove(Vector2 offset)
    {
        IsMoving = true;
        transform.localPosition = TransPosition + (Vector3)offset;
        float minX = 0f;
        float maxX = GameManager.Cols * SpriteHeight - SpriteHeight;
        float minY = 0f;
        float maxY = GameManager.Rows * SpriteWidth - SpriteWidth;
        Vector2 pos = transform.localPosition;
        if (pos.x < minX)
        {
            pos.x = minX;
        }
        if (pos.x > maxX)
        {
            pos.x = maxX;
        }
        if (pos.y < minY)
        {
            pos.y = minY;
        }
        if (pos.y > maxY)
        {
            pos.y = maxY;
        }
        transform.localPosition = pos;
    }

    public void SelectedMoveEnd()
    {
        //Debug.Log("SelectedMoveEnd");   
        IsMoving = false;
        CompleteAllAnimations();
        selectedMoveAnimation = transform.DOLocalMove(
            new Vector3(TransPosition.x, TransPosition.y, 0f),
            _selectedMoveAnimationTime
            );
        selectedMoveAnimation.onComplete = () =>
        {
            _bgSprite.sortingOrder = BACK;
            startMoveAnimation = transform.DOScale(1f, 0.2f);
            startMoveAnimation.SetEase(Ease.OutSine);
            startMoveAnimation.Play();
        };
        selectedMoveAnimation.Play();
    }

    public void MoveEnd()
    {
        //Debug.Log("MoveEnd");
        CompleteAllAnimations();
        _bgSprite.sortingOrder = FRONT;
        moveAnimation = transform.DOLocalMove(
            new Vector3(TransPosition.x, TransPosition.y, 0f),
            _moveAnimationTime
            );
        moveAnimation.onComplete = () =>
        {
            _bgSprite.sortingOrder = BACK;
        };
        moveAnimation.Play();
    }

    void CompleteAllAnimations()
    {
        startAnimation?.Complete(true);
        spriteAnimation?.Complete(true);
        startMoveAnimation?.Complete(true);
        selectedMoveAnimation?.Complete(true);
        moveAnimation?.Complete(true);
    }
}
