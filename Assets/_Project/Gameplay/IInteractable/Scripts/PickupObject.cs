using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupObject : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private HandRequirement handRequirement = HandRequirement.OneHanded;

    private Rigidbody rb;

    private bool isHeld;

    public Rigidbody Rigidbody => rb;

    public HandRequirement HandRequirement => handRequirement;
    [Header("Hold Settings")]
    [SerializeField] private Transform gripPoint;

    public Transform GripPoint => gripPoint;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact()
    {
        Debug.Log("Hammer Interacted");

        PlayerPickup pickup = FindFirstObjectByType<PlayerPickup>();

        if (pickup == null)
        {
            Debug.LogError("PlayerPickup not found!");
            return;
        }

        pickup.TryPickup(this);
    }

    public void OnPickedUp(Transform hand)
    {
        isHeld = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;

        // Calculate how far the root is from the grip point
        Vector3 rootToGrip = transform.position - gripPoint.position;

        // Rotate the object so the grip matches the hand
        Quaternion deltaRotation =
            hand.rotation * Quaternion.Inverse(gripPoint.rotation);

        transform.rotation = deltaRotation * transform.rotation;

        // Move the root so the grip sits exactly on the hand
        transform.position = hand.position + deltaRotation * rootToGrip;

        transform.SetParent(hand, true);
    }
    public void OnDropped()
    {
        isHeld = false;

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
    }

    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = visible;
        }
    }
}
