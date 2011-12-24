using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BountyBandits
{

    /// <summary>
    /// Enum that can represent a screen resolution
    /// </summary>
    public enum ScreenMode
    {
        /// <summary>
        /// 160x120
        /// </summary>
        QQVGA,
        /// <summary>
        /// 320x240
        /// </summary>
        QVGA,
        /// <summary>
        /// 640x480
        /// </summary>
        VGA,
        /// <summary>
        /// 800x600
        /// </summary>
        SVGA,
        /// <summary>
        /// 1024x768
        /// </summary>
        XGA,
        /// <summary>
        /// 1280x1024
        /// </summary>
        SXGA,
        /// <summary>
        /// 1600x1200
        /// </summary>
        UXGA,
        /// <summary>
        /// 2048x1536
        /// </summary>
        QXGA,
        /// <summary>
        /// 2560x2048
        /// </summary>
        QSXGA,
        /// <summary>
        /// 3200x2400
        /// </summary>
        QUXGA,
        /// <summary>
        /// 4096x3072
        /// </summary>
        HXGA,
        /// <summary>
        /// 5120x4096
        /// </summary>
        HSXGA,
        /// <summary>
        /// 6400, 4080
        /// </summary>
        HUXGA,
        /// <summary>
        /// 1280,720
        /// </summary>
        WXGA,
        /// <summary>
        /// 1440x900
        /// </summary>
        WSXGA,
        /// <summary>
        /// 1680x1050
        /// </summary>
        WSXGAplus,
        /// <summary>
        /// 1920x1200
        /// </summary>
        WUXGA,
        /// <summary>
        /// 2560x1600
        /// </summary>
        WQXGA,
        /// <summary>
        /// 3200x2048
        /// </summary>
        WQSXGA,
        /// <summary>
        /// 3840x2400
        /// </summary>
        WQUXGA,
        /// <summary>
        /// 5120x3200
        /// </summary>
        WHXGA,
        /// <summary>
        /// 6400x4096
        /// </summary>
        WHSXGA,
        /// <summary>
        /// 7680x4800
        /// </summary>
        WHUXGA,
        /// <summary>
        /// 640x480
        /// </summary>
        tv480i,
        /// <summary>
        /// 1280x720
        /// </summary>
        tv720p,
        /// <summary>
        /// 1920x1080
        /// </summary>
        tv1080p
    }

    public class Resolution
    {
        private int screenWidth;
        private int screenHeight;
        private bool widescreen;
        private Matrix scale;
        private ScreenMode currentMode;
        private ScreenMode baseMode;

        private static readonly int[,] resolutions =
        new int[,] {
        // Normal
        {160, 120},
        {320, 240},
        {640, 480},
        {800, 600},
        {1024, 768},
        {1280, 1024},
        {1600, 1200},
        {2048, 1536},
        {2560, 2048},
        {3200, 2400},
        {4096, 3072},
        {5120, 4096},
        {6400, 4800},
        // Widescreen
        {1280, 720},
        {1440, 900},
        {1680, 1050},
        {1920, 1200},
        {2560, 1600},
        {3200, 2048},
        {3840, 2400},
        {5120, 3200},
        {6400, 4096},
        {7680, 4800},
        // TV
        {640, 480},
        {1280, 720},
        {1920, 1080}
        };


        public int ScreenWidth { get { return this.screenWidth; } }

        public int ScreenHeight { get { return this.screenHeight; } }

        public bool WideScreen { get { return this.widescreen; } }

        public Matrix Scale { get { return this.scale; } }

        public ScreenMode BaseMode
        {
            get { return this.baseMode; }
            set { this.baseMode = value; }
        }

        public ScreenMode Mode
        {
            get { return currentMode; }
            set
            {
                currentMode = value;
                this.screenWidth = resolutions[(int)currentMode, 0];
                this.screenHeight = resolutions[(int)currentMode, 1];
                if ((int)currentMode >= 13)
                    this.widescreen = true;
                else
                    this.widescreen = false;
            }
        }


        public Resolution(GraphicsDeviceManager graphics)
        {
            this.Mode = ScreenMode.XGA;
            this.baseMode = this.currentMode;
            SetResolution(graphics);
        }

        public Resolution(GraphicsDeviceManager graphics, ScreenMode mode)
        {
            this.Mode = mode;
            this.baseMode = this.currentMode;
            SetResolution(graphics);
        }

        public void Initialize(GraphicsDeviceManager graphics)
        {
            DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            if (displayMode.Width == 1920 && displayMode.Height == 1080)
                this.Mode = ScreenMode.tv1080p;
            else if (displayMode.Width == 1280 && displayMode.Height == 720)
                this.Mode = ScreenMode.tv720p;
            else if (displayMode.Width == 1024 && displayMode.Height == 768)
                this.Mode = ScreenMode.XGA;
            else
                this.Mode = ScreenMode.tv480i;
            this.baseMode = this.currentMode;
            SetResolution(graphics);
        }

        public void SetResolution(GraphicsDeviceManager graphics)
        {
            graphics.PreferredBackBufferWidth = this.screenWidth;
            graphics.PreferredBackBufferHeight = this.screenHeight;
            scale = Matrix.CreateScale(
                    (float)screenWidth / (float)resolutions[(int)baseMode, 0],
                    (float)screenHeight / (float)resolutions[(int)baseMode, 1],
                    1f);

            graphics.ApplyChanges();
        }
    }
}

