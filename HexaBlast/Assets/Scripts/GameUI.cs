using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public void ReStartGame()
    {
        GameManager.Instance.ResetBoard();
    }
}
