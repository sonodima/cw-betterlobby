using System.Linq;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using Steamworks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

namespace BetterLobby;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin
{
    public static ManualLogSource CurLogger { get; private set; } = null;

    private void Awake()
    {
        CurLogger = Logger;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EscapeMenuUtils.CreateButton("FILL", "FILL LOBBY", () => LobbyUtils.MakePublic());
    }
}

internal sealed class EscapeMenuUtils : MonoBehaviour
{
    internal static bool CreateButton(string name, string text, UnityAction action)
    {
        var uiButtonList = FindButtonList();
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

        var uiText = uiControl.GetComponentInChildren<TextMeshProUGUI>();
        if (uiText == null)
        {
            Plugin.CurLogger?.LogError("Failed to find the TextMeshProUGUI component!");
            Destroy(uiControl);
            return false;
        }

        uiButton.onClick.AddListener(action);
        // SynchronizationContext.Current.Post(_ => uiText.SetText(text), null);
        uiText.SetCharArray(text.ToCharArray());
        uiText.SetAllDirty();
        return true;
    }

    private static GameObject FindButtonList()
    {
        // If the plugin has broken, this is probably what needs to be updated.
        return Resources.FindObjectsOfTypeAll<Transform>()
            .FirstOrDefault(obj => obj.name == "EscapeMenu")?
            .Find("MainPage/LIST")?.gameObject;
    }
}

internal static class LobbyUtils
{
    private static CSteamID? CurrentID
    {
        get
        {
            if (MainMenuHandler.SteamLobbyHandler == null)
            {
                return null;
            }

            return Traverse.Create(MainMenuHandler.SteamLobbyHandler)
                .Field("m_CurrentLobby")
                .GetValue<CSteamID>();
        }
    }

    private static bool MasterClient
    {
        get
        {
            return MainMenuHandler.SteamLobbyHandler?.MasterClient ?? false;
        }
    }

    internal static bool MakePublic()
    {
        CSteamID? id = CurrentID;
        if (id == null)
        {
            Plugin.CurLogger?.LogWarning("Cannot make the lobby public because it does not exist.");
            return false;
        }

        if (!MasterClient)
        {
            Plugin.CurLogger?.LogWarning("Cannot make the lobby public because you are not the master client.");
            return false;
        }

        SteamMatchmaking.SetLobbyType(id.Value, ELobbyType.k_ELobbyTypePublic);
        SteamMatchmaking.SetLobbyJoinable(id.Value, true);

        // Allows players to join the game after it has started.
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }

        return true;
    }
}
