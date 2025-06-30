using System;
using System.IO;
using System.Reflection;
using SkiaSharp;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace CollectionViewIssues.Components;

public class SvgImageEx : Border
    {
        #region FIELDS
        private SKCanvasView _canvasView = new SKCanvasView();
        //private string _resourceName = string.Empty;

        private SKPicture _svgPicture;
        #endregion END FIELDS



        #region CONSTRUCTORS
        /// <summary>
        /// 
        /// </summary>
        public SvgImageEx()
        {
            Padding = new Thickness(0);
            BackgroundColor = Colors.Transparent;
            StrokeThickness = 0;

            Content = _canvasView;
            _canvasView.PaintSurface += SkiaSharpPaintSurfaceEventHandler;
        }
        #endregion END CONSTRUCTORS

        #region EXTENDED PROPERTIES
        public enum SvgImageExShape { None, Circle }

        public static readonly BindableProperty ShapeProperty = BindableProperty.Create(
            nameof(Shape),
            typeof(SvgImageExShape),
            typeof(SvgImageEx),
            SvgImageExShape.None,
            propertyChanged: RedrawCanvas);

        public SvgImageExShape Shape
        {
            get => (SvgImageExShape)GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        public static readonly BindableProperty ShapeBackgroundColorProperty = BindableProperty.Create(
            nameof(ShapeBackgroundColor),
            typeof(Color),
            typeof(SvgImageEx),
            Colors.Transparent,
            propertyChanged: RedrawCanvas);

        public Color ShapeBackgroundColor
        {
            get => (Color)GetValue(ShapeBackgroundColorProperty);
            set => SetValue(ShapeBackgroundColorProperty, value);
        }

        public static readonly BindableProperty ShapeBorderColorProperty = BindableProperty.Create(
            nameof(ShapeBorderColor),
            typeof(Color),
            typeof(SvgImageEx),
            Colors.Transparent,
            propertyChanged: RedrawCanvas);

        public Color ShapeBorderColor
        {
            get => (Color)GetValue(ShapeBorderColorProperty);
            set => SetValue(ShapeBorderColorProperty, value);
        }

        public static readonly BindableProperty ShapeBorderSizeProperty = BindableProperty.Create(
            nameof(ShapeBorderSize),
            typeof(double),
            typeof(SvgImageEx),
            0.0,
            propertyChanged: RedrawCanvas);

        public double ShapeBorderSize
        {
            get => (double)GetValue(ShapeBorderSizeProperty);
            set => SetValue(ShapeBorderSizeProperty, value);
        }

        public static readonly BindableProperty ShapePaddingProperty = BindableProperty.Create(
            nameof(ShapePadding),
            typeof(double),
            typeof(SvgImageEx),
            0.0,
            propertyChanged: RedrawCanvas);

        public double ShapePadding
        {
            get => (double)GetValue(ShapePaddingProperty);
            set => SetValue(ShapePaddingProperty, value);
        }
        #endregion


        #region DEPENDENCY PROPERTIES
        public static readonly BindableProperty ResourceProperty = BindableProperty.Create(
            nameof(Resource),
            typeof(byte[]),
            typeof(SvgImageEx),
            default(byte[]),
            propertyChanged: RedrawCanvas);

        /// <summary>
        /// Ein SVG-Bild das gezeigt werden soll wenn IsSelected auf False gesetzt ist
        /// Bzw. das Standard-Bild was per default angezeigt wird (ResourceSelected muss ja nicht gesetzt werden
        /// </summary>
        public byte[] Resource
        {
            get => (byte[])GetValue(ResourceProperty);
            set => SetValue(ResourceProperty, value);
        }

        public static readonly BindableProperty ResourceSelectedProperty = BindableProperty.Create(
            nameof(ResourceSelected),
            typeof(byte[]),
            typeof(SvgImageEx),
            default(byte[]),
            propertyChanged: RedrawCanvas);

        /// <summary>
        /// ein SVG-Bild das gezeigt werden soll, wenn IsSelected auf True gesetzt wird
        /// </summary>
        public byte[] ResourceSelected
        {
            get => (byte[])GetValue(ResourceSelectedProperty);
            set => SetValue(ResourceSelectedProperty, value);
        }

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(SvgImageEx),
            false,
            propertyChanged: RedrawCanvas);

        /// <summary>
        /// am image kann ein Selected-Status true/false verwaltet werden; kann zusammen mit ResourceSelected genutzt werden,
        /// damit das image ein anderes Bild zeigt, wenn das image als selektiert markiert wird (z.B. für selbstgebaute Tabs
        /// um zu visualisieren, welcher tab gerade aktiv ist)
        /// </summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly BindableProperty TintColorProperty = BindableProperty.Create(
          nameof(TintColor),
          typeof(Color),
          typeof(SvgImageEx),
          Colors.Black,
          propertyChanged: RedrawCanvas);

        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }

        private static void RedrawCanvas(BindableObject bindable, object oldvalue, object newvalue)
        {
            SvgImageEx svgImage = bindable as SvgImageEx;

            //if (!string.IsNullOrEmpty(svgImage.ResourceName))
            //    svgImage?.TryLoadSvgResourceByName();

            try
            {
                svgImage?.LoadSvgPicture();
            }
            catch (Exception ex)
            {
                Debugger.Break();
                //ex.Trace("SvgImageEx.cs LoadSvgPicture: Mit Absicht abgefangen; siehe Kommentare im Code");
            }

            try
            {
                svgImage?._canvasView.InvalidateSurface();
            }
            catch (Exception ex)
            {
                Debugger.Break();
                //ex.Trace("SvgImageEx.cs InvalidateSurface: Mit Absicht abgefangen; siehe Kommentare im Code");
            }
        }

        #endregion END DEPENDENCY PROPERTIES



        #region EVENT HANDLER

        private float _resolutionFactor = 1;
        float DeviceValue(float independentValue)
        {
            return independentValue * _resolutionFactor;
        }

        private void SkiaSharpPaintSurfaceEventHandler(object sender, SKPaintSurfaceEventArgs args) 
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            if (Resource == null)
                return;

            if (_svgPicture == null)
                return;

            SKImageInfo info = args.Info;

            //-- Um Auflösungsunabhängig zeichnen zu können, müssen wir einen Faktor für die aktuelle Auflösung ermitteln
            _resolutionFactor = info.Width / (float)this.Width;

            //canvas.Translate(info.Width / 2f, info.Height / 2f);

            //SKRect bounds = _svgPicture.CullRect;
            //float ratio = (bounds.Width > bounds.Height) ? (info.Width / bounds.Width) : (info.Height / bounds.Height);

            //canvas.Scale(ratio);
            //canvas.Translate(-bounds.MidX, -bounds.MidY);

            if (this.TintColor == Colors.Black)
            {
                if (Shape == SvgImageExShape.None)
                {
                    canvas.Translate(info.Width / 2f, info.Height / 2f);

                    SKRect bounds = _svgPicture.CullRect;
                    float ratio = (bounds.Width > bounds.Height) ? (info.Width / bounds.Width) : (info.Height / bounds.Height);

                    canvas.Scale(ratio);
                    canvas.Translate(-bounds.MidX, -bounds.MidY);

                    //-- einfach das SVG so zeichnen wie es in der Datei steht (Originale Farben)
                    canvas.DrawPicture(_svgPicture);
                }
                else if (Shape == SvgImageExShape.Circle)
                {
                    //-- Hintergrund zeichnen (Kreis mit optionalem Rand)
                    DrawBackground_Circle(args);

                    //-- Größes des Bildes in der Mitte ermitteln
                    //   = gedachter innerer Kreis in dem ein passendes Quadrat berechnet wird
                    var rect = GetCenterRect(args);

                    //-- Das Svg-Picture in die Mitte zeichnen
                    DrawPicture(args, _svgPicture, rect);
                }
            }
            else
            {
                if (Shape == SvgImageExShape.None)
                {
                    canvas.Translate(info.Width / 2f, info.Height / 2f);

                    SKRect bounds = _svgPicture.CullRect;
                    float ratio = (bounds.Width > bounds.Height) ? (info.Width / bounds.Width) : (info.Height / bounds.Height);

                    canvas.Scale(ratio);
                    canvas.Translate(-bounds.MidX, -bounds.MidY);

                    //-- SVG mit der TintColor einfärben (Default ist schwarz)
                    using (var paint = new SKPaint())
                    {
                        paint.ColorFilter = SKColorFilter.CreateBlendMode(
                            this.TintColor.ToSKColor(), //SKColors.Red,// the color, also `(SKColor)0xFFFF0000` is valid

                            SKBlendMode.SrcIn); // use the source color

                        canvas.DrawPicture(_svgPicture, paint);
                    }
                }

            }
        }
        #endregion END EVENT HANDLER

        #region ZEICHENROUTINEN
        void DrawBackground_Circle(SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            float width = info.Width;
            float height = DeviceValue((float)this.Height); //info.Height;

            //-- Gesamt-Maße
            float x = 0.0f;
            float y = 0.0f;
            float w = width;
            float h = height;

            //-- Hintergrund Shape
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0,
                StrokeJoin = SKStrokeJoin.Miter,
                Color = ShapeBackgroundColor.ToSKColor(),
                IsAntialias = true,
            })
            {
                canvas.DrawCircle(w / 2.0f, h / 2.0f, w / 2.0f, paint);
            }

            //-- Hintergrund Shape Umrandung
            if (ShapeBorderSize > 0)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = DeviceValue((float)ShapeBorderSize),
                    StrokeJoin = SKStrokeJoin.Miter,
                    IsStroke = true,
                    Color = ShapeBorderColor.ToSKColor(),
                    IsAntialias = true,
                })
                {
                    float r = (w / 2.0f) - (paint.StrokeWidth / 2.0f);
                    canvas.DrawCircle(w / 2.0f, h / 2.0f, r, paint);
                }
            }

        }
        SKRect GetCenterRect(SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;

            float width = info.Width;
            float height = DeviceValue((float)this.Height); //info.Height;

            float radiusAussen = width / 2.0f;
            float radiusInnen = radiusAussen - DeviceValue((float)ShapeBorderSize) - DeviceValue((float)ShapePadding);

            float a2 = radiusInnen * radiusInnen;
            float b2 = radiusInnen * radiusInnen;
            float c2 = a2 + b2;

            float c = (float)Math.Sqrt(c2);

            float x = (info.Width - c) / 2.0f;
            float y = (info.Height - c) / 2.0f;
            float x2 = x + c;
            float y2 = y + c;

            SKRect rect = new SKRect(x, y, x2, y2);
            return rect;
        }
        void DrawPicture(SKPaintSurfaceEventArgs args, SKPicture picture, SKRect rect)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            float width = info.Width;
            float height = DeviceValue((float)this.Height); //info.Height;

            var bmp = GetBitmapFromSvg((int)rect.Width, (int)rect.Height);

            canvas.DrawBitmap(bmp, rect.Left, rect.Top);
            //canvas.DrawImage(img, rect.Left, rect.Top);
            //canvas.DrawPicture(_svgPicture);

            //using (var paint = new SKPaint
            //{
            //    Style = SKPaintStyle.Stroke,
            //    StrokeWidth = DeviceValue((float)ShapeBorderSize),
            //    StrokeJoin = SKStrokeJoin.Miter,
            //    Color = ShapeBorderColor.ToSKColor(),
            //    IsAntialias = true,
            //})
            //{
            //    canvas.DrawCircle(w / 2.0f, h / 2.0f, w / 2.0f, paint);
            //}

        }
        SKBitmap GetBitmapFromSvg(int width, int height)
        {
            SKBitmap bitmap = new SKBitmap(width, height);

            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear();

                canvas.Translate(width / 2f, height / 2f);

                SKRect bounds = _svgPicture.CullRect;
                float ratio = (bounds.Width > bounds.Height) ? (width / bounds.Width) : (height / bounds.Height);

                canvas.Scale(ratio);
                canvas.Translate(-bounds.MidX, -bounds.MidY);

                if (TintColor == Colors.Black)
                {
                    //-- einfach das SVG so zeichnen wie es in der Datei steht (Originale Farben)
                    canvas.DrawPicture(_svgPicture);
                }
                else
                {
                    //-- SVG mit der TintColor einfärben (Default ist schwarz)
                    using (var paint = new SKPaint())
                    {
                        paint.ColorFilter = SKColorFilter.CreateBlendMode(
                            TintColor.ToSKColor(), //SKColors.Red,// the color, also `(SKColor)0xFFFF0000` is valid

                            SKBlendMode.SrcIn); // use the source color

                        canvas.DrawPicture(_svgPicture, paint);
                    }
                }

                //var data = bitmap.Encode(SKEncodedImageFormat.Png, 80);
                //var imageSource = ImageSource.FromStream(() => data.AsStream());

                //return imageSource;
                return bitmap;
            }
        }
        #endregion

        #region METHODS (PRIVATE)
        /// <summary>
        /// Lädt die SVG-Datei und weist es dem privaten Feld <seealso cref="_svgPicture"/> zu.
        /// </summary>
        private void LoadSvgPicture()
        {
            //-- Bild abhängig vom IsSelected Status laden
            byte[] data = Resource; //Default das normale Bild
            if (IsSelected && ResourceSelected != null && ResourceSelected.Length > 0)
                data = ResourceSelected; //selekted-Bild wenn eines vorhanden ist

            
            if (data == null || data.Length == 0)
                return;

            using (Stream stream = new MemoryStream(data))
            {
                SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();
                svg.Load(stream);

                _svgPicture = svg.Picture;
            }
        }

        public byte[] TryLoadSvgResourceByName(string name)
        {
            //string name = this.ResourceName;
            //if (string.IsNullOrEmpty(name))
            //{
            //    _resourceName = string.Empty;
            //    this.Resource = default(byte[]);
            //    return null;
            //}
            ////-- wenn wir die Ressource schon einmal geladen haben, dann nicht noch mal laden sondern das Array zurück liefern
            //if (string.Compare(this.ResourceName, _resourceName) == 0)
            //{
            //    return this.Resource;
            //}

            //-- wir haben die Ressource noch nicht geladen, dann jetzt suchen
            Assembly AssemblyCache = Assembly.GetCallingAssembly();
            if (AssemblyCache == null)
                return null;

            string svgName = System.IO.Path.HasExtension(name) ? System.IO.Path.ChangeExtension(name, ".svg") : name + ".svg";

            string realRessource =
                AssemblyCache.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(svgName, StringComparison.CurrentCultureIgnoreCase));
            if (string.IsNullOrEmpty(realRessource))
                return null;

            var stream = AssemblyCache.GetManifestResourceStream(realRessource);
            if (stream == null)
                return null;

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            stream.CopyTo(ms);
            byte[] result = ms.ToArray();

            stream.Close();
            stream.Dispose();
            ms.Close();
            ms.Dispose();

            //_resourceName = this.ResourceName; //Name Merken, damit wir nächstes Mal das gelesene Array zurück liefern (Performance)
            //this.Resource = result;

            return result;
        }
        #endregion END METHODS (PRIVATE)
    }