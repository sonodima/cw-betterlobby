<h1 align="center">Content Warning: BetterLobby 🌐</h1>

<div align="center">
  <img src="https://img.shields.io/badge/license-MIT-blue.svg"/>
</div>

<br>

> BetterLobby is a mod that improves the matchmaking system of _Content Warning_ by adding
> a __FILL LOBBY__ button in the pause menu

## Features

- Invite your friends in the lobby, and then fill it up with other people
- Fill the lobby back up when someone quits, even after the game has stated
- Avoid the standard matchmaking experience, which makes it hard to join a session
without an error

## Usage

- Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) in
the `Content Warning` game directory
- Copy the `BetterLobby.dll` file in the `BepInEx / plugins` folder
- Launch the game, click on __PLAY WITH FRIENDS__, and host a game
- Invite the friends you want to play with, then press <kbd>ESC</kbd> to open the pause menu
- Click on the new __FILL LOBBY__ button to open the matchmaking to the public

## Notes and Safety Measures

When using the late-join feature (prressing __FILL LOBBY__ while the game has started),
BetterLobby will wait for the lobby to reach its maximum capacity, and then disable the
late-join.

I implemented this because I suspect that leaving the lobby open would disrupt matchmaking
even more, as players would attempt to connect to our full game. _(this happens with Virality)_

Simply put, if the game has started, you opened the lobby, all players joined and then
someone left, just press __FILL LOBBY__ again and it will fill back.

Furthermore, if the lobby you are trying to make public is already full, the operation
will be ignored.

## Incompatible Plugins

_The following plugins will not work correctly together with BetterLobby!_

- [PublicHostingFix](https://thunderstore.io/c/content-warning/p/lazypatching/PublicHostingFix)
- [Virality](https://thunderstore.io/c/content-warning/p/MaxWasUnavailable/Virality)
