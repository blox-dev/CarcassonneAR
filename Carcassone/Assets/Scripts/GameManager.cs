using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LibCarcassonne.GameComponents;
using LibCarcassonne.GameStructures;
using LibCarcassonne.GameLogic;

public class GameManager : MonoBehaviour
{
    public GameObject TileRoot;
    public GameObject TilePrefab;
    public GameObject SelectorTilePrefab;
    public GameObject MeeplePrefab;
    public GameObject MeeplePlacePrefab;
    public Image NextTile;

    // TileDeck
    private System.Random rand = new System.Random();
    private uint deckIndex = 0;
    private int[] tileDeck;
    private int currentTile;

    // TilePositions
    private List<(int, int)> filledPositions = new List<(int, int)>();

    // CoreLogic
    //ComponentManager componentManager = new ComponentManager();
    //List<TileComponent> tileComponents;

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
        //tileComponents = componentManager.ParseJson("Assets/Scripts/LibCarcassonne/tiles_map.json");
        //if (tileComponents.Count != 72)
        //{
        //    throw new Exception("Incorrect number of tiles");
        //}

        tileDeck = Enumerable.Range(0, 71).OrderBy(c => rand.Next()).ToArray();

        AddTile((0, 0), getNextTile());
        currentTile = getNextTile();
        SetNextTile(currentTile);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var pos = hitInfo.collider.GetComponent<Transform>().localPosition;
                if (hitInfo.collider.name.StartsWith("SelectorTile"))
                {
                    AddTile(((int)pos.x, (int)pos.z), currentTile);
                    currentTile = getNextTile();
                    SetNextTile(currentTile);
                }
                else if (hitInfo.collider.name.StartsWith("MeeplePlace"))
                {
                    AddMeeple(hitInfo.collider.gameObject);
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
        var tileClone = tile >= 0 ? Instantiate(TilePrefab, TileRoot.transform) : Instantiate(SelectorTilePrefab, TileRoot.transform);
        tileClone.transform.localPosition = relativePosition;
        if (tile < 0)
        {
            return;
        }
        string spriteName = "tile" + (tile + 1).ToString();
        var sprite = Resources.Load<Sprite>("Images/Tiles/" + spriteName);
        if (sprite)
        {
            tileClone.name = spriteName;
            var rend = tileClone.transform.GetChild(0).GetComponent<SpriteRenderer>();
            rend.sprite = sprite;
            //for (var i = 0; i < tileComponents[tile].Types.Count; i++)
            //{
            //    var feature = tileComponents[tile].Types[i];
            //    var featureClone = Instantiate(MeeplePlacePrefab, tileClone.transform);
            //    featureClone.transform.localPosition = new Vector3(feature.Center[0]*0.5f, featureClone.transform.localPosition.y, feature.Center[1]*0.5f);
            //}
        }
        else
        {
            Debug.Log("Failed to load" + "Images/Tiles/" + spriteName); //todo: a serializable field with all sprites added would remove loading
        }
    }

    public void CreateMeeple(MeepleColor meepleColor, Transform parent, Vector3 relativePosition)
    {
        const float meepleHeight = 0.115f; //should not be a random constant
        var materialName = meepleColor.ToString() + "_meeple";
        var material = Resources.Load<Material>("Materials/" + materialName);
        if (material)
        {
            var newMeeple = Instantiate(MeeplePrefab, parent);
            var rend = newMeeple.transform.GetChild(0).GetComponent<Renderer>();
            rend.material = material;
            newMeeple.transform.localPosition = new Vector3(relativePosition.x, relativePosition.y + meepleHeight, relativePosition.z);
        }
        else
        {
            Debug.Log("Failed to load" + "Materials/" + materialName);
        }
    }

    void SetNextTile(int tile)
    {
        string spriteName = "tile" + (tile + 1).ToString();
        var sprite = Resources.Load<Sprite>("Images/Tiles/" + spriteName);
        if (sprite)
        {
            NextTile.sprite = sprite;
        }
    }

    // Tile placement
    void AddTile((int, int) tuple, int tileID)
    {
        for (int i = 0; i < TileRoot.transform.childCount; i++)
        {
            var child = TileRoot.transform.GetChild(i);
            var pos = child.transform.localPosition;
            if ((int)pos.x == tuple.Item1 && (int)pos.z == tuple.Item2)
            {
                Destroy(child.gameObject);
                break;
            }
        }
        CreateTile(tileID, new Vector3(tuple.Item1, 0, tuple.Item2));
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

    void AddMeeple(GameObject place)
    {
        Vector3 o = place.transform.localPosition;
        CreateMeeple(MeepleColor.red, place.transform.parent.transform, new Vector3(o.x - 0.22f, o.y, o.z - 0.22f));
        Destroy(place);
    }
}
