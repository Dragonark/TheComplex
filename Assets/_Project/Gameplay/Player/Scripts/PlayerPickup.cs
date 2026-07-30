using UnityEngine;
using UnityEngine.UI;

public class PlayerPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera playerCamera;

    [Header("UI")]
    [SerializeField] private Image throwChargeImage;

    [Header("Throw Settings")]
    [SerializeField] private float tapThreshold = 0.3f;
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float maxChargeTime = 1.5f;

    public PickupObject HeldObject { get; private set; }
    public bool IsHoldingObject => HeldObject != null;

    private bool isChargingThrow;
    private float throwChargeTimer;

    private void Start()
    {
        if (throwChargeImage != null)
        {
            throwChargeImage.fillAmount = 0f;
            throwChargeImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleThrowInput();
    }

    public bool TryPickup(PickupObject pickupObject)
    {
        if (IsHoldingObject)
            return false;

        HeldObject = pickupObject;
        pickupObject.OnPickedUp(holdPoint);

        return true;
    }

    public void Drop()
    {
        if (HeldObject == null)
            return;

        HeldObject.OnDropped();
        HeldObject = null;
    }

    public void Throw()
    {
        if (HeldObject == null)
            return;

        PickupObject thrownObject = HeldObject;

        // Release the object
        thrownObject.OnDropped();

        Rigidbody rb = thrownObject.Rigidbody;

        // Calculate throw force
        float chargePercent = Mathf.InverseLerp(
            tapThreshold,
            maxChargeTime,
            throwChargeTimer);

        float throwForce = Mathf.Lerp(
            minThrowForce,
            maxThrowForce,
            chargePercent);

        // Throw in camera direction
        rb.AddForce(
            playerCamera.transform.forward * throwForce,
            ForceMode.Impulse);

        HeldObject = null;
    }

    private void HandleThrowInput()
    {
        if (!IsHoldingObject)
            return;

        // Start charging
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isChargingThrow = true;
            throwChargeTimer = 0f;

            if (throwChargeImage != null)
            {
                throwChargeImage.fillAmount = 0f;
                throwChargeImage.gameObject.SetActive(true);
            }
        }

        // Charge
        if (isChargingThrow && Input.GetKey(KeyCode.Q))
        {
            throwChargeTimer += Time.deltaTime;
            throwChargeTimer = Mathf.Min(throwChargeTimer, maxChargeTime);

            if (throwChargeImage != null)
            {
                throwChargeImage.fillAmount = throwChargeTimer / maxChargeTime;
            }
        }

        // Release
        if (isChargingThrow && Input.GetKeyUp(KeyCode.Q))
        {
            isChargingThrow = false;

            if (throwChargeImage != null)
            {
                throwChargeImage.fillAmount = 0f;
                throwChargeImage.gameObject.SetActive(false);
            }

            if (throwChargeTimer < tapThreshold)
            {
                Drop();
            }
            else
            {
                Throw();
            }
        }
    }
}