
using HarmonyLib;

using Photon.Pun;
using Steamworks;

namespace BetterLobby;


internal static class LobbyHelpers
{
    private static readonly int s_dNumPlayers = 1;
    private static readonly int s_dMaxPlayers = 4;

    internal static bool IsFull => (NumPlayers ?? s_dNumPlayers)
        >= (MaxPlayers ?? s_dMaxPlayers);

    internal static int? NumPlayers
        => PlayerHandler.instance?.players.Count;

    internal static int? MaxPlayers =>
        Traverse.Create(MainMenuHandler.SteamLobbyHandler)
            .Field("m_MaxPlayers")
            .GetValue<int>();

    internal static CSteamID? CurrentID =>
        Traverse.Create(MainMenuHandler.SteamLobbyHandler)
            .Field("m_CurrentLobby")
            .GetValue<CSteamID>();

    internal static bool SetPublic(bool value)
    {
        if (!RunChecks(out CSteamID? id) || !id.HasValue)
        {
            return false;
        }

        Plugin.CurLogger?.LogInfo("Changing Steam lobby type to "
            + (value ? "PUBLIC" : "FRIENDS ONLY"));

        SteamMatchmaking.SetLobbyType(id.Value, value
            ? ELobbyType.k_ELobbyTypePublic
            : ELobbyType.k_ELobbyTypeFriendsOnly);
        return true;
    }

    internal static bool SetJoinable(bool value)
    {
        if (!RunChecks(out CSteamID? id) || !id.HasValue)
        {
            return false;
        }

        Plugin.CurLogger?.LogInfo("Changing the current lobby to "
            + (value ? "JOINABLE" : "NOT JOINABLE"));

        SteamMatchmaking.SetLobbyJoinable(id.Value, value);
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = value;
            PhotonNetwork.CurrentRoom.IsVisible = value;
        }

        return true;
    }

    private static bool RunChecks(out CSteamID? id)
    {
        id = CurrentID;
        if (!id.HasValue)
        {
            Plugin.CurLogger?.LogWarning("Failed to get the current lobby's CSteamID value.");
            return false;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Plugin.CurLogger?.LogWarning("You can't perform this operation because you are not the master client!");
            return false;
        }

        return true;
    }
}
