# APNG.NET

*A fully-managed APNG (Animated Portable Network Graphics) Parser*


**Original is https://github.com/xupefei/APNG.NET**

## Difference 

* LibAPNG is **.NET Standard** libary
* Extensions for **WPF** (.NET Framework)
    * Convert each PNG frame to `BitmapSource` objects
    * Create key frame animation, So you can display APNG to `Image` control using `Storyboard`.
* Remove XNA projects
* Update to Visual Studio 2019

## Usages

### APNG to BitmapSource objects

```cs
var apng = new APNG("a.png");
var bitmaps = apng.ToBitmapSources();

// Save to PNG files
foreach (var (bitmap, index) in bitmaps.Select((item, index) => (item, index)))
{
    using (var fileStream = new FileStream($"{index}.png", FileMode.Create))
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(fileStream);
    }
}
```

### Display APNG to Image control

```xml
<Image Name="PngImage" />
```

```cs
var apng = new APNG("a.png");
if (apng.IsSimplePNG)
    PngImage.Source = BitmapFrame.Create(
        apng.DefaultImage.GetStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
else
    apng.ToAnimation().CreateStoryboardFor(PngImage).Begin(PngImage);
```
