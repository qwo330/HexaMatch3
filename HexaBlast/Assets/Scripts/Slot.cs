using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum EDirect
//{
//    Up,
//    UpRight,
//    DownRight,
//    Down,
//    DownLeft,
//    UpLeft
//}

public class Slot : MonoBehaviour
{
    public bool IsGenerateSlot; // 블럭이 생성되는 슬롯

    public int X { get; private set; }
    public int Y { get; private set; }

    public Block MyBlock;

    public void SetSlot(int x, int y)
    {
        X = x;
        Y = y;
        GameManager.Instance.Slots[X][Y] = this;
    }

    public bool HasBlock()
    {
        return MyBlock != null;
    }

    public void DropBlock()
    {
        // 하단의 슬롯에서 상단의 슬롯에게 블럭 받아옴
        if (HasBlock() == false)
        {
            if (IsGenerateSlot)
            {
                var generator = GetComponent<Generator>();
                generator.Generate();
            }
            else
            {
                Slot upSlot = GameManager.Instance.GetSlot(X + GameManager.SearchX[0], Y + GameManager.SearchY[0]);
                Slot rightUpSlot = GameManager.Instance.GetSlot(X + GameManager.SearchX[1], Y + GameManager.SearchY[1]);
                Slot leftUpSlot = GameManager.Instance.GetSlot(X + GameManager.SearchX[5], Y + GameManager.SearchY[5]);
                Slot privSlot = null;

                if (upSlot && upSlot.HasBlock())
                {
                    // 상단 드랍 (1, 0)
                    privSlot = upSlot;
                }
                else
                {
                    // 좌상단, 우상단 드랍 (1, -1) or (0, 1)
                    int rand = Random.Range(0, 2);

                    if (rightUpSlot != null && rightUpSlot.HasBlock() == false)
                    {
                        rand += 1;
                    }

                    if (leftUpSlot != null && leftUpSlot.HasBlock() == false)
                    {
                        rand -= 1;
                    }

                    privSlot = rand > 0 ? rightUpSlot : leftUpSlot;
                    if (privSlot == null)
                        privSlot = rand > 0 ? leftUpSlot : rightUpSlot;
                }

                if (privSlot && privSlot.MyBlock != null)
                {
                    MyBlock = privSlot.MyBlock;
                    privSlot.MyBlock = null;

                    MyBlock.SetBlock(this);
                }
            }
        }
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", X, Y);
    }
}