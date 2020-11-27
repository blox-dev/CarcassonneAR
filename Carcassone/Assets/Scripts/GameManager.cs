using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject TileRoot;
    public GameObject MeepleRoot;
    public GameObject TilePrefab;
    public GameObject SelectorTilePrefab;
    public GameObject MeeplePrefab;

    // TileDeck
    private System.Random rand = new System.Random();
    private uint deckIndex = 0;
    private int[] tileDeck;

    // TilePositions
    private List<(int, int)> filledPositions = new List<(int, int)>();

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
        tileDeck = Enumerable.Range(0, 71).OrderBy(c => rand.Next()).ToArray();

        AddTile((0, 0));

        CreateMeeple(MeepleColor.black, new Vector3(0, 0, -0.2f));
        CreateMeeple(MeepleColor.red, new Vector3(0.21f, 0, -0.4f));
        CreateMeeple(MeepleColor.blue, new Vector3(-0.2f, 0, -0.6f));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var pos = hitInfo.collider.GetComponent<Transform>().position;
                if (hitInfo.collider.name.StartsWith("SelectorTile"))
                {
                    AddTile(((int)pos.x, (int)pos.z));
                }
            }
        }
    }

    // Deck handling
    int getNextTile()
    {
        if (deckIndex == 72)
        {
            //throw
            return 0;
        }
        return tileDeck[deckIndex++];
    }

    // Object Creation
    public void CreateTile(int tile, Vector3 relativePosition)
    {
        var clone = tile > 0 ? Instantiate(TilePrefab, TileRoot.transform) : Instantiate(SelectorTilePrefab, TileRoot.transform);
        clone.transform.position = relativePosition;
        if (tile < 0)
        {
            return;
        }
        string spriteName = "tile" + (tile + 1).ToString();
        var sprite = Resources.Load<Sprite>("Images/Tiles/" + spriteName);
        if (sprite)
        {
            clone.name = spriteName;
            var rend = clone.transform.GetChild(0).GetComponent<SpriteRenderer>();
            rend.sprite = sprite;
        }
        else
        {
            Debug.Log("Failed to load" + "Images/Tiles/" + spriteName);
        }
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

    // Tile placement
    void AddTile((int, int) tuple)
    {
        for (int i = 0; i < TileRoot.transform.childCount; i++)
        {
            var child = TileRoot.transform.GetChild(i);
            var pos = child.transform.position;
            if ((int)pos.x == tuple.Item1 && (int)pos.z == tuple.Item2)
            {
                Destroy(child.gameObject);
                break;
            }
        }
        CreateTile(getNextTile(), new Vector3(tuple.Item1, 0, tuple.Item2));
        filledPositions.Add(tuple);
        foreach (var xy in new List<(int, int)> {(0, 1), (-1, 0), (0, -1), (1, 0) })
        {
            var new_t = (tuple.Item1 + xy.Item1, tuple.Item2 + xy.Item2);
            if (!filledPositions.Contains(new_t))
            {
                filledPositions.Add(new_t);
                CreateTile(-1, new Vector3(tuple.Item1 + xy.Item1, 0, tuple.Item2 + xy.Item2));
            }
        }
    }
}
