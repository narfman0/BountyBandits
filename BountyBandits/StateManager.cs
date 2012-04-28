using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.GameScreen;

namespace BountyBandits
{
    public class StateManager
    {
        private Game gameref;
        private GameState current = GameState.RootMenu;
        private BaseGameScreen screen;

        public StateManager(Game gameref)
        {
            this.gameref = gameref;
            screen = new MainScreen(gameref);
        }

        public void setState(GameState newState){
            bool skip = (current == GameState.Gameplay && newState == GameState.Cutscene) ||
                        (current == GameState.Cutscene && newState == GameState.Gameplay);
            current = newState;
            switch (current)
            {
                case GameState.CharacterSelection:
                    screen = new CharacterSelectionScreen(gameref);
                    break;
                case GameState.Cutscene:
                    screen = new CutsceneScreen(gameref);
                    break;
                case GameState.Gameplay:
                    screen = new GameplayScreen(gameref);
                    break;
                case GameState.JoinScreen:
                    screen = new JoinScreen(gameref);
                    break;
                case GameState.Multiplayer:
                    screen = new MultiplayerScreen(gameref);
                    break;
                case GameState.RootMenu:
                    screen = new MainScreen(gameref);
                    break;
                case GameState.WorldMap:
                    screen = new WorldMapScreen(gameref);
                    break;
            }
            if(!skip)
                gameref.network.sendGameStateUpdate();
        }

        public GameState getState() { return current; }

        public BaseGameScreen getScreen() { return screen; }
    }
}
