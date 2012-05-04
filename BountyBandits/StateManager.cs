using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.GameScreen;

namespace BountyBandits
{
    public class StateManager
    {
        private GameState current = GameState.RootMenu;
        private BaseGameScreen screen;

        public StateManager()
        {
            screen = new MainScreen();
        }

        public void setState(GameState newState){
            bool skip = (current == GameState.Gameplay && newState == GameState.Cutscene) ||
                        (current == GameState.Cutscene && newState == GameState.Gameplay);
            current = newState;
            switch (current)
            {
                case GameState.CharacterSelection:
                    screen = new CharacterSelectionScreen();
                    break;
                case GameState.Cutscene:
                    screen = new CutsceneScreen();
                    break;
                case GameState.Gameplay:
                    screen = new GameplayScreen();
                    break;
                case GameState.JoinScreen:
                    screen = new JoinScreen();
                    break;
                case GameState.Multiplayer:
                    screen = new MultiplayerScreen();
                    break;
                case GameState.RootMenu:
                    screen = new MainScreen();
                    break;
                case GameState.WorldMap:
                    screen = new WorldMapScreen();
                    break;
            }
            if(!skip)
                Game.instance.network.sendGameStateUpdate();
        }

        public GameState getState() { return current; }

        public BaseGameScreen getScreen() { return screen; }
    }
}
