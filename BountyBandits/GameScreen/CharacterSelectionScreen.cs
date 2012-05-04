using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BountyBandits.Character;

namespace BountyBandits.GameScreen
{
    public class CharacterSelectionScreen : BaseGameScreen
    {
        public override void Update(GameTime gameTime)
        {
            foreach (Input input in Game.instance.inputs)
            {
                input.update();

                if (input.getButtonHit(Buttons.A))
                {
                    bool isPlayerOneAdded = false;
                    foreach (Being player in Game.instance.players.Values)
                        if (player.input.useKeyboard)
                            isPlayerOneAdded = true;

                    if (Game.instance.selectedMenuIndex[input.getPlayerIndex()] == -1)
                        Game.instance.selectedMenuIndex[input.getPlayerIndex()] = 0;
                    else
                    {
                        Being player = new Being(Game.nameGenerator.NextName, 1, Game.instance, Game.instance.animationManager.getController("pirate"), input, true, true);
                        if (Game.instance.selectedMenuIndex[input.getPlayerIndex()] != 0)
                        {
                            int charindex = Game.instance.selectedMenuIndex[input.getPlayerIndex()] - 1;
                            String characterName = Game.instance.characterOptions[charindex];
                            player = SaveManager.loadCharacter(characterName, Game.instance);
                            player.isLocal = true;
                            player.isPlayer = true;
                            player.input = input;
                        }
                        List<Guid> killGuids = new List<Guid>();
                        foreach (Being extraPlayer in Game.instance.players.Values)
                            if (extraPlayer.input.getPlayerIndex() == input.getPlayerIndex())
                                killGuids.Add(extraPlayer.guid);
                        foreach (Guid kill in killGuids)
                            Game.instance.players.Remove(kill);
                        Game.instance.players.Add(player.guid, player);
                    }
                    //go to worldmap if player one hits a
                    if (isPlayerOneAdded && input.getPlayerIndex() == PlayerIndex.One)
                    {
                        if (Game.instance.network.isClient())
                        {
                            Game.instance.network.sendFullPlayersUpdateClient();
                            Game.instance.currentState.setState(GameState.WorldMap);
                        }
                        else
                            foreach (Being player in Game.instance.players.Values)
                                if (player.input.getButtonHit(Buttons.A))
                                    Game.instance.currentState.setState(GameState.WorldMap);
                    }
                }
                if (input.getButtonHit(Buttons.DPadDown) || input.getButtonHit(Buttons.LeftThumbstickDown))
                {
                    int selected = Game.instance.selectedMenuIndex[input.getPlayerIndex()] + 1;
                    PlayerIndex[] indices = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
                    for (int playerIndex = 0; playerIndex < indices.Length; playerIndex++)
                    {
                        if (input.getPlayerIndex() != indices[playerIndex] && //same player
                            Game.instance.selectedMenuIndex[indices[playerIndex]] == selected)
                        {
                            selected++;
                            playerIndex = 0;
                        }
                    }
                    Game.instance.selectedMenuIndex[input.getPlayerIndex()] = selected;
                    if (Game.instance.characterOptions.Count < selected)
                        Game.instance.selectedMenuIndex[input.getPlayerIndex()] = 0;
                }
                if (input.getButtonHit(Buttons.DPadUp) || input.getButtonHit(Buttons.LeftThumbstickUp))
                {
                    int selected = Game.instance.selectedMenuIndex[input.getPlayerIndex()] - 1;
                    PlayerIndex[] indices = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
                    for (int playerIndex = 0; playerIndex < indices.Length; playerIndex++)
                    {
                        if (indices[playerIndex] != input.getPlayerIndex() && //same player
                            selected != 0 && Game.instance.selectedMenuIndex[indices[playerIndex]] == selected)
                        {
                            selected--;
                            playerIndex = 0;
                        }
                    }
                    if (selected <= 0)
                        selected = 0;
                    Game.instance.selectedMenuIndex[input.getPlayerIndex()] = selected;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(Game.instance.texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            Vector2 fontPos = new Vector2(1.0f, 1.0f);
            if (Game.instance.players.Count > 0)
                drawTextBorder(Game.instance.vademecumFont24, "Press " + Input.AFFIRM_KEY + " to start game", fontPos, Color.White, Color.Black, 0);
            fontPos = new Vector2(1.0f, res.ScreenHeight / 2);
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
            {
                if (Game.instance.selectedMenuIndex[playerIndex] == -1)
                    drawTextBorder(Game.instance.vademecumFont24, "Press " + Input.AFFIRM_KEY + "\nto join", fontPos, Color.White, Color.Black, 0);
                else
                {
                    List<String> saves = new List<string>();
                    saves.Add("Create New...");
                    saves.AddRange(SaveManager.getAvailableCharacterNames());
                    for (int saveIndex = 0; saveIndex < saves.Count; saveIndex++)
                    {
                        Color color = Game.instance.selectedMenuIndex[playerIndex] == saveIndex ? Color.Yellow : Color.White;
                        drawTextBorder(Game.instance.vademecumFont24, saves[saveIndex], fontPos, color, Color.Black, 0);
                        fontPos.Y -= 28f;
                    }
                    fontPos.Y = res.ScreenHeight / 2;
                }
                fontPos.X += res.ScreenWidth / 4;
            }
        }
    }
}
