using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits.Network
{
    public enum MessageType
    {
        InitialSendCharacter, GameState, PlayersUpdate, IncrementLevelRequest, LevelIndexChange
    }
}
