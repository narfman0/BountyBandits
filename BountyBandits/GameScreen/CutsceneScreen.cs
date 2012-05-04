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
        public Dictionary<int, Being> storyBeings;
        public double timeStoryElementStarted;
        private GameTime previousGameTime;

        public CutsceneScreen() {
            timeStoryElementStarted = Environment.TickCount;
            storyBeings = new Dictionary<int, Being>();
        }

        public override void Update(GameTime gameTime) 
        {
            double elapsedCutsceneTime = Environment.TickCount - timeStoryElementStarted;
            #region audio
            AudioElement audio = Game.instance.storyElement.popAudioElement(elapsedCutsceneTime);
            if (audio != null)
                Game.instance.Content.Load<SoundEffect>(Game.instance.mapManager.currentCampaignPath + audio.audioPath).Play();
            #endregion
            #region characters
            foreach (BeingController controller in Game.instance.storyElement.beingControllers)
            {
                if (!storyBeings.ContainsKey(controller.entranceMS) &&
                    controller.entranceMS >= elapsedCutsceneTime)
                {
                    Being being = new Being(controller.entranceMS + "", 1, Game.instance, controller.animationController, null, false, true);
                    being.body.Position = controller.startLocation;
                    being.changeAnimation(controller.animations[0].animationName);
                    being.setDepth(controller.startDepth);
                    storyBeings.Add(controller.entranceMS, being);
                }
                if (storyBeings.ContainsKey(controller.entranceMS))
                {
                    Being being = storyBeings[controller.entranceMS];
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
            foreach (Being being in storyBeings.Values)
                being.update(gameTime);
            #endregion
            #region quit cutscene
            bool startPressed = false;
            foreach (Being player in Game.instance.players.Values)
                if (player.isLocal && player.input.getButtonHit(Buttons.Start))
                    startPressed = true;
            if (startPressed || Game.instance.storyElement.cutsceneLength + 500 < Environment.TickCount - timeStoryElementStarted)
            {
                Game.instance.currentState.setState(GameState.Gameplay);
                Game.instance.storyElement = null;
                storyBeings.Clear();
            }
            #endregion
            #region Physics
            if (previousGameTime == null)
                previousGameTime = gameTime;
            float timeElapsed = (float)gameTime.TotalGameTime.TotalMilliseconds - (float)previousGameTime.TotalGameTime.TotalMilliseconds;
            Game.instance.physicsSimulator.Update((timeElapsed > .1f) ? timeElapsed : .1f);
            previousGameTime = gameTime;
            #endregion
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                drawGameplay(Game.instance.getAvePosition() + new Vector2(Game.instance.storyElement.getCameraOffset(gameTime).X, 0f));
                foreach (Being storyBeing in storyBeings.Values)
                    storyBeing.draw();
            }
            catch (Exception e) { System.Console.WriteLine(e.StackTrace); }
            drawTextBorder(Game.instance.vademecumFont18, "Press Start to skip cutscene", new Vector2(2, res.ScreenHeight - 40), Color.Black, Color.White, 0);
        }
    }
}
