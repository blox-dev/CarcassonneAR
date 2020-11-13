using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject TileRoot;
    public GameObject MeepleRoot;
    public GameObject TilePrefab;
    public GameObject MeeplePrefab;
    public enum MeepleColor
    {
        black,
        red,
        blue,
        breen,
        yellow
    }

    void Start()
    {
        CreateTile(5);
        CreateMeeple(MeepleColor.black, new Vector3(0, 0, -0.2f));
        CreateMeeple(MeepleColor.red, new Vector3(0.21f, 0, -0.4f));
        CreateMeeple(MeepleColor.blue, new Vector3(-0.2f, 0, -0.6f));
    }

    void Update()
    {

    }

    public void CreateTile(uint tile)
    {
        var clone = Instantiate(TilePrefab);
        string spriteName = "tile" + tile.ToString();
        var sprite = Resources.Load<Sprite>("Images/Tiles/" + spriteName);
        if (sprite)
        {
            var rend = clone.transform.GetChild(0).GetComponent<SpriteRenderer>();
            rend.sprite = sprite;
            Instantiate(clone, TileRoot.GetComponent<Transform>());
        }
        else
        {
            Debug.Log("Failed to load" + "Images/Tiles/" + spriteName);
        }
        Destroy(clone);
    }

    public void CreateMeeple(MeepleColor meepleColor, Vector3 relativePosition)
    {
        float meepleHeight = 0.115f; //should not be a random constant
        var clone = Instantiate(MeeplePrefab);
        var materialName = meepleColor.ToString() + "_meeple";
        var material = Resources.Load<Material>("Materials/" + materialName);
        if (material)
        {
            var rend = clone.GetComponent<Renderer>();
            rend.material = material;
            var newMeeple = Instantiate(clone, MeepleRoot.GetComponent<Transform>());
            newMeeple.transform.localScale = new Vector3(1, 1, 1);
            var newMeeplePosition = newMeeple.transform.position;
            newMeeplePosition = new Vector3(relativePosition.x + newMeeplePosition.x, relativePosition.y + newMeeplePosition.y + meepleHeight, relativePosition.z + newMeeplePosition.z);
            newMeeple.transform.position = newMeeplePosition;
        }
        else
        {
            Debug.Log("Failed to load" + "Materials/" + materialName);
        }
        Destroy(clone);
    }
}
