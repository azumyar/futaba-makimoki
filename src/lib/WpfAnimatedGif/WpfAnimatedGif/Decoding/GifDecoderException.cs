using System;

namespace WpfAnimatedGif.Decoding
{
    [Serializable]
	public class GifDecoderException : Exception
    {
		public GifDecoderException() { }
		public GifDecoderException(string message) : base(message) { }
		public GifDecoderException(string message, Exception inner) : base(message, inner) { }
        protected GifDecoderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
