using System.Linq;
using System.Collections;

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
        // This is used to keep track of how many players there are, so that if the user
        // is using the late join feature, the lobby does not stay open after it has
        // filled. (Landfall thank me later)
        if (PlayerHandler.instance != null)
        {
            PlayerHandler.instance.OnPlayerJoined 
                += (player) => StartCoroutine(OnPlayerJoined(player));
        }

        EscapeMenuUtils.CreateButton("FILL", "FILL LOBBY", OnFillPress);
    }

    private IEnumerator OnPlayerJoined(Player player)
    {
        int curPlayers = PlayerHandler.instance?.players.Count ?? 1;
        int maxPlayers = LobbyUtils.MaxPlayers ?? 4;
        Logger.LogInfo($"Player has joined the lobby! {curPlayers} / {maxPlayers}");
        if (curPlayers >= maxPlayers)
        {
            // Lobby has filled: if the game has already started and the lobby is
            // open for late join, stop accepting new clients. 2kind I know.
            if (SurfaceNetworkHandler.HasStarted && LobbyUtils.IsOpen)
            {
                Logger.LogWarning("Maximum number of players reached, closing lobby...");
                LobbyUtils.StopAccepting();
            }
        }

        // If we execute the RPC immediatly, bad things happen :(
        yield return new WaitForSeconds(2f);

        // If the game has started, open the remote door for the player that just
        // joined.
        if (SurfaceNetworkHandler.HasStarted)
        {
            Logger.LogInfo("Game has already started, sending RPCA_OpenDoor to the late-joiner...");
            SurfaceNetworkHandler.Instance?.photonView?.RPC("RPCA_OpenDoor", RpcTarget.All, []);
        }

        yield break;
    }

    private void OnFillPress()
    {
        int curPlayers = PlayerHandler.instance?.players.Count ?? 1;
        int maxPlayers = LobbyUtils.MaxPlayers ?? 4;
        if (curPlayers >= maxPlayers)
        {
            Logger.LogWarning("Lobby is already full, will not make it public.");    
            return;
        }

        LobbyUtils.MakePublic();
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
    private static bool MasterClient => MainMenuHandler.SteamLobbyHandler?.MasterClient ?? false;
    internal static bool IsOpen = (PhotonNetwork.CurrentRoom?.IsOpen ?? false) 
        && (PhotonNetwork.CurrentRoom?.IsVisible ?? false);

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

    internal static int? MaxPlayers
    {
        get
        {
            if (MainMenuHandler.SteamLobbyHandler == null)
            {
                return null;
            }

            return Traverse.Create(MainMenuHandler.SteamLobbyHandler)
                .Field("m_MaxPlayers")
                .GetValue<int>();
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
        SetJoinable(id.Value, true);
        return true;
    }

    internal static bool StopAccepting()
    {
        CSteamID? id = CurrentID;
        if (id == null)
        {
            return false;
        }

        if (!MasterClient)
        {
            return false;
        }

        SetJoinable(id.Value, false);
        return true;
    }

    private static void SetJoinable(CSteamID id, bool value)
    {
        SteamMatchmaking.SetLobbyJoinable(id, value);

        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = value;
            PhotonNetwork.CurrentRoom.IsVisible = value;
        }
    }
}
