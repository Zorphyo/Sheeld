using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class ArenaIntroCamera : MonoBehaviour
{
    public static ArenaIntroCamera Instance { get; private set; }

    public CinemachineCamera introVirtualCamera;
    public CinemachineCamera gameplayCamera;
    public float flyDuration = 5f;
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CinemachineSplineCart cart;
    private PlayerController playerController;

    private void Awake()
    {
        Instance = this;
        cart = introVirtualCamera.GetComponentInChildren<CinemachineSplineCart>();
    }

    private void Start()
    {
        // find player input after scene load
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            playerController = p.GetComponent<PlayerController>();
    }

    public IEnumerator PlayIntro()
    {
        // disable player input
        if (playerController != null)
            playerController.enabled = false;

        // prioritize intro cam
        introVirtualCamera.Priority = 20;
        gameplayCamera.Priority = 10;

        // reset cart to start of path
        if (cart != null)
            cart.SplinePosition = 0f;

        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / flyDuration);

            // use curve for ease in/out instead of linear movement
            if (cart != null)
                cart.SplinePosition = speedCurve.Evaluate(t);

            yield return null;
        }

        // ensure cart is exactly at end
        if (cart != null)
            cart.SplinePosition = 1f;

        // small pause at end before handing back control
        yield return new WaitForSeconds(0.5f);

        // hand control back to gameplay cam
        introVirtualCamera.Priority = 0;
        gameplayCamera.Priority = 20;

        // re-enable player input
        if (playerController != null)
            playerController.enabled = true;
    }
}