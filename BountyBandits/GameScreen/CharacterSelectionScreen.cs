using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BountyBandits.Character;
using BountyBandits.Animation;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.GameScreen
{
    public class CharacterSelectionScreen : BaseGameScreen
    {
        private List<String> characterOptions = new List<string>(SaveManager.getAvailableCharacterNames());
        private Dictionary<PlayerIndex, int> selectedMenuIndex = new Dictionary<PlayerIndex, int>();
        private static MarkovNameGenerator nameGenerator;
        private List<String> newCharacterOptions = new List<string>();
        private Dictionary<PlayerIndex, int> newCharacterOption = new Dictionary<PlayerIndex, int>();

        public CharacterSelectionScreen()
            : base()
        {
            nameGenerator = new MarkovNameGenerator(MarkovNameGenerator.SAMPLES, 3, 5);
            newCharacterOptions.AddRange(Enum.GetNames(typeof(PlayerTypes)));
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
            {
                newCharacterOption.Add(playerIndex, 0);
                if (!selectedMenuIndex.ContainsKey(playerIndex))
                    selectedMenuIndex.Add(playerIndex, -1);
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Input input in Game.instance.inputs)
            {
                input.update();

                if (input.getButtonHit(Buttons.A))
                {
                    bool isPlayerOneAdded = false;
                    foreach (Being player in Game.instance.players.Values)
                        if (player.input != null && player.input.useKeyboard)
                            isPlayerOneAdded = true;

                    if (selectedMenuIndex[input.getPlayerIndex()] == -1)
                        selectedMenuIndex[input.getPlayerIndex()] = 0;
                    else
                    {
                        string newCharacterName = newCharacterOptions[newCharacterOption[input.getPlayerIndex()]];
                        Being player = new Being(nameGenerator.NextName, 1, Game.instance.animationManager.getController(newCharacterName), input, true, true);
                        if (selectedMenuIndex[input.getPlayerIndex()] != 0)
                        {
                            int charindex = selectedMenuIndex[input.getPlayerIndex()] - 1;
                            String characterName = characterOptions[charindex];
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
                    int selected = selectedMenuIndex[input.getPlayerIndex()] + 1;
                    PlayerIndex[] indices = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
                    for (int playerIndex = 0; playerIndex < indices.Length; playerIndex++)
                    {
                        if (input.getPlayerIndex() != indices[playerIndex] && //same player
                            selectedMenuIndex[indices[playerIndex]] == selected)
                        {
                            selected++;
                            playerIndex = 0;
                        }
                    }
                    selectedMenuIndex[input.getPlayerIndex()] = selected;
                    if (characterOptions.Count < selected)
                        selectedMenuIndex[input.getPlayerIndex()] = 0;
                }
                if (input.getButtonHit(Buttons.DPadUp) || input.getButtonHit(Buttons.LeftThumbstickUp))
                {
                    int selected = selectedMenuIndex[input.getPlayerIndex()] - 1;
                    PlayerIndex[] indices = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
                    for (int playerIndex = 0; playerIndex < indices.Length; playerIndex++)
                    {
                        if (indices[playerIndex] != input.getPlayerIndex() && //same player
                            selected != 0 && selectedMenuIndex[indices[playerIndex]] == selected)
                        {
                            selected--;
                            playerIndex = 0;
                        }
                    }
                    if (selected <= 0)
                        selected = 0;
                    selectedMenuIndex[input.getPlayerIndex()] = selected;
                }

                if (input.getButtonHit(Buttons.DPadRight) || input.getButtonHit(Buttons.LeftThumbstickRight))
                    if (selectedMenuIndex[input.getPlayerIndex()] == 0)
                    {
                        int currOption = newCharacterOption[input.getPlayerIndex()];
                        newCharacterOption.Remove(input.getPlayerIndex());
                        newCharacterOption.Add(input.getPlayerIndex(), (1 + currOption) % newCharacterOptions.Count);
                    }
                if (input.getButtonHit(Buttons.DPadLeft) || input.getButtonHit(Buttons.LeftThumbstickLeft))
                    if (selectedMenuIndex[input.getPlayerIndex()] == 0)
                    {
                        int currOption = newCharacterOption[input.getPlayerIndex()];
                        newCharacterOption.Remove(input.getPlayerIndex());
                        newCharacterOption.Add(input.getPlayerIndex(), (newCharacterOptions.Count - 1 + currOption) % newCharacterOptions.Count);
                    }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(Game.instance.texMan.getTex("atmosphere"), new Rectangle(0, 0, res.ScreenWidth, res.ScreenHeight), Color.White);
            Vector2 fontPos = Vector2.One;
            if (Game.instance.players.Count > 0)
                drawTextBorder(Game.instance.vademecumFont24, "Press " + Input.AFFIRM_KEY + " to start game", fontPos, Color.White, Color.Black, 0);
            fontPos = new Vector2(8f, res.ScreenHeight / 2);
            foreach (PlayerIndex playerIndex in Enum.GetValues(typeof(PlayerIndex)))
            {
                if (selectedMenuIndex[playerIndex] == -1)
                    drawTextBorder(Game.instance.vademecumFont24, "Press " + Input.AFFIRM_KEY + "\nto join", fontPos, Color.White, Color.Black, 0);
                else
                {
                    List<String> saves = new List<string>();
                    saves.Add("Create New...");
                    if (selectedMenuIndex[playerIndex] == 0)
                        saves[0] = "<- Create New ->";
                    saves.AddRange(SaveManager.getAvailableCharacterNames());
                    for (int saveIndex = 0; saveIndex < saves.Count; saveIndex++)
                    {
                        Color color = selectedMenuIndex[playerIndex] == saveIndex ? Color.Yellow : Color.White;
                        drawTextBorder(Game.instance.vademecumFont24, saves[saveIndex], fontPos, color, Color.Black, 0);
                        fontPos.Y -= 28f;
                    }
                    if (selectedMenuIndex[playerIndex] == 0)
                    {
                        String name = newCharacterOptions[newCharacterOption[playerIndex]], 
                            nameCapitalized = name.Substring(0,1).ToUpper() + name.Substring(1);
                        Texture2D portrait = Game.instance.animationManager.getController(name).portrait;
                        spriteBatch.Draw(portrait, new Vector2(fontPos.X, Game.instance.res.ScreenHeight/2 - 128 - 32), Color.White);
                        Vector2 namePosition = new Vector2(fontPos.X + 64, Game.instance.res.ScreenHeight / 2 + 32);
                        drawTextBorder(Game.instance.vademecumFont24, nameCapitalized, namePosition, Color.White, Color.Black, 0);
                    }
                    fontPos.Y = res.ScreenHeight / 2;
                }
                fontPos.X += res.ScreenWidth / 4;
            }
        }
    }
}
