using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private PlayerReferences references;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private InteractionUI interactionUI;

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        Ray ray = new Ray(
            references.PlayerCamera.transform.position,
            references.PlayerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green);

            interactionUI.Show("Press [E]");

            if (playerInput.InteractPressed)
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    interactable.Interact();
                }

                playerInput.ConsumeInteract();
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

            interactionUI.Hide();
        }
    }
}