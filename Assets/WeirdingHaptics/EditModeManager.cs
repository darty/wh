using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModeManager : MonoBehaviour
{
    public GameObject environmentRoot;

    public GameObject hapticRoot;

    public Color inactiveColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);

    public Material inactiveMaterial;

    public bool colorMode = false;

    public bool materialMode = true;

    private Dictionary<Transform, Color> changedTransformColors;

    private Dictionary<Transform, Material> changedTransformMaterials;

    private bool editModeActive = false;

    // Start is called before the first frame update
    void Start()
    {
        // this.EnableEditMode();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (editModeActive)
            {
                DisableEditMode();
            }
            else
            {
                EnableEditMode();
            }
        }
    }

    public void EnableEditMode()
    {
        editModeActive = true;

        if (environmentRoot == null)
        {
            return;
        }

        Transform otherObject = environmentRoot.transform.Find("Other");

        List <Transform> allChildren = new List<Transform>();
        this.GetAllChildren(otherObject, ref allChildren);

        if (materialMode && inactiveMaterial)
        {
            changedTransformMaterials = new Dictionary<Transform, Material>();
            foreach(Transform child in allChildren)
            {
                Renderer renderer = child.gameObject.GetComponent<Renderer>();
                if(renderer) {
                    changedTransformMaterials.Add(child, renderer.material);
                    renderer.material = inactiveMaterial;
                }
                
            }
        }

        if (colorMode)
        {
            changedTransformColors = new Dictionary<Transform, Color>();
            foreach(Transform child in allChildren)
            {
                // Debug.Log(child);
                Renderer renderer = child.gameObject.GetComponent<Renderer>();
                if(renderer) {
                    changedTransformColors.Add(child, renderer.material.color);
                    renderer.material.color = inactiveColor;
                }
                
            }
        }
    }

    public void DisableEditMode()
    {
        editModeActive = false;

        if (changedTransformColors?.Count > 0)
        {
            foreach(KeyValuePair<Transform, Color> pair in changedTransformColors)
            {
                Renderer renderer = pair.Key.gameObject.GetComponent<Renderer>();
                if(renderer) {
                    renderer.material.color = pair.Value;
                }
            }
        }

        if (changedTransformMaterials?.Count > 0)
        {
            foreach(KeyValuePair<Transform, Material> pair in changedTransformMaterials)
            {
                Renderer renderer = pair.Key.gameObject.GetComponent<Renderer>();
                if(renderer) {
                    renderer.material = pair.Value;
                }
            }
        }
    }


    private void GetAllChildren(Transform parent, ref List <Transform> transforms)
    {
        foreach (Transform t in parent) {
            transforms.Add(t);
            GetAllChildren(t, ref transforms);
        }
    }
}
