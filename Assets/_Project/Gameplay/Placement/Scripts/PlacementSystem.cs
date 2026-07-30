using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerPickup playerPickup;

    public bool IsPlacing { get; private set; }
    
    private PickupObject currentObject;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryEnterPlacementMode();
        }
    }

    private void TryEnterPlacementMode()
    {
        currentObject = playerPickup.HeldObject;

        currentObject.SetVisible(false);

        IsPlacing = true;
        if (IsPlacing)
            return;

        if (!playerPickup.IsHoldingObject)
            return;

        PickupObject objectToPlace = playerPickup.HeldObject;

        IsPlacing = true;

        Debug.Log("Entered Placement Mode with: " + objectToPlace.name);

    }

    public void ExitPlacementMode()
    {
        IsPlacing = false;

        Debug.Log("Exited Placement Mode");
    }
}