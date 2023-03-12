using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LibAPNG.WPF.Test
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var apng = new APNG("firefox.png");
            if (apng.IsSimplePNG)
                PngImage.Source = BitmapFrame.Create(
                    apng.DefaultImage.GetStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            else
                apng.ToAnimation().CreateStoryboardFor(PngImage).Begin(PngImage);


            var bitmaps = apng.ToBitmapSources();
            foreach (var (bitmap, index) in bitmaps.Select((item, index) => (item, index)))
            {
                using (var fileStream = new FileStream($"{index}.png", FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }
            }
        }
    }
}
