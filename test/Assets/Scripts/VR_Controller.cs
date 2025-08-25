using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRRi : MonoBehaviour
{
    [Header("VR Components")]
    public Transform headConstraint;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Header("XR References")]
    public XRNode leftHandNode = XRNode.LeftHand;
    public XRNode rightHandNode = XRNode.RightHand;
    public XRNode headNode = XRNode.Head;

    private Vector3 headBodyOffset;

    void Start()
    {
        if (headConstraint == null || leftHandTarget == null || rightHandTarget == null)
        {
            Debug.LogError("Head or Hand targets are not assigned in VRRig script.");
            enabled = false;
            return;
        }

        // Baþýn ilk pozisyonunu referans alarak vücut ile olan farký sakla
        headBodyOffset = transform.position - headConstraint.position;
    }

    void Update()
    {
        UpdateHead();
        UpdateHand(leftHandNode, leftHandTarget);
        UpdateHand(rightHandNode, rightHandTarget);
    }

    void UpdateHead()
    {
        InputDevices.GetDeviceAtXRNode(headNode).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(headNode).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        headConstraint.position = position;
        headConstraint.rotation = rotation;

        // Vücut pozisyonunu kafa pozisyonuna göre güncelle (yüksekliði koruyarak)
        Vector3 bodyPosition = headConstraint.position + headBodyOffset;
        bodyPosition.y = transform.position.y;  // sabit ayak yüksekliði
        transform.position = bodyPosition;
    }

    void UpdateHand(XRNode node, Transform target)
    {
        if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
        {
            target.position = position;
        }

        if (InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
        {
            target.rotation = rotation;
        }
    }
}
