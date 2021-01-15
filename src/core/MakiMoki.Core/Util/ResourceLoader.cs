using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public class ResourceLoader {
		private Type Target { get; }

		public ResourceLoader(Type target) {
			System.Diagnostics.Debug.Assert(target != null);
	
			this.Target = target;
		}

		public Stream Get(string file) {
			System.Diagnostics.Debug.Assert(file != null);

			return this.Target.Assembly.GetManifestResourceStream(
				$"{ this.Target.Namespace }.{ file }");
		}
	}
	public static class ResourceLoader<T> {
		private static ResourceLoader Loader { get; } = new ResourceLoader(typeof(T));

		public static Stream Get(string file) => Loader.Get(file);
	}
}
