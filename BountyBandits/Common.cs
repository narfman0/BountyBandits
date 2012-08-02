using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits
{
    public class Const
    {
        public const int GameServerPort = 14242, GameClientPort = 14243;
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
    public enum BeingTypes
    {
        amish, buddhistmonk, cow, cowboy, frenchman, godzilla, governator,
        hippie, hitler, kimjongil, mexican, mountie, nerd, obama, panda,
        pedobear, seal, shakespeare, sloth, stalin, sumo, tikiSmile, tikiTeeth,
        poe, cat, virgil
    }
    public enum PlayerTypes
    {
        pirate, russian, ninja
    }
}
