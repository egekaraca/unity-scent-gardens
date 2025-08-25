using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils; // XROrigin

public class SeatInteraction : MonoBehaviour
{
    [Header("Refs")]
    public XROrigin xrOrigin;      // XR Origin (root)
    public Transform headTarget;   // Oturunca göz hizası hedefi
    public XRNode standUpInput = XRNode.RightHand; // grip ile kalk

    [Header("While Seated")]
    public bool disableLocomotionWhileSeated = true;   // otururken move/turn/teleport kapat
    public bool zeroCameraYOffsetWhileSeated = true;   // otururken CameraYOffset=0
    public bool standUpOnMoveInput = true;             // joystick'e basınca kalk
    [Range(0.05f, 0.6f)] public float moveInputThreshold = 0.2f;
    public bool standUpOnRigDrift = true;              // kamera headTarget'tan fazla kayarsa kalk
    [Range(0.1f, 1.0f)] public float driftRadius = 0.35f; // metre

    private CharacterController cc;
    private CharacterControllerDriver ccDriver; // varsa
    private ContinuousMoveProviderBase moveProvider;
    private ContinuousTurnProviderBase turnProvider;
    private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportProvider;

    private bool isSeated;
    private float savedCameraYOffset = 0f;
    private bool savedCCDriverEnabled = false;
    private Vector3 sitAnchorCamPos; // oturulduğu andaki kamera konumu (drift kontrolü için)

    void Awake()
    {
        if (!xrOrigin) xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin)
        {
            cc = xrOrigin.GetComponent<CharacterController>();
            ccDriver = xrOrigin.GetComponent<CharacterControllerDriver>();
            moveProvider = xrOrigin.GetComponentInChildren<ContinuousMoveProviderBase>(true);
            turnProvider = xrOrigin.GetComponentInChildren<ContinuousTurnProviderBase>(true);
            teleportProvider = xrOrigin.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>(true);
        }
    }

    void Update()
    {
        if (!isSeated) return;

        // 1) Grip ile kalk
        var dev = InputDevices.GetDeviceAtXRNode(standUpInput);
        if (dev.TryGetFeatureValue(CommonUsages.gripButton, out bool grip) && grip)
        {
            StandUp();
            return;
        }

        // 2) Joystick ile kalk (hareket girişinde)
        if (standUpOnMoveInput && MoveInputPressed())
        {
            StandUp();
            return;
        }

        // 3) Oturulan noktadan fazla kaydıysa kalk
        if (standUpOnRigDrift && xrOrigin && xrOrigin.Camera)
        {
            Vector3 cam = xrOrigin.Camera.transform.position;
            float planarDist = Vector2.Distance(new Vector2(cam.x, cam.z), new Vector2(headTarget.position.x, headTarget.position.z));
            if (planarDist > driftRadius)
            {
                StandUp();
                return;
            }
        }
    }

    // XR Simple Interactable -> Select Entered eventine bağla
    public void OnSeatSelected(SelectEnterEventArgs _)
    {
        if (!isSeated) SitDown();
    }

    private void SitDown()
    {
        if (!xrOrigin || !xrOrigin.Camera || !headTarget) return;

        // CC ve sürücüyü kapat: yüksekliği/çarpışmayı etkilemesin
        if (cc) cc.enabled = false;
        if (ccDriver)
        {
            savedCCDriverEnabled = ccDriver.enabled;
            ccDriver.enabled = false;
        }

        // Kamera Y offset'ini sıfırla (boydan bağımsız oturuş)
        if (zeroCameraYOffsetWhileSeated)
        {
            savedCameraYOffset = xrOrigin.CameraYOffset;
            xrOrigin.CameraYOffset = 0f;
        }

        // Yalnızca yatay yönü koltuğa çevir
        Vector3 f = headTarget.forward; f.y = 0f; f.Normalize();
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, f);

        // Kamerayı tam headTarget konumuna getir
        xrOrigin.MoveCameraToWorldLocation(headTarget.position);

        // Otururken locomotion kapat (isteğe bağlı)
        if (disableLocomotionWhileSeated)
        {
            if (moveProvider) moveProvider.enabled = false;
            if (turnProvider) turnProvider.enabled = false;
            if (teleportProvider) teleportProvider.enabled = false;
        }

        // Drift kontrolü için kamera konumunu kaydet
        sitAnchorCamPos = xrOrigin.Camera.transform.position;

        isSeated = true;
    }

    private void StandUp()
    {
        // Kamera Y offset'ini geri yükle
        if (zeroCameraYOffsetWhileSeated)
            xrOrigin.CameraYOffset = savedCameraYOffset;

        // CC/driver aç
        if (cc) cc.enabled = true;
        if (ccDriver) ccDriver.enabled = savedCCDriverEnabled;

        // Locomotion sağlayıcıları aç
        if (disableLocomotionWhileSeated)
        {
            if (moveProvider) moveProvider.enabled = true;
            if (turnProvider) turnProvider.enabled = true;
            if (teleportProvider) teleportProvider.enabled = true;
        }

        isSeated = false;
    }

    // Sol/sağ el joystick (primary2DAxis) girişinin büyüklüğünü kontrol eder
    private bool MoveInputPressed()
    {
        Vector2 l = Vector2.zero, r = Vector2.zero;

        var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (left.isValid) left.TryGetFeatureValue(CommonUsages.primary2DAxis, out l);

        var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (right.isValid) right.TryGetFeatureValue(CommonUsages.primary2DAxis, out r);

        float mag = Mathf.Max(l.magnitude, r.magnitude);
        return mag > moveInputThreshold;
    }
}
