using System;

namespace Yarukizero.Net.MakiMoki.Exceptions {
	public class InitializeFailedException : Exception {
		public InitializeFailedException() { }
		public InitializeFailedException(string message) : base(message) { }
		public InitializeFailedException(string message, Exception e) : base(message, e) { }
	}
	public class MigrateFailedException : Exception {
		public MigrateFailedException() { }
		public MigrateFailedException(string message) : base(message) { }
		public MigrateFailedException(string message, Exception e) : base(message, e) { }
	}
	public class ImageLoadFailedException : Exception {
		public ImageLoadFailedException() { }
		public ImageLoadFailedException(string message) : base(message) { }
		public ImageLoadFailedException(string message, Exception e) : base(message, e) { }
	}
}