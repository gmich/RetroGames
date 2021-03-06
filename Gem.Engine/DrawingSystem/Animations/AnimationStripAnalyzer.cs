﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gem.DrawingSystem.Animations
{
    public class AnimationStripAnalyzer
    {
        private readonly AnimationStripSettings settings;
        private readonly int tileSheetColumns;
        private readonly int tileSheetRows;
        private readonly int tileSheetCount;
        private readonly List<Tuple<int, Rectangle>> frames = new List<Tuple<int, Rectangle>>();

        public AnimationStripAnalyzer(AnimationStripSettings settings)
        {
            this.settings = settings;
            tileSheetColumns = ((settings.TileSheetWidth-1) / settings.FrameWidth)+ 1;
            tileSheetRows = ((settings.TileSheetHeight-1) / settings.FrameHeight) + 1;
            tileSheetCount = tileSheetColumns * tileSheetRows;
            ParseSpriteSheet(0);

        }

        public int Width => tileSheetColumns * settings.FrameWidth;

        public int Height => tileSheetRows * settings.FrameHeight;

        public IEnumerable<Tuple<int, Rectangle>> Frames
        { get { return frames; } }

        private void ParseSpriteSheet(int currentFrame)
        {
            int frameInRow = currentFrame / tileSheetColumns;
            int frameinColumn = currentFrame % tileSheetColumns;

            frames.Add(new Tuple<int, Rectangle>(
                currentFrame,
                    new Rectangle(
                    frameinColumn * settings.FrameWidth,
                    frameInRow * settings.FrameHeight,
                    settings.FrameWidth,
                    settings.FrameHeight)));

            int nextFrame = currentFrame + 1;
            if (nextFrame >= tileSheetCount)
            {
                return;
            }
            ParseSpriteSheet(nextFrame);
        }
    }
}