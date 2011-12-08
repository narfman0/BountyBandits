using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits
{
    public class Const
    {
        public const int GameServerPort = 14242;
    }

    public enum GameState
    {
        RootMenu, Multiplayer, JoinScreen, CharacterSelection, WorldMap, Gameplay, Cutscene
    }
    public enum RootMenuOptions
    {
        SinglePlayer, Multiplayer, Exit
    }
    public enum MultiplayerMenuOptions
    {
        Host, Join, Back
    }
    public enum JoinMenuOptions
    {
        Join, Back
    }
}
