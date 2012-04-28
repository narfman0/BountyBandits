using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BountyBandits.Character;
using BountyBandits.Story;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace BountyBandits.GameScreen
{
    public class CutsceneScreen : BaseGameScreen
    {
        public CutsceneScreen(Game game) : base(game) { }

        public override void Update(GameTime gameTime) 
        {
            float timeElapsed = (float)gameTime.ElapsedGameTime.Milliseconds;
            double elapsedCutsceneTime = gameTime.TotalGameTime.TotalMilliseconds - game.timeStoryElementStarted;
            #region audio
            AudioElement audio = game.storyElement.popAudioElement(elapsedCutsceneTime);
            if (audio != null)
                game.Content.Load<SoundEffect>(game.mapManager.currentCampaignPath + audio.audioPath).Play();
            #endregion
            #region characters
            foreach (BeingController controller in game.storyElement.beingControllers)
            {
                if (!game.storyBeings.ContainsKey(controller.entranceMS) &&
                    controller.entranceMS >= elapsedCutsceneTime)
                {
                    Being being = new Being(controller.entranceMS + "", 1, game, controller.animationController, null, false, true);
                    being.body.Position = controller.startLocation;
                    being.changeAnimation(controller.animations[0].animationName);
                    being.setDepth(controller.startDepth);
                    game.storyBeings.Add(controller.entranceMS, being);
                }
                if (game.storyBeings.ContainsKey(controller.entranceMS))
                {
                    Being being = game.storyBeings[controller.entranceMS];
                    being.changeAnimation(controller.getCurrentAnimation(elapsedCutsceneTime));
                    ActionStruct currentAction = controller.getCurrentAction(elapsedCutsceneTime);
                    if (currentAction != null)
                    {
                        switch (currentAction.action)
                        {
                            case ActionEnum.Jump:
                                being.jump();
                                break;
                            case ActionEnum.Stop:
                                being.body.ApplyForce(-being.body.Force);
                                break;
                            case ActionEnum.Move:
                                being.move(new Vector2(currentAction.intensity, 0));
                                break;
                        }
                    }
                }
            }
            foreach (Being being in game.storyBeings.Values)
                being.update(gameTime);
            #endregion
            #region quit cutscene
            bool startPressed = false;
            foreach (Being player in game.players.Values)
                if (player.isLocal && player.input.getButtonHit(Buttons.Start))
                    startPressed = true;
            double msTotal = gameTime.TotalGameTime.TotalMilliseconds;
            if (startPressed || game.storyElement.cutsceneLength + 500 < msTotal - game.timeStoryElementStarted)
            {
                game.currentState.setState(GameState.Gameplay);
                game.storyElement = null;
                game.storyBeings.Clear();
            }
            #endregion
            game.physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                game.drawGameplay(game.getAvePosition() + new Vector2(game.storyElement.getCameraOffset(gameTime).X, 0f));
                foreach (Being storyBeing in game.storyBeings.Values)
                    storyBeing.draw();
            }
            catch (Exception e) { System.Console.WriteLine(e.StackTrace); }
            game.drawTextBorder(game.vademecumFont18, "Press Start to skip cutscene", new Vector2(2, res.ScreenHeight - 40), Color.Black, Color.White, 0);
        }
    }
}
