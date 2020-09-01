using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public GameObject GeneratedObject;

    [SerializeField]
    Vector3 offset = new Vector3(0f, 4f, 0f);

    Vector3 pos;
    Slot slot;
    int x, y;

    void Start()
    {
        x = GameManager.Instance.Width - 1;
        y = GameManager.Instance.Height - 1;
        pos = transform.position;
        slot = GetComponent<Slot>();
        GeneratedObject = Resources.Load("Prefabs/Block") as GameObject;
    }

    public void Generate()
    {
        Block b = Instantiate(GeneratedObject, transform).GetComponent<Block>();
        b.CreateBlock(slot);
    }
}
