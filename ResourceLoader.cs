using System.Reflection;
using UnityEngine;

namespace WhiteLib {
    public static class ResourceLoader {
        public static bool TryGetEmbeddedResourceBytes(string name, out byte[] bytes)
		{
			bytes = null;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string text = executingAssembly.GetManifestResourceNames().FirstOrDefault(delegate(string resourceName)
			{
				string name2 = executingAssembly.GetName().Name;
				return !string.IsNullOrEmpty(name2) && resourceName.StartsWith(name2) && resourceName.Contains(name);
			});
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			bool result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				executingAssembly.GetManifestResourceStream(text).CopyTo(memoryStream);
				bytes = memoryStream.ToArray();
				result = true;
			}
			return result;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003358 File Offset: 0x00001558
		public static Texture2D Texture2dFromBytes(byte[] imgBytes)
		{
			Texture2D texture2D = new(2, 2);
			ImageConversion.LoadImage(texture2D, imgBytes);
			return texture2D;
		}
    }
}