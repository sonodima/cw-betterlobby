<h1 align="center">Content Warning: BetterLobby 🌐</h1>

<div align="center">
  <img src="https://img.shields.io/badge/license-MIT-blue.svg"/>
</div>

<br>

> BetterLobby is a mod that improves the matchmaking system of _Content Warning_ by adding
> a __FILL LOBBY__ button in the pause menu and modifying the behaviour of the
> __INVITE FRIENDS__ button

## Features

- Invite your friends in the lobby, and then fill it up with other people
- Fill the lobby back up when someone quits, even after the game has started
- Avoid the standard matchmaking experience, which makes it hard to join a session
without an error
- Invite friends to join, even when already in the underworld _(may have some glitches, for now)_

## Usage

- Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) in
the `Content Warning` game directory
- Copy the `BetterLobby.dll` file in the `BepInEx / plugins` folder
- Launch the game, click on __PLAY WITH FRIENDS__, and host a game
- Invite the friends you want to play with, then press <kbd>ESC</kbd> to open the pause menu
- Click on the new __FILL LOBBY__ button to open the matchmaking to the public

When you press the __INVITE FRIENDS__ button, the matchmaking is closed to the public and
becomes friends-only. To open it up again, press __FILL LOBBY__.

## Notes and Safety Measures

__LATE-JOIN IN THE UNDERWORLD IS DISABLED FOR RANDOMS__

Late-join for randoms is only enabled when on the surface, to avoid other players getting put
directly in the underworld mid-game.
You can use the __INVITE FRIENDS__ button even in the underworld, but the spawn points
may be messed up.
If you want to fill the lobby back up with randoms, return to the surface first. 

__LATE-JOIN DISABLES ITSELF AFTER REACHING MAX LOBBY CAPACITY__

When using the late-join feature _(pressing __FILL LOBBY__ while the game has started)_,
BetterLobby will wait for the lobby to reach its maximum capacity, and then disable the
late-join.

I implemented this because I suspect that leaving the lobby open would disrupt matchmaking
even more, as players would attempt to connect to our full game. _(this happens with Virality)_

Simply put, if the game has started, you opened the lobby, all players joined and then
someone left, just press __FILL LOBBY__ again and it will fill back.

__I PRESS ON THE BUTTONS BUT NOTHING HAPPENS__

If the lobby already has the maximum number of players connected, all operations will
be ignored.

Wait for a player to leave before inviting friends or opening the lobby to the public.

## Incompatible Plugins

_The following plugins may not work correctly together with BetterLobby!_

- [PublicHostingFix](https://thunderstore.io/c/content-warning/p/lazypatching/PublicHostingFix)
- [Virality](https://thunderstore.io/c/content-warning/p/MaxWasUnavailable/Virality)
