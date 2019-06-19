﻿using System;
using System.Collections.ObjectModel;

namespace GeometryFriends.XNAStub
{
    internal class GameComponentCollection : Collection<DrawableGameComponent>
    {
        /// <summary>
        /// Event that is triggered when a <see cref="GameComponent"/> is added
        /// to this <see cref="GameComponentCollection"/>.
        /// </summary>
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;

        /// <summary>
        /// Event that is triggered when a <see cref="GameComponent"/> is removed
        /// from this <see cref="GameComponentCollection"/>.
        /// </summary>
        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

        /// <summary>
        /// Removes every <see cref="GameComponent"/> from this <see cref="GameComponentCollection"/>.
        /// Triggers <see cref="OnComponentRemoved"/> once for each <see cref="GameComponent"/> removed.
        /// </summary>
        protected override void ClearItems()
        {
            for (int i = 0; i < base.Count; i++)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(base[i]));
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, DrawableGameComponent item)
        {
            if (base.IndexOf(item) != -1)
            {
                throw new ArgumentException("Cannot Add Same Component Multiple Times");
            }
            base.InsertItem(index, item);
            if (item != null)
            {
                this.OnComponentAdded(new GameComponentCollectionEventArgs(item));
            }
        }

        private void OnComponentAdded(GameComponentCollectionEventArgs eventArgs)
        {
            if (this.ComponentAdded != null)
            {
                this.ComponentAdded(this, eventArgs);
            }
        }

        private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
        {
            if (this.ComponentRemoved != null)
            {
                this.ComponentRemoved(this, eventArgs);
            }
        }

        protected override void RemoveItem(int index)
        {
            DrawableGameComponent gameComponent = base[index];
            base.RemoveItem(index);
            if (gameComponent != null)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(gameComponent));
            }
        }

        protected override void SetItem(int index, DrawableGameComponent item)
        {
            throw new NotSupportedException();
        }
    }
}
