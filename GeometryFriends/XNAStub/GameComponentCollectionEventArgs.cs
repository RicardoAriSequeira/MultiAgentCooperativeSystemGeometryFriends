using System;

namespace GeometryFriends.XNAStub
{
    internal class GameComponentCollectionEventArgs : EventArgs
    {
        private DrawableGameComponent _gameComponent;

        public GameComponentCollectionEventArgs(DrawableGameComponent gameComponent)
        {
            _gameComponent = gameComponent;
        }

        public DrawableGameComponent GameComponent
        {
            get
            {
                return _gameComponent;
            }
        }
    }
}
