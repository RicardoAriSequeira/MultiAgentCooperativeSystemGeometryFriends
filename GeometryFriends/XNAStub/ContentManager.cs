using System;
using System.Collections.Generic;
using System.IO;

namespace GeometryFriends.XNAStub
{
    internal class ContentManager
    {
        public string RootDirectory { get; set; }

        private Dictionary<string, object> loadedAssets;

        public ContentManager()
        {
            loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        public T Load<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }

            T result = default(T);

            // On some platforms, name and slash direction matter.
            // We store the asset by a /-seperating key rather than how the
            // path to the file was passed to us to avoid
            // loading "content/asset1.png" and "content\\ASSET1.png" as if they were two 
            // different files. This matches stock XNA behavior.
            // The dictionary will ignore case differences
            var key = assetName.Replace('\\', '/');

            // Check for a previously loaded asset first
            object asset = null;
            if (loadedAssets.TryGetValue(key, out asset))
            {
                if (asset is T)
                {
                    return (T)asset;
                }
            }

            // Load the asset.
            result = (T) ReadAsset<T>(assetName);

            loadedAssets[key] = result;
            return result;
        }

        public object ReadAsset<T>(string assetName){
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }

            string completeAssetPath = Path.Combine(RootDirectory, assetName);

            if (typeof(T) == typeof(SpriteFont))
            {
                //load spritefont
                return new SpriteFont(completeAssetPath);
            }
            else if (typeof(T) == typeof(Texture2D))
            {
                //load texture2D
                return new Texture2D(completeAssetPath);
            }
            else if (typeof(T) == typeof(SoundEffect))
            {
                //load texture2D
                return new SoundEffect(completeAssetPath);
            }
            else
            {
                throw new ContentLoadException("Could not load " + assetName + " asset!");
            }
        }

        public void Unload()
        {

        }
    }
}
