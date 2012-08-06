using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BountyBandits.Map
{
    public interface IMovableItem
    {
        void setPosition(Vector2 position);
    }
}
