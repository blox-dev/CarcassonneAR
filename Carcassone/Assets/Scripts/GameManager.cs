using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject Root;
    public GameObject TilePrefab;

    void Start()
    {
        CreateTile(5);
    }

    void Update()
    {
        
    }

    public void CreateTile(uint tile){
        var clone = Instantiate(TilePrefab);
        var spriteName = "tile" + tile.ToString();
        var sprite = Resources.Load<Sprite>("Images/Tiles/" + spriteName);
        if (sprite)
        {
            var rend = clone.transform.GetChild(0).GetComponent<SpriteRenderer>();
            rend.sprite = sprite;
            Instantiate(clone, Root.GetComponent<Transform>());
        }
        else
        {
            Debug.Log("Failed to load" + "Images/Tiles/" + spriteName);
        }
        Destroy(clone);
    }
}
