using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EType
{
    Block,
    Top,
}

public enum EColor
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Purple
}

public class Block : MonoBehaviour
{
    public EColor EColor { get; private set; }
    public EType EType { get; private set; }

    public Slot mySlot;

    public bool bDrop;
    public bool bMatch;

    float velocity = 0;
    float dropSpeed = 10f;

    public Vector3 StartPos, EndPos;
    public int X { get; private set; }
    public int Y { get; private set; }

    SpriteRenderer render;
    Color orangeColor = new Color(1, 0.6f, 0);
    Color purpleColor = new Color(0.8f, 0, 1);

    void Update()
    {
        Drop();
    }

    public void Drop()
    {
        if (bDrop && transform.position != EndPos)
        {
            velocity += Time.deltaTime * dropSpeed;
            transform.position = Vector3.MoveTowards(StartPos, EndPos, velocity);

            if (transform.position == EndPos)
            {
                velocity = 0;
                bDrop = false;
            }
        }
    }

    public virtual void SetBlock(Slot s)
    {
        X = s.X;
        Y = s.Y;
        GameManager.Instance.Board[X][Y] = this;

        transform.SetParent(s.transform);
        mySlot = s;
        s.MyBlock = this;

        StartPos = transform.position;
        EndPos = s.transform.position;
        bDrop = true;

    }

    public void CreateBlock(Slot s)
    {
        EType = EType.Block;
        X = s.X;
        Y = s.Y;
        GameManager.Instance.Board[X][Y] = this;

        s.MyBlock = this;
        mySlot = s;

        render = GetComponent<SpriteRenderer>();
        EColor = (EColor)Random.Range(0, 6);
        render.color = SetColor(EColor);

    }

    Color SetColor(EColor eColor)
    {
        Color result = Color.white;
        switch (eColor)
        {
            case EColor.Red:
                result = Color.red; break;
            case EColor.Orange:
                result = orangeColor; break;
            case EColor.Yellow:
                result = Color.yellow; break;
            case EColor.Green:
                result = Color.green; break;
            case EColor.Blue:
                result = Color.blue; break;
            case EColor.Purple:
                result = purpleColor; break;
        }

        return result;
    }
    
    public void DestoryBlock()
    {
        if (!bMatch) return;

        AfterDestroy();

        mySlot.MyBlock = null;
        mySlot = null;
        GameManager.Instance.Board[X][Y] = null;

        Destroy(gameObject);
    }

    /// <summary>
    /// 블럭이 파괴될 때 추가적인 동작 처리
    /// </summary>
    void AfterDestroy()
    {

    }

    public bool CheckAdjcent(Block target)
    {
        int length = GameManager.Instance.DirectCount;
        for (int i = 0; i < length; i++)
        {
            Block adjBlock = GameManager.Instance.GetBlockFromDirect(this, i);
            if (adjBlock != null && EqualCoord(adjBlock, target))
            {
                return true;
            }
        }

        return false;
    }

    public bool EqualCoord(Block current, Block target)
    {
        return current.X == target.X && current.Y == target.Y;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", X, Y);
    }
}