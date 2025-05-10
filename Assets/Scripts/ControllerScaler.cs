using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ControllerScaler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference aButtonAction;
    public InputActionReference bButtonAction;

    [Header("XR Setup")]
    public NearFarInteractor interactor; // assign this to the RightHand Direct Interactor

    private ScalableObject currentScalable;

    private void OnEnable()
    {
        aButtonAction.action.Enable();
        bButtonAction.action.Enable();

        aButtonAction.action.started += OnAScaleStarted;
        bButtonAction.action.started += OnBScaleStarted;
    }

    private void OnDisable()
    {
        aButtonAction.action.started -= OnAScaleStarted;
        bButtonAction.action.started -= OnBScaleStarted;

        aButtonAction.action.Disable();
        bButtonAction.action.Disable();
    }

    private void OnAScaleStarted(InputAction.CallbackContext ctx)
    {
        TryScale(1); // scale up
    }

    private void OnBScaleStarted(InputAction.CallbackContext ctx)
    {
        TryScale(-1); // scale down
    }

    private void TryScale(int direction)
    {
        var selected = interactor.GetOldestInteractableSelected();
        if (selected == null) return;

        currentScalable = selected.transform.GetComponent<ScalableObject>();
        if (currentScalable == null) return;

        if (currentScalable.originalScale == Vector3.zero)
            currentScalable.originalScale = currentScalable.transform.localScale;

        float delta = direction * currentScalable.scaleSpeed * Time.deltaTime;
        float factor = 1f + delta;
        Vector3 newScale = currentScalable.transform.localScale * factor;

        float min = currentScalable.originalScale.x * currentScalable.minScaleFactor;
        float max = currentScalable.originalScale.x * currentScalable.maxScaleFactor;
        float clamped = Mathf.Clamp(newScale.x, min, max);

        currentScalable.transform.localScale = new Vector3(clamped, clamped, clamped);
    }


    private void Update()
    {
        // Optional: Hold to scale continuously
        if (aButtonAction.action.IsPressed())
            TryScale(1);
        else if (bButtonAction.action.IsPressed())
            TryScale(-1);
    }
}
