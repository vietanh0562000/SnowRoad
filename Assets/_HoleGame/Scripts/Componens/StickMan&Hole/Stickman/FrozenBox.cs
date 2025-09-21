using UnityEngine;
using TMPro;

public class FrozenBox : MonoBehaviour
{
    [SerializeField] private GameObject  iceVisual; // Visual representation of full ice
    [SerializeField] private GameObject  halfIceVisual; // Visual representation of half-melted ice
    [SerializeField] private TextMeshPro textDisplay; // Text to display above the ice

    [SerializeField] public ParticleSystem  fx;
    
    private int frozenValue; // Current frozen value
    private int maxFrozenValue; // Maximum frozen value to calculate melt percentage

    /// <summary>
    /// Initialize frozen state and visuals.
    /// </summary>
    /// <param name="intFrozen">The maximum frozen value (initial state).</param>
    public void SetFrozen(int intFrozen)
    {
        if (intFrozen <= 0)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        frozenValue    = intFrozen;
        maxFrozenValue = intFrozen;

        UpdateVisuals();
    }

    /// <summary>
    /// Decrease the frozen value and update the state.
    /// </summary>
    public void Melting()
    {
        frozenValue = Mathf.Max(0, frozenValue - 1);
        UpdateVisuals();
    }

    private bool isHalfMelt;
    
    /// <summary>
    /// Handles visuals, scaling, and text display based on the current frozen value.
    /// </summary>
    private void UpdateVisuals()
    {
        float meltPercentage = (float)frozenValue / maxFrozenValue;

        // Update the displayed text
        UpdateTextDisplay(frozenValue);

        if (meltPercentage > 0.5f)
        {
            // More than 50% frozen: show the full ice
            iceVisual?.SetActive(true);
            halfIceVisual?.SetActive(false);
            
            ScaleObject(halfIceVisual, meltPercentage);
        }
        else if (meltPercentage > 0.0f)
        {
            if (!isHalfMelt)
            {
                AudioController.PlaySound(SoundKind.FrozenMelting);
                fx.Play();
                isHalfMelt = true;
            }
            
            // Between 0% and 50% frozen: show half-melted ice and bomb object
            iceVisual?.SetActive(false);
            halfIceVisual?.SetActive(true);

            ScaleObject(halfIceVisual, meltPercentage * 2); // Adjust scale for halfIceVisual
        }
        else
        {
            AudioController.PlaySound(SoundKind.FrozenMelting);
            fx.Play();
            // Fully melted: hide all visuals
            iceVisual?.SetActive(false);
            halfIceVisual?.SetActive(false);

            Disappear(); // Optional cleanup if needed
        }
    }

    /// <summary>
    /// Updates the text to reflect the current frozen value.
    /// </summary>
    /// <param name="currentValue">The current frozen value.</param>
    private void UpdateTextDisplay(int currentValue)
    {
        if (textDisplay != null)
        {
            textDisplay.text = currentValue > 0 ? currentValue.ToString() : string.Empty;
        }
    }

    /// <summary>
    /// Scales the target object based on the given percentage.
    /// </summary>
    /// <param name="target">The object to scale.</param>
    /// <param name="percentage">The percentage of the object's initial size.</param>
    private void ScaleObject(GameObject target, float percentage)
    {
        var min = Mathf.Max(0.8f, percentage);

        if (target != null)
        {
            target.transform.localScale = Vector3.one * min;
        }
    }

    /// <summary>
    /// Called when the ice melts completely.
    /// </summary>
    private void Disappear()
    {
        textDisplay?.SetText(string.Empty); // Clear text on full melt
    }
}