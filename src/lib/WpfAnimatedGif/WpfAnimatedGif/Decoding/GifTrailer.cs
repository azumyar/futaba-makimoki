namespace WpfAnimatedGif.Decoding
{
	public class GifTrailer : GifBlock
    {
		public const int TrailerByte = 0x3B;

        private GifTrailer()
        {
        }

		public override GifBlockKind Kind
        {
            get { return GifBlockKind.Other; }
        }

		public static GifTrailer ReadTrailer()
        {
            return new GifTrailer();
        }
    }
}
