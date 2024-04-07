using System.Linq;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BetterLobby;


internal sealed class PauseMenu : MonoBehaviour
{
    private static GameObject UIMenu
        => Resources.FindObjectsOfTypeAll<Transform>()
            .FirstOrDefault(obj => obj.name == "EscapeMenu")?
            .Find("MainPage")?.gameObject;

    private static GameObject UIButtonList
        => UIMenu?.transform.Find("LIST")?.gameObject;

    internal static bool AddButton(string name, string text, UnityAction action)
    {
        var uiButtonList = UIButtonList;
        if (uiButtonList == null)
        {
            Plugin.CurLogger?.LogError("Failed to find the EscapeMenu control list!");
            return false;
        }

        GameObject uiBaseControl = uiButtonList.transform.Find("RESUME")?.gameObject;
        if (uiBaseControl == null)
        {
            Plugin.CurLogger?.LogError("Failed to find the 'RESUME' control!");
            return false;
        }

        GameObject uiControl = Instantiate(uiBaseControl, uiButtonList.transform);
        uiControl.name = name;

        var uiButton = uiControl.GetComponentInChildren<Button>();
        if (uiButton == null)
        {
            Plugin.CurLogger?.LogError("Failed to find the Button component!");
            Destroy(uiControl);
            return false;
        }

        var uiText = uiControl.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (uiText == null)
        {
            Plugin.CurLogger?.LogError("Failed to find the TextMeshProUGUI component!");
            Destroy(uiControl);
            return false;
        }

        uiButton.onClick.AddListener(action);
        uiText.SetCharArray(text.ToCharArray());
        uiText.SetAllDirty();
        return true;
    }
}
