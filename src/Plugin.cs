using System.Collections;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using DefaultNamespace;

namespace BetterLobby;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin
{
    internal static bool IsOnSurface
        => FindObjectOfType<DivingBell>()?.onSurface ?? false;

    public static ManualLogSource CurLogger { get; private set; } = null;

    private void Awake()
    {
        CurLogger = Logger;
        new Harmony("sonodima.BetterLobby").PatchAll(typeof(Plugin));
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only allow the plugin to run when the player is the master client
        // and on the surface.
        if (!IsOnSurface || !LobbyHelpers.IsMasterClient)
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

        PauseMenu.CreateButton("FILL", "FILL LOBBY", OnFillPress);
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

        // If the game has started, open the remote door for the player that just
        // joined.
        if (SurfaceNetworkHandler.HasStarted)
        {
            Logger.LogInfo("Game has already started, sending RPCA_OpenDoor to the late-joiner...");
            SurfaceNetworkHandler.Instance?.photonView?
                .RPC("RPCA_OpenDoor", RpcTarget.All, []);
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

        if (!IsOnSurface)
        {
            Logger.LogWarning("You need to be on surface to fill the lobby!");
            return;
        }

        LobbyHelpers.SetPublic(true);
        LobbyHelpers.SetJoinable(true);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamLobbyHandler), "InviteScreen")]
    private static bool InviteScreenPatch()
    {
        if (LobbyHelpers.IsFull)
        {
            CurLogger?.LogWarning("Lobby is already full, will not show the invite screen.");
            return false;
        }

        // TODO: Here we could decide whether we should check for IsOnSurface or not.

        // When a user opens the friend invite UI, we make the lobby friend only
        // so random people can't join. (until the "FILL LOBBY" button is pressed)
        LobbyHelpers.SetPublic(false);
        LobbyHelpers.SetJoinable(true);
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InviteFriendsTerminal), "IsGameStarted", MethodType.Getter)]
    private static bool InviteTerminalPatch(ref bool __result)
    {
        // Stop the InviteFriendsTerminal object from getting disabled when
        // the game starts.
        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EscapeMenuMainPage), "Update")]
    private static bool InviteButtonPatch(EscapeMenuMainPage __instance)
    {
        // Stop the "INVITE FRIENDS" button from getting hidden when the game
        // has started.
        //
        // I think that we could improve this, as I don't like the fact that
        // we are replacing the entire Update method. If the game updates and
        // adds more stuff in it, we have to update the patch too.
        __instance.inviteButton?.gameObject?.SetActive(true);
        return false;
    }
}
