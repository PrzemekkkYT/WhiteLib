using RedLoader;
using UnityEngine;

namespace WhiteLib {
    public class Assets {
        public static Texture2D ArrowLeft = new(0, 0);
        public static Texture2D ArrowRight = new(0, 0);

        public static void Init() {
            byte[] imgBytes;
            if (ResourceLoader.TryGetEmbeddedResourceBytes("ArrowLeft", out imgBytes)) {
				ArrowLeft = ResourceLoader.Texture2dFromBytes(imgBytes);
			} else {
				RLog.Error("Couldn't load ArrowLeft.png");
			}
            if (ResourceLoader.TryGetEmbeddedResourceBytes("ArrowRight", out imgBytes)) {
				ArrowRight = ResourceLoader.Texture2dFromBytes(imgBytes);
			} else {
				RLog.Error("Couldn't load ArrowRight.png");
			}
        }
    }
}