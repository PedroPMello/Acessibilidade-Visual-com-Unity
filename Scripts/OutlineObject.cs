using UnityEngine;

public class OutlineObject : MonoBehaviour
{
    private GameObject outlineObject;
    private Renderer originalRenderer;

    private Material outlineMaterial;

    public void Setup(Material material)
    {
        originalRenderer = GetComponent<Renderer>();

        if (originalRenderer == null)
            return;

        outlineMaterial = material;

        CreateOutline();
    }

    private void CreateOutline()
    {
        outlineObject = new GameObject(
            gameObject.name + "_Outline"
        );

        outlineObject.transform.SetParent(
            transform,
            false
        );

        // Copia posição, rotação e escala
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one;

        MeshFilter originalMesh =
            GetComponent<MeshFilter>();

        if (originalMesh != null)
        {
            MeshFilter outlineMesh =
                outlineObject.AddComponent<MeshFilter>();

            outlineMesh.sharedMesh =
                originalMesh.sharedMesh;
        }

        MeshRenderer outlineRenderer =
            outlineObject.AddComponent<MeshRenderer>();

        outlineRenderer.sharedMaterial =
            outlineMaterial;

        outlineObject.SetActive(false);
    }

    public void SetOutlineActive(bool active)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(active);
        }
    }

    private void OnDestroy()
    {
        if (outlineObject != null)
        {
            Destroy(outlineObject);
        }
    }
}