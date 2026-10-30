using UnityEngine;
using System.Collections.Generic;

public class OutlineManager : MonoBehaviour
{
    [Header("Material do Outline")]
    [SerializeField] private Material outlineMaterial;

    private readonly List<OutlineObject> outlineObjects =
        new List<OutlineObject>();

    private bool outlineActive = false;

    private void Start()
    {
        CreateOutlines();
    }

    private void CreateOutlines()
    {
        Renderer[] renderers =
            FindObjectsByType<Renderer>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (Renderer renderer in renderers)
        {
            // Ignora elementos da UI
            if (renderer is UnityEngine.UI.Graphic)
                continue;

            // Ignora o próprio objeto do OutlineManager
            if (renderer.gameObject == gameObject)
                continue;

            // Somente objetos com MeshFilter nesta primeira versão
            if (renderer.GetComponent<MeshFilter>() == null)
                continue;

            // Evita duplicação
            if (renderer.GetComponent<OutlineObject>() != null)
                continue;

            OutlineObject outline =
                renderer.gameObject.AddComponent<OutlineObject>();

            outline.Setup(outlineMaterial);

            outlineObjects.Add(outline);
        }

        SetOutlineActive(false);
    }

    public void SetOutlineActive(bool active)
    {
        outlineActive = active;

        foreach (OutlineObject outline in outlineObjects)
        {
            if (outline != null)
                outline.SetOutlineActive(active);
        }
    }

    public bool IsOutlineActive()
    {
        return outlineActive;
    }
}