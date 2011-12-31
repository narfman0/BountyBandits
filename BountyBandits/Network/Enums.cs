using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits.Network
{
    enum TimedUpdate 
    { 
        BeingsUpdate, ObjectsUpdate 
    }

    enum MessageType
    {
        PlayerFullUpdateClient, PlayerFullUpdateServer, BeingAnimationChange, 
        BeingDepthChange, BeingNewCombatText, BeingNewCurrentHP,
        GameState, ObjectsFullUpdate, ObjectsUpdate,
        PlayersUpdate, PlayersUpdateClient, 
        IncrementLevelRequest, LevelIndexChange, 
        NewEnemy, EnemiesUpdate
    }
}
