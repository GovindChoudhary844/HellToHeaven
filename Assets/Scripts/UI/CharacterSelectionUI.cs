using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime script attached to the Character Selection Panel.
/// Handles wiring the UI buttons to the CharacterManager and destroying the panel upon selection.
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    public CharacterManager manager;
    public Button btnKael;
    public Button btnElara;

    private void Start()
    {
        if (manager == null) manager = Object.FindFirstObjectByType<CharacterManager>();

        if (btnKael != null)
        {
            btnKael.onClick.AddListener(() => {
                manager.SelectCharacter(true);
                Destroy(gameObject); // Destroy the selection UI panel
            });
        }

        if (btnElara != null)
        {
            btnElara.onClick.AddListener(() => {
                manager.SelectCharacter(false);
                Destroy(gameObject); // Destroy the selection UI panel
            });
        }
    }
}
