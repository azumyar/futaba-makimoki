using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace LibAPNG.WPF
{
    public static class Extensions
    {
        public static BitmapSource[] ToBitmapSources(this APNG apng)
        {
            var list = new List<BitmapSource>();

            if (apng.IsSimplePNG)
            {
                var frameBitmap = BitmapFactory.ConvertToPbgra32Format(
                    BitmapFrame.Create(apng.DefaultImage.GetStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad));

                list.Add(frameBitmap);
                return list.ToArray();
            }

            var firstFrame = apng.Frames.First();
            var width = firstFrame.IHDRChunk.Width;
            var height = firstFrame.IHDRChunk.Height;

            WriteableBitmap backgroundBitmap = null;
            foreach (var frame in apng.Frames)
            {
                var fcTlChunk = frame.fcTLChunk;
                WriteableBitmap foregroundBitmap;

                var frameBitmap = BitmapFactory.ConvertToPbgra32Format(
                    BitmapFrame.Create(frame.GetStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad));

                if (fcTlChunk.XOffset == 0 &&
                    fcTlChunk.YOffset == 0 &&
                    fcTlChunk.Width == width &&
                    fcTlChunk.Height == height &&
                    fcTlChunk.BlendOp == BlendOps.APNGBlendOpSource)
                {
                    foregroundBitmap = frameBitmap;
                }
                else
                {
                    foregroundBitmap = backgroundBitmap ?? BitmapFactory.New(width, height);

                    using (foregroundBitmap.GetBitmapContext())
                    {
                        // blend_op 
                        switch (fcTlChunk.BlendOp)
                        {
                            case BlendOps.APNGBlendOpSource:
                                foregroundBitmap.Blit(
                                    new Rect(fcTlChunk.XOffset, fcTlChunk.YOffset, fcTlChunk.Width, fcTlChunk.Height),
                                    frameBitmap,
                                    new Rect(0, 0, frameBitmap.PixelWidth, frameBitmap.PixelHeight),
                                    WriteableBitmapExtensions.BlendMode.None);
                                break;

                            case BlendOps.APNGBlendOpOver:
                                foregroundBitmap.Blit(
                                    new Rect(fcTlChunk.XOffset, fcTlChunk.YOffset, fcTlChunk.Width, fcTlChunk.Height),
                                    frameBitmap,
                                    new Rect(0, 0, frameBitmap.PixelWidth, frameBitmap.PixelHeight),
                                    WriteableBitmapExtensions.BlendMode.Alpha);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                // dispose_op
                switch (fcTlChunk.DisposeOp)
                {
                    case DisposeOps.APNGDisposeOpNone:
                        backgroundBitmap = foregroundBitmap.Clone();
                        break;
                    case DisposeOps.APNGDisposeOpBackground:
                        backgroundBitmap = null;
                        break;
                    case DisposeOps.APNGDisposeOpPrevious:
                        backgroundBitmap = backgroundBitmap?.Clone();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                list.Add(foregroundBitmap);
            }

            return list.ToArray();
        }

        public static ObjectAnimationUsingKeyFrames ToAnimation(this APNG apng)
        {
            var bitmaps = ToBitmapSources(apng);
            var animation = new ObjectAnimationUsingKeyFrames();

            // Repeat
            if (apng.acTLChunk.NumPlays == 0)
                animation.RepeatBehavior = RepeatBehavior.Forever;

            // KeyFrames
            var keyTime = TimeSpan.Zero;
            foreach (var (frame, index) in apng.Frames.Select((item, index) => (item, index)))
            {
                var key = new DiscreteObjectKeyFrame
                {
                    KeyTime = keyTime,
                    Value = bitmaps[index]
                };
                animation.KeyFrames.Add(key);

                var delayDen = (double)(frame.fcTLChunk.DelayDen == 0 ? 100 : frame.fcTLChunk.DelayDen);
                keyTime += frame.fcTLChunk.DelayNum == 0
                    ? TimeSpan.FromMilliseconds(1)
                    : TimeSpan.FromSeconds(frame.fcTLChunk.DelayNum / delayDen);
            }

            animation.Duration = keyTime;
            return animation;
        }


        public static Storyboard CreateStoryboardFor(this Timeline animation, Image image)
        {
            var storyboard = new Storyboard();

            Storyboard.SetTargetName(animation, image.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));

            storyboard.Children.Add(animation);

            return storyboard;
        }
    }
}
