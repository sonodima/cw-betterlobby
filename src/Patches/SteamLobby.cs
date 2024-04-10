using HarmonyLib;

namespace BetterLobby.Patches;


// Elegance: 0%
// Effectiveness: 100%
// Exactly what we needed!

[HarmonyPatch]
public static class LobbyHandler
{
    private static bool? s_wasHostPrivate = null;

    /// <summary>
    /// Lobbies are always created as k_ELobbyTypePublic, even if they will
    /// become k_ELobbyTypeFriendsOnly later.
    /// This is required starting from April 10th 2024, which broke the previous
    /// method we used to change the lobby type.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamLobbyHandler), "HostMatch")]
    private static bool OnHostMatch(ref bool privateMatch)
    {
        Plugin.CurLogger?.LogInfo($"Requested " + (privateMatch ? "PRIVATE" : "PUBLIC")
            + " match. Creating a PUBLIC lobby...");

        // Store the actual target lobby type, which we will apply after the
        // lobby initialization has finished.
        s_wasHostPrivate = privateMatch;
        privateMatch = false;
        return true;
    }

    /// <summary>
    /// This function changes the type of the lobby to k_ELobbyTypePublic.
    /// It is called at the end of the surface initialization method, which
    /// is called on room join.
    /// With this patch we can restore the actual target lobby type that we
    /// stored previously, to match what it actually needs to be.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SteamLobbyHandler), "OpenLobby")]
    private static void OpenLobby()
    {
        if (!s_wasHostPrivate.HasValue)
        {
            Plugin.CurLogger?.LogWarning("OpenLobby called, but we didn't store a "
                + "lobby type target! Check this routine again!");
            return;
        }

        Plugin.CurLogger?.LogInfo("Making the created lobby "
            + (s_wasHostPrivate.Value ? "PRIVATE" : "PUBLIC")
            + " as specified in HostMatch...");

        LobbyHelpers.SetPublic(!s_wasHostPrivate.Value);
        s_wasHostPrivate = null; 
    }
}
