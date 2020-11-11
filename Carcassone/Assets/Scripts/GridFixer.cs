using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFixer : MonoBehaviour
{
    public float GridSize = 0f;

    void LateUpdate()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform child = gameObject.transform.GetChild(i);
            child.position = new Vector3(Mathf.Round(child.transform.position.x), child.transform.position.y, Mathf.Round(child.transform.position.z));
        }   
    }
}
