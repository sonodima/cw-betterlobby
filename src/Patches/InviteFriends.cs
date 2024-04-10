using HarmonyLib;

using DefaultNamespace;

namespace BetterLobby.Patches;


[HarmonyPatch]
internal static class InviteFriends
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamLobbyHandler), "InviteScreen")]
    private static bool InviteScreenPatch()
    {
        if (LobbyHelpers.IsFull)
        {
            Plugin.CurLogger?.LogWarning("Lobby is already full, will not show "
                + "the invite screen.");
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
