using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int Rows;
    public static int Cols;

    [SerializeField] private List<Level> _curLevels;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _movesText;
    [SerializeField] private TMP_Text _bestText;
    [SerializeField] private Transform _playButtonTransform;
    [SerializeField] private Transform _gridParent;
    [SerializeField] private Transform _boardRoot;
    [SerializeField] private Transform _nextButtonTransform;
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private float screenScaleFactor;
    [SerializeField] private float screenHeightRatio;
    [SerializeField] private RectTransform transNotiFinish;
    [SerializeField] private GameObject objReplayButton;
    [SerializeField] private AudioClip audioSwap;

    private Level _currentlevelData;
    private int levelNum;
    private int moveNum;
    private int bestNum;

    private bool hasGameStarted;
    private bool hasGameFinished;
    private bool canMove;
    private bool canStartClicking;
    private float clickCooldown = 0.3f; // Cooldown period in seconds
    private float lastClickTime;

    private Tween playStartTween;
    private Tween playNextTween;

    private Cell[,] cells;
    private Cell[,] correctCells;

    private Cell selectedCell;
    private Cell movedCell;
    private Vector2 startPos;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        canMove = false;
        canStartClicking = false;
        hasGameStarted = false;
        lastClickTime = -clickCooldown; // Initialize to allow immediate first click
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        AudioManager.Instance.AddButtonSound();
        transNotiFinish.gameObject.SetActive(false);
        objReplayButton.SetActive(false);

        levelNum = PlayerPrefs.GetInt(Constants.Data.LEVEL, 1);
        LoadLevel(levelNum);
        moveNum = 0;
        bestNum = PlayerPrefs.GetInt(Constants.Data.HIGH_SCORE + levelNum.ToString(), 0);
        _levelText.text = levelNum.ToString();
        _movesText.text = moveNum.ToString();
        _bestText.text = bestNum.ToString();

        DOTween.defaultAutoPlay = AutoPlay.None;

        playStartTween = _playButtonTransform
            .DOScale(_playButtonTransform.localScale * 1.1f, 1f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
        playStartTween.Play();

        SpawnCells();
    }

    private void LoadLevel(int levelNumber)
    {
        // Ensure the level number is within the valid range
        if (levelNumber < 1 || levelNumber > _curLevels.Count)
        {
            Debug.LogError($"Level {levelNumber} is out of range. Total levels: {_curLevels.Count}");
            return;
        }

        // Access the level data by index (levelNumber - 1 because levelNumber is 1-based)
        _currentlevelData = _curLevels[levelNumber - 1];

        Rows = _currentlevelData.Row;
        Cols = _currentlevelData.Col;
    }

    private void SpawnCells()
    {
        cells = new Cell[_currentlevelData.Row, _currentlevelData.Col];
        correctCells = new Cell[_currentlevelData.Row, _currentlevelData.Col];

        Camera.main.backgroundColor = _currentlevelData.BackGroundColor;
        Sprite[,] imgPieces = SplitImageSprite(_currentlevelData);

        // Lấy Sprite đầu tiên làm chuẩn
        Sprite sprite = imgPieces[0, 0];
        float cellWidth = sprite.bounds.size.x;
        float cellHeight = sprite.bounds.size.y;
        float totalWidth = Cols * cellWidth;
        float totalHeight = Rows * cellHeight;
        float startX = -totalWidth / 2 + cellWidth / 2;
        float startY = -totalHeight / 2 + cellHeight / 2;
        _gridParent.transform.localPosition = new Vector3(startX, startY, 0);

        // Calculate the screen width and height in world units
        float screenWidth = Camera.main.orthographicSize * 2 * Screen.width / Screen.height;
        float screenHeight = Camera.main.orthographicSize * 2;
        float scaleFactorWidth = screenWidth / totalWidth;
        float scaleFactorHeight = screenHeight / totalHeight;
        float scaleFactor = Mathf.Min(scaleFactorWidth, scaleFactorHeight * screenHeightRatio);

        // Apply the scale factor to the board root
        _boardRoot.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1) * screenScaleFactor;

        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                float posX = y * cellWidth;
                float posY = x * cellHeight;

                cells[x, y] = Instantiate(_cellPrefab, _gridParent);
                cells[x, y].Init(imgPieces[x, y], y, x, posX, posY);
                cells[x, y].name = $"Cell_{x}x{y}";
                correctCells[x, y] = cells[x, y];
            }
        }
    }

    public Sprite[,] SplitImageSprite(Level level)
    {
        Texture2D texture = level.ImageSprite.texture;
        int Row = level.Row;
        int Col = level.Col;
        int pieceWidth = texture.width / Col;
        int pieceHeight = texture.height / Row;

        Sprite[,] pieces = new Sprite[Row, Col];

        for (int i = 0; i < Row; i++)
        {
            for (int j = 0; j < Col; j++)
            {
                Rect rect = new Rect(j * pieceWidth, i * pieceHeight, pieceWidth, pieceHeight);
                pieces[i, j] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            }
        }

        return pieces;
    }

    public void ClickedPlayButton()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                if (cells[i, j].IsStartTweenPlaying) return;
            }
        }

        playStartTween.Kill();
        playStartTween = null;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                if (_currentlevelData.LockedCells.Contains(new Vector2Int(i, j)))
                {
                    continue;
                }

                int swapX, swapY;
                do
                {
                    swapX = Random.Range(0, Rows);
                    swapY = Random.Range(0, Cols);
                } while (_currentlevelData.LockedCells.Contains(new Vector2Int(swapX, swapY)));
                Cell temp = cells[i, j];
                cells[i, j] = cells[swapX, swapY];
                Vector2Int swappedPostion = cells[swapX, swapY].Position;
                cells[i, j].Position = temp.Position;
                cells[swapX, swapY] = temp;
                temp.Position = swappedPostion;
            }
        }

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                cells[i, j].AnimateStartPosition();
            }
        }

        hasGameStarted = true;
        _playButtonTransform.gameObject.SetActive(false);
        objReplayButton.SetActive(true);
    }

    private void Update()
    {
        if (hasGameFinished) return;

        if (!hasGameStarted) return;

        if (!canStartClicking)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (cells[i, j].IsStartMovePlaying)
                        return;
                }
            }
            canStartClicking = true;
            canMove = true;
        }

        if (!canMove)
        {
            if (!selectedCell.IsAnimating && !movedCell.IsAnimating && !selectedCell.IsMoving)
            {
                selectedCell = null;
                movedCell = null;
                canMove = true;
                CheckWin();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("MouseDown");
            if (Time.time - lastClickTime < clickCooldown) return; // Prevent double-click
            lastClickTime = Time.time;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit && hit.collider.TryGetComponent(out selectedCell))
            {
                if (_currentlevelData.LockedCells.Contains(
                    new Vector2Int(selectedCell.Position.y, selectedCell.Position.x)
                    ))
                {
                    selectedCell = null;
                    return;
                }
                startPos = mousePos2D;
                selectedCell.SelectedMoveStart();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            //Debug.Log("Mouse held");
            if (selectedCell == null) return;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Vector2 offset = mousePos2D - startPos;
            selectedCell.SelectedMove(offset);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Mouse Up");
            if (selectedCell == null) return;

            canMove = false;

            Vector3 localPos = selectedCell.gameObject.transform.localPosition;
            int col = (int)((localPos.x + selectedCell.SpriteWidth / 2) / selectedCell.SpriteWidth);
            int row = (int)((localPos.y + selectedCell.SpriteHeight / 2) / selectedCell.SpriteHeight);

            movedCell = cells[row, col];
            if (_currentlevelData.LockedCells.Contains(new Vector2Int(row, col))
                || movedCell == selectedCell)
            {
                selectedCell.SelectedMoveEnd();
                return;
            }

            Vector2Int tempPos = selectedCell.Position;
            selectedCell.Position = movedCell.Position;
            movedCell.Position = tempPos;

            cells[selectedCell.Position.y, selectedCell.Position.x] = selectedCell;
            cells[movedCell.Position.y, movedCell.Position.x] = movedCell;

            selectedCell.SelectedMoveEnd();
            movedCell.MoveEnd();
            AudioManager.Instance.PlaySound(audioSwap);

            moveNum++;
            _movesText.text = moveNum.ToString();
        }

    }

    private void CheckWin()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                if (cells[i, j] != correctCells[i, j])
                    return;
            }
        }

        hasGameFinished = true;
        if (bestNum == 0 || bestNum > moveNum)
        {
            bestNum = moveNum;
        }
        PlayerPrefs.SetInt(Constants.Data.HIGH_SCORE + levelNum.ToString(), bestNum);
        _bestText.text = bestNum.ToString();
        if (levelNum < _curLevels.Count)
        {
            PlayerPrefs.SetInt(Constants.Data.LEVEL, levelNum + 1);
        }

        _nextButtonTransform.gameObject.SetActive(true);
        playNextTween = _nextButtonTransform
            .DOScale(1.1f, 1f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
        playNextTween.Play();

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                cells[i, j].GameFinished();
            }
        }
    }

    public void ClickedNextButton()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                if (cells[i, j].IsStartTweenPlaying)
                    return;
            }
        }

        playNextTween.Kill();
        if (levelNum >= _curLevels.Count)
        {
            // Show congratulations message
            ShowCongratulationsMessage();
        }
        else
        {
            // Load the next level
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public void ClickedReplayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    Tween notiTween;
    private void ShowCongratulationsMessage()
    {
        notiTween?.Complete();
        transNotiFinish.gameObject.SetActive(true);
        notiTween = transNotiFinish.DOSizeDelta(new Vector2(transNotiFinish.sizeDelta.x, 800f), 0.5f);
        notiTween.SetEase(Ease.OutBack);
        notiTween.Play();
    }

    public void HideCongratulationsMessage()
    {
        notiTween?.Complete();
        notiTween = transNotiFinish.DOSizeDelta(new Vector2(transNotiFinish.sizeDelta.x, 0f), 0.5f);
        notiTween.SetEase(Ease.OutBack);
        notiTween.OnComplete(() => transNotiFinish.gameObject.SetActive(false));
        notiTween.Play();
    }
}
