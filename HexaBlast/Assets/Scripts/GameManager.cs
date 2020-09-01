using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EGameState
{
    Wait,  // 기타 대기
    Touch, // 사용자 터치
    Swap,  // 블럭 스왑
    Match, // 블럭 매치
    Drop,  // 블럭 드랍
    Clear,
    Fail,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static int[] SearchX = { 1, 0, -1, -1, 0, 1, 2, 0, -2, -2, 0, 2 };
    public static int[] SearchY = { 1, 1, 0, -1, -1, 0, 2, 2, 0, -2, -2, 0 };
    [SerializeField]
    int boardX = 6, boardY = 6;

    public GameUI UI;

    [HideInInspector]
    public int DirectCount = 6;

    [HideInInspector]
    public int Width, Height;

    public EGameState GameState { get; private set; }

    public float Dist_X, Dist_Y; // 슬롯 오브젝트의 x, y 길이
    public Block[][] Board;
    public Slot[][] Slots;

    public int MatchCount;
    public int Combo;
    public int MoveCount;
    public int Score;

    GameObject slotPrefab, blockPrefab;
    Block selectTarget, swapTarget;
    Camera cam;

    bool isDraging, isFirstSwap, isSwaping, isMatching, hasMatch;


    void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("카메라 없음");
        }

        slotPrefab = Resources.Load("Prefabs/Slot") as GameObject;
        blockPrefab = Resources.Load("Prefabs/Block") as GameObject;

        DirectCount = (int)(SearchX.Length * 0.5);
        UI = GetComponent<GameUI>();

        CreateBoard(boardX, boardY);
    }

    void Update()
    {
        GameLogic();
    }

    #region 게임 시스템
    public void CreateBoard(int width, int height)
    {
        ChangeGameState(EGameState.Wait);

        Width = width;
        Height = height;

        Board = new Block[width][];
        Slots = new Slot[width][];

        for (int i = 0; i < width; i++)
        {
            Board[i] = new Block[height];
            Slots[i] = new Slot[height];
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = Dist_X * (-i + j);
                float y = Dist_Y * (i + j);

                Slot s = Instantiate(slotPrefab).GetComponent<Slot>();
                s.transform.position = new Vector3(x, y);
                s.SetSlot(i, j);

                if (boardX - 1 == i && boardY - 1 == j)
                {
                    s.IsGenerateSlot = true;
                    s.gameObject.AddComponent<Generator>();
                }

                //if (s.GetComponent<Generator>())
                //{
                //    s.IsGenerateSlot = true;
                //}

                Block b = Instantiate(blockPrefab, s.transform).GetComponent<Block>();
                b.CreateBlock(s);
            }
        }

        // 테스트용
        //ChangeGameState(EGameState.Touch);

        //보드 생성 후 블럭 매치 확인
        ChangeGameState(EGameState.Match);
    }

    public void ResetBoard()
    {
        if (CheckDropFinish() && !HasEmptySlot())
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Block b = Board[i][j];
                    Slot s = Slots[i][j];

                    Board[i][j] = null;
                    Slots[i][j] = null;

                    Destroy(b.gameObject);
                    Destroy(s.gameObject);
                }
            }

            CreateBoard(Width, Height);
        }
    }

    public void GameLogic()
    {
        if (GameState == EGameState.Touch)
        {
            Touch();
        }
        else if (GameState == EGameState.Swap)
        {
            Swap();
        }
        else if (GameState == EGameState.Match)
        {
            Match();
        }
        else if (GameState == EGameState.Drop)
        {
            Drop();
        }
        else if (GameState == EGameState.Clear)
        {
            // 승리 처리
        }
        else if (GameState == EGameState.Fail)
        {
            // 패배 처리 // 재시작
        }
        else // (GameState == EGameState.Wait)
        {

        }
    }

    public void ChangeGameState(EGameState nextState)
    {
        GameState = nextState;
        Debug.LogFormat("Change State : " + nextState);
    }

    #endregion

    #region 유저 터치
    public void Touch()
    {
        if (Input.GetMouseButtonDown(0))
            MouseDown();

        else if (Input.GetMouseButton(0))
            MouseDrag();

        else if (Input.GetMouseButtonUp(0))
            isDraging = false;
    }

    void MouseDown()
    {
        RaycastHit2D hit;
        var pos = cam.ScreenToWorldPoint(Input.mousePosition);

        hit = Physics2D.Raycast(pos, transform.forward);

        if (hit && hit.collider.CompareTag("Block"))
        {
            selectTarget = hit.collider.GetComponent<Block>();
            isDraging = true;
        }
    }

    void MouseDrag()
    {
        if (isDraging)
        {
            RaycastHit2D hit;
            var pos = cam.ScreenToWorldPoint(Input.mousePosition);
            hit = Physics2D.Raycast(pos, transform.forward);

            if (hit && hit.collider.CompareTag("Block"))
            {
                swapTarget = hit.collider.GetComponent<Block>();
                if (selectTarget != swapTarget)
                {
                    if (selectTarget.CheckAdjcent(swapTarget))
                    {
                        isFirstSwap = true;
                        ChangeGameState(EGameState.Swap);
                        isDraging = false;
                    }
                }
            }
        }
    }
    #endregion

    #region 블럭 이동
    public void Swap()
    {
        if (isFirstSwap)
        {
            SwapBlock();

            if (!isSwaping)
            {
                CheckMatch();

                if (hasMatch)
                {
                    ChangeGameState(EGameState.Match);
                }
                else
                {
                    isFirstSwap = false;
                }
            }
        }
        else // 스왑하기 전으로 되돌린다
        {
            SwapBlock();

            if (!isSwaping)
            {
                ChangeGameState(EGameState.Touch);
            }
        }
    }

    public void SwapBlock()
    {
        if (!isSwaping)
        {
            hasMatch = false;
            isSwaping = true;

            Slot startSlot = selectTarget.mySlot;
            Slot endSlot = swapTarget.mySlot;

            selectTarget.SetBlock(endSlot);
            swapTarget.SetBlock(startSlot);
        }

        if (!selectTarget.bDrop)
        {
            isSwaping = false;
        }
    }

    #endregion

    #region 매치
    public void Match()
    {
        CheckMatch();
        DestroyBlock();
        CheckClear();
    }

    void CheckMatch()
    {
        MatchCount = 0;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Block b = GetBlock(i, j);
                MatchLine(b);
            }
        }
    }

    /// <summary>
    /// 직선 Match 3 ~ 4
    /// </summary>
    void MatchLine(Block targetBlock)
    {
        EColor eColor = targetBlock.EColor;
        int x = targetBlock.X;
        int y = targetBlock.Y;
        bool isMatch = false;

        // 6방향 매치 체크
        for (int i = 0; i < 6; i++)
        {
            Block searchBlock = GetBlock(x + SearchX[i], y + SearchY[i]);
            if (searchBlock != null && searchBlock.EColor == eColor)
            {
                searchBlock = GetBlock(x + SearchX[i + 3] % 6, y + SearchY[i + 3] % 6);
                if (searchBlock != null && searchBlock.EColor == eColor)
                {
                    isMatch = true;
                }

                searchBlock = GetBlock(x + SearchX[i + 6], y + SearchY[i + 6]);
                if (searchBlock != null && searchBlock.EColor == eColor)
                {
                    isMatch = true;
                }

                if (isMatch)
                {
                    hasMatch = true;
                    targetBlock.bMatch = true;
                    MatchCount++;
                    //Debug.Log("Match " + targetBlock);
                }
            }
        }
    }

    public void DestroyBlock()
    {
        if (!hasMatch) return;
        // todo : 블럭 파괴
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Block b = GetBlock(i, j);
                b.DestoryBlock();
            }
        }

        ChangeGameState(EGameState.Drop);
    }

    void CheckClear()
    {
        if (MatchCount == 0)
        {
            // todo : 게임 클리어 체크

            // 클리어 아니면 다시 Touch로
            ChangeGameState(EGameState.Touch);
        }
    }
    #endregion

    #region Drop
    /// <summary>
    /// 빈 슬롯이 있으면 블럭을 떨어지게 한다.
    /// </summary>
    void Drop()
    {
        // 드랍 중인 블럭이 있다면 대기
        if (!CheckDropFinish())
            return;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Slot s = Slots[i][j];
                s.DropBlock();
            }
        }

        if (CheckDropFinish() && !HasEmptySlot())
            ChangeGameState(EGameState.Match);
    }  
    #endregion

    public Block GetBlock(int x, int y)
    {
        if (CheckInBoard(x, y))
            return Board[x][y];
        else
            return null;
    }

    public Slot GetSlot(int x, int y)
    {
        if (CheckInBoard(x, y))
            return Slots[x][y];
        else
            return null;
    }

    /// <summary>
    /// 이동 중인 블럭이 존재하는지 체크
    /// </summary>
    public bool CheckDropFinish()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Block block = Board[i][j];
                if (block != null && block.bDrop)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 빈 슬롯이 존재하는지 확인
    /// </summary>
    public bool HasEmptySlot()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (!Slots[i][j].HasBlock())
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Block GetBlockFromDirect(Block current, int index)
    {
        Block result = null;
        int X = current.X + SearchX[index];
        int Y = current.Y + SearchY[index];

        if (CheckInBoard(X, Y))
        {
            result = Board[X][Y];
        }

        return result;
    }

    bool CheckInBoard(int x, int y)
    {
        return (0 <= x && x < Width) && (0 <= y && y < Height);
    }
}

/* -> 맵 생성 
 *      while()
 *      {
 *          while()
 *          {
 *              -> 매치
 *              -> 드랍
 *              ->> 매치 더이상 없으면 탈출
 *          }
 *          ->> 게임 클리어 시 탈출
 *          -> 유저 터치 
 *      }
 *      
     */


/*
 필수 구현항목

매칭조건 : 직선 3개 이상, 4개 이상의 블럭이 모일 경우
드랍로직 : 새로 생성된 블럭이 좌우로 흘러내리는 로직
장애물 : 팽이

기타 : UI, 특수블럭 생성, 특수블럭 기능 등 
필수 구현항목 이외의 기능은 자유롭게 추가 구현하시면 됩니다.
     
    YouTube URL : https://www.youtube.com/watch?v=5QUxdj1Fg7w

    ===================

    //보드 생성,
    //블럭 스왑,
    //매칭,
    //블럭 파괴,
    
    //드랍,
    //상단에서 블럭 생성,
    
    
    팽이,
    (맵 세팅 인스펙터 에디터,) -> 맵 모양 수정,
    
    UI,
    
    매치 가능한 블럭이 있는지 체크하는 로직, -> 블럭 리셋

     */
