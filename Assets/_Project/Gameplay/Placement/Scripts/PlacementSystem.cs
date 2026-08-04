using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerPickup playerPickup;
    [SerializeField] private Camera playerCamera;

    [Header("Placement")]
    [SerializeField] private float placementDistance = 5f;
    [SerializeField] private float placementOffset = 0.02f;
    [SerializeField] private LayerMask placementMask = ~0;

    [Header("Ghost Preview")]
    [SerializeField] private Color validPlacementColor = new Color(0.35f, 1f, 0.6f, 0.45f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0.35f, 0.35f, 0.45f);

    private PickupObject currentObject;
    private GameObject previewObject;
    private bool canPlaceObject;
    private Vector3 lastValidPreviewPosition;
    private Quaternion lastValidPreviewRotation;

    public bool IsPlacing { get; private set; }

    private void Update()
    {
        if (!IsPlacing)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartPlacement();
            }

            return;
        }

        UpdatePlacement();

        if (canPlaceObject && Input.GetMouseButtonDown(0))
        {
            ConfirmPlacement();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    private void StartPlacement()
    {
        if (playerPickup == null || !playerPickup.IsHoldingObject)
            return;

        currentObject = playerPickup.HeldObject;

        if (currentObject == null || currentObject.ModelRoot == null)
            return;

        previewObject = Instantiate(
            currentObject.ModelRoot.gameObject,
            currentObject.ModelRoot.position,
            currentObject.ModelRoot.rotation);

        previewObject.name = currentObject.name + "_GhostPreview";
        SetPreviewToIgnoreRaycast(previewObject);
        DisablePreviewPhysics(previewObject);

        foreach (Renderer renderer in previewObject.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
            CloneMaterialForPreview(renderer);
        }

        lastValidPreviewPosition = previewObject.transform.position;
        lastValidPreviewRotation = previewObject.transform.rotation;

        currentObject.SetVisible(false);
        canPlaceObject = false;
        IsPlacing = true;
        UpdatePlacement();
    }

    private void UpdatePlacement()
    {
        if (previewObject == null)
            return;

        Ray ray = playerCamera != null
            ? playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f))
            : new Ray(transform.position, transform.forward);

        int raycastMask = placementMask;
        int previewLayer = previewObject.layer;
        if (previewLayer >= 0)
            raycastMask &= ~(1 << previewLayer);

        if (Physics.Raycast(ray, out RaycastHit hit, placementDistance, raycastMask))
        {
            if (hit.collider != null && hit.collider.transform.IsChildOf(previewObject.transform))
            {
                previewObject.transform.position = lastValidPreviewPosition;
                previewObject.transform.rotation = lastValidPreviewRotation;
                canPlaceObject = false;
                SetPreviewColor(invalidPlacementColor);
                return;
            }

            previewObject.transform.position = hit.point + hit.normal * placementOffset;
            previewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            lastValidPreviewPosition = previewObject.transform.position;
            lastValidPreviewRotation = previewObject.transform.rotation;
            canPlaceObject = true;
            SetPreviewColor(validPlacementColor);
        }
        else
        {
            previewObject.transform.position = lastValidPreviewPosition;
            previewObject.transform.rotation = lastValidPreviewRotation;
            canPlaceObject = false;
            SetPreviewColor(invalidPlacementColor);
        }
    }

    private void ConfirmPlacement()
    {
        if (currentObject == null || previewObject == null)
            return;

        currentObject.transform.position = previewObject.transform.position;
        currentObject.transform.rotation = previewObject.transform.rotation;
        currentObject.SetVisible(true);

        Destroy(previewObject);
        previewObject = null;

        playerPickup.Drop();
        currentObject = null;
        IsPlacing = false;
        canPlaceObject = false;
    }

    private void CancelPlacement()
    {
        if (currentObject != null)
        {
            currentObject.SetVisible(true);
        }

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        currentObject = null;
        IsPlacing = false;
        canPlaceObject = false;
    }

    private void SetPreviewColor(Color previewColor)
    {
        if (previewObject == null)
            return;

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];

                if (material == null)
                    continue;

                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                Color newColor = material.color;
                newColor.r = previewColor.r;
                newColor.g = previewColor.g;
                newColor.b = previewColor.b;
                newColor.a = previewColor.a;

                material.color = newColor;
            }
        }
    }

    private void CloneMaterialForPreview(Renderer renderer)
    {
        if (renderer == null)
            return;

        Material[] materials = renderer.sharedMaterials;
        Material[] cloneMaterials = new Material[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            Material source = materials[i];

            if (source == null)
            {
                cloneMaterials[i] = null;
                continue;
            }

            Material clone = new Material(source);
            clone.name = source.name + "_Ghost";
            cloneMaterials[i] = clone;
            MakeMaterialTransparent(clone);
        }

        renderer.materials = cloneMaterials;
    }

    private void MakeMaterialTransparent(Material material)
    {
        if (material == null)
            return;

        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color color = material.color;
        color.a = 0.5f;
        material.color = color;
    }

    private void SetPreviewToIgnoreRaycast(GameObject target)
    {
        if (target == null)
            return;

        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer == -1)
            ignoreLayer = 2;

        target.layer = ignoreLayer;

        foreach (Transform child in target.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = ignoreLayer;
        }
    }

    private void DisablePreviewPhysics(GameObject target)
    {
        if (target == null)
            return;

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        Rigidbody[] rigidbodies = target.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = false;
            rigidbody.useGravity = false;
        }
    }
}