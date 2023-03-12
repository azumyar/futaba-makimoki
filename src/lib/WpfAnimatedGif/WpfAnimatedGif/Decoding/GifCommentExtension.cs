using System.IO;
using System.Text;

namespace WpfAnimatedGif.Decoding
{
	public class GifCommentExtension : GifExtension
    {
		public const int ExtensionLabel = 0xFE;

        public string Text { get; private set; }

        private GifCommentExtension()
        {
        }

		public override GifBlockKind Kind
        {
            get { return GifBlockKind.SpecialPurpose; }
        }

		public static GifCommentExtension ReadComment(Stream stream)
        {
            var comment = new GifCommentExtension();
            comment.Read(stream);
            return comment;
        }

        private void Read(Stream stream)
        {
            // Note: at this point, the label (0xFE) has already been read

            var bytes = GifHelpers.ReadDataBlocks(stream, false);
            if (bytes != null)
                Text = Encoding.ASCII.GetString(bytes);
        }
    }
}
