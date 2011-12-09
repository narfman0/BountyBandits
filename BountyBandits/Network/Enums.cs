﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits.Network
{
    enum ServerTimedUpdate { State, PlayersUpdates }
    public enum MessageType
    {
        InitialSendCharacter, GameState, PlayersUpdate, IncrementLevelRequest, LevelIndexChange
    }
}
