using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerGames.FarseerPhysics;

namespace BountyBandits
{
    public static class PhysicsHelper
    {
        public static CollisionCategory depthToCollisionCategory(int depth)
        {
            switch (depth)
            {
                case 0:
                    return CollisionCategory.Cat1;
                case 1:
                    return CollisionCategory.Cat2;
                case 2:
                    return CollisionCategory.Cat3;
                case 3:
                    return CollisionCategory.Cat4;
            }
            return CollisionCategory.None;
        }

        public static int collisionCategoryToDepth(CollisionCategory category)
        {
            switch (category)
            {
                case CollisionCategory.Cat1:
                    return 0;
                case CollisionCategory.Cat2:
                    return 1;
                case CollisionCategory.Cat3:
                    return 2;
                case CollisionCategory.Cat4:
                    return 3;
            }
            return -1;
        }
    }
}
