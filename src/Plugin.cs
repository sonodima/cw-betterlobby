﻿using System.Collections;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterLobby;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin
{
    public static ManualLogSource CurLogger { get; private set; } = null;

    private void Awake()
    {
        CurLogger = Logger;
        new Harmony("sonodima.BetterLobby").PatchAll();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only allow the plugin to run when the player is the master client
        // and on the surface.
        if (!PhotonGameLobbyHandler.IsSurface || !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // This is used to keep track of how many players there are, so that if the user
        // is using the late join feature, the lobby does not stay open after it has
        // filled. (Landfall thank me later)
        if (PlayerHandler.instance != null)
        {
            PlayerHandler.instance.OnPlayerJoined
                += (player) => StartCoroutine(OnPlayerJoined(player));
        }

        PauseMenu.AddButton("FILL", "FILL LOBBY", OnFillPress, 1);
    }

    private IEnumerator OnPlayerJoined(Player player)
    {
        // If the lobby has filled and the game has already started, close it
        // to the public.
        // It will stay open for friends to join, but consider that if it is full
        // they will not be able to join and receive a matchmaking error.
        if (LobbyHelpers.IsFull && SurfaceNetworkHandler.HasStarted)
        {
            Logger.LogWarning("Maximum number of players reached, closing lobby to the public...");
            LobbyHelpers.SetPublic(false);
        }

        // If we execute the RPC immediatly, bad things happen :(
        yield return new WaitForSeconds(2f);

        // Send the current objective another time, so that the newly connected client
        // is on par with the progress.
        if (PhotonGameLobbyHandler.CurrentObjective != null)
        {
            PhotonGameLobbyHandler.Instance.SetCurrentObjective(
                PhotonGameLobbyHandler.CurrentObjective);
        }

        // If the game has started, open the remote door for the player that just
        // joined.
        if (SurfaceNetworkHandler.Instance != null && SurfaceNetworkHandler.HasStarted)
        {
            Logger.LogInfo("Game has already started, sending RPCA_OpenDoor to the late-joiner...");
            SurfaceNetworkHandler.Instance.photonView.RPC(
                "RPCA_OpenDoor", RpcTarget.All, []);
        }

        yield break;
    }

    private void OnFillPress()
    {
        if (LobbyHelpers.IsFull)
        {
            Logger.LogWarning("Lobby is already full, will not make it public.");
            return;
        }

        if (!PhotonGameLobbyHandler.IsSurface)
        {
            Logger.LogWarning("You need to be on surface to fill the lobby!");
            return;
        }

        LobbyHelpers.SetPublic(true);
        LobbyHelpers.SetJoinable(true);
    }
}
