namespace GeometryFriendsAgents
{
    class GameInfo
    {
        public const int CIRCLE_RADIUS = 40;
        public const int CIRCLE_HEIGHT = 80;
        public const int CIRCLE_AREA = 6400;  // FAKE AREA

        public const int SQUARE_HEIGHT = 100;
        public const int MIN_RECTANGLE_HEIGHT = 54;
        public const int MAX_RECTANGLE_HEIGHT = 196;
        public const int RECTANGLE_AREA = 10000;

        public const int RIDE_HEIGHT = MIN_RECTANGLE_HEIGHT + CIRCLE_HEIGHT;

        public static int[] RECTANGLE_HEIGHTS = new int[3] { MIN_RECTANGLE_HEIGHT, SQUARE_HEIGHT, MAX_RECTANGLE_HEIGHT };

        public static int[] ALLOWED_HEIGHTS = new int[4] { MIN_RECTANGLE_HEIGHT, SQUARE_HEIGHT, RIDE_HEIGHT, MAX_RECTANGLE_HEIGHT };

        public const int MAX_VELOCITYX = 200;
        public const int MAX_VELOCITYY = 20;

        public const float JUMP_VELOCITYY = 437f;
        public const float FALL_VELOCITYY = 0;
        public const float GRAVITY = 299.1f;

        public const int LEVEL_WIDTH = 1272;
        public const int LEVEL_HEIGHT = 776;
        public const int LEVEL_ORIGINAL = 8;
        public const int LEVEL_MARGIN = 4;

        public const string IST_CIRCLE_PLAYING = "IST Circle Playing";
        public const string IST_RECTANGLE_PLAYING = "IST Rectangle Playing";
        public const string UNREACHABLE_PLATFORMS = "Unreachable Platforms";
        public const string COOPERATION_FINISHED = "Cooperation Finished";
        public const string JUMPED = "Jumped";
        public const string INDIVIDUAL_MOVE = "Taking Individual Move";
        public const string RIDING_HELP = "Ride Help";

        public const string UNSYNCHRONIZED = "There is a cooperation move to be executed but the players have to assume their positions.";
        public const string RIDING = "The players are in the ride position (circle is above the rectangle).";


        public enum CooperationStatus
        {
            SINGLE,
            UNSYNCHRONIZED,
            SYNCHRONIZED,
            RIDING,
            RIDING_HELP
        }
    }
}
