﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gem.Console
{
    class ConsoleCompositor
    {
        //dummy
        private float GetConsoleSizeY()
        {
            return 100.0f;
        }

        public void Compose()
        {
            SpriteFont font = null;

            var cursor = new Cursor();
            var appender = new CellAppender((ch) =>
            {
                string cell = ch.ToString();
                var size = font.MeasureString(cell);
                return new Cell(cell, (int)size.X, (int)size.Y);
            });
            var aligner = new CellAligner(GetConsoleSizeY, appender.GetCells);

            appender.OnCellAppend((sender, args) => aligner.ArrangeRows());
            appender.OnCellAppend((sender, args) =>
            {
               cursor.Update(aligner.Rows());
               cursor.Right();
            });

        }
    }
}
