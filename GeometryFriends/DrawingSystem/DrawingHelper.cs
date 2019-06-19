
namespace GeometryFriends.DrawingSystem {
    internal class DrawingHelper {
        private static bool InShell(int x, int y, int width, int height, int shell) {
            //check each line of rectangle.
            if ((x == shell && IsBetween(y, shell, height - 1 - shell)) || (x == width - 1 - shell && IsBetween(y, shell, height - 1 - shell))) {
                return true;
            }
            else if ((y == shell && IsBetween(x, shell, width - 1 - shell)) || (y == height - 1 - shell && IsBetween(x, shell, width - 1 - shell))) {
                return true;
            }
            else {
                return false;
            }
        }

        private static bool IsBetween(float value, float min, float max) {
            if (value >= min && value <= max) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}
