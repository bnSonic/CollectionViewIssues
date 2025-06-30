using System;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Extended.Svg;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;
using System.Diagnostics;

namespace CollectionViewIssues.Components;

    public class SvgImage : Border
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
        public SvgImage()
        {
            Padding = new Thickness(0);
            BackgroundColor = Colors.Transparent;
            StrokeThickness = 0;

            Content = _canvasView;
            _canvasView.PaintSurface += SkiaSharpPaintSurfaceEventHandler;
        }
        #endregion END CONSTRUCTORS



        #region DEPENDENCY PROPERTIES
        public static readonly BindableProperty ResourceProperty = BindableProperty.Create(
            nameof(Resource),
            typeof(byte[]),
            typeof(SvgImage),
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
            typeof(SvgImage),
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
            typeof(SvgImage),
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
        //public static readonly BindableProperty ResourceNameProperty = BindableProperty.Create(
        //    nameof(Resource),
        //    typeof(string),
        //    typeof(SvgImage),
        //    string.Empty,
        //    propertyChanged: RedrawCanvas);

        //public string ResourceName
        //{
        //    get => (string)GetValue(ResourceNameProperty);
        //    set => SetValue(ResourceNameProperty, value);
        //}

        public static readonly BindableProperty TintColorProperty = BindableProperty.Create(
          nameof(TintColor),
          typeof(Color),
          typeof(SvgImage),
          Colors.Black,
          propertyChanged: RedrawCanvas);

        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }

        private static void RedrawCanvas(BindableObject bindable, object oldvalue, object newvalue)
        {
            SvgImage svgImage = bindable as SvgImage;

            //if (!string.IsNullOrEmpty(svgImage.ResourceName))
            //    svgImage?.TryLoadSvgResourceByName();

            try
            {
                svgImage?.LoadSvgPicture();
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            try
            {
                svgImage?._canvasView.InvalidateSurface();
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        #endregion END DEPENDENCY PROPERTIES



        #region EVENT HANDLER
        private void SkiaSharpPaintSurfaceEventHandler(object sender, SKPaintSurfaceEventArgs args) 
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            if (Resource == null)
                return;

            if (_svgPicture == null)
                return;

            SKImageInfo info = args.Info;
            canvas.Translate(info.Width / 2f, info.Height / 2f);

            SKRect bounds = _svgPicture.CullRect;
            float ratio = (bounds.Width > bounds.Height) ? (info.Width / bounds.Width) : (info.Height / bounds.Height);

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);

            if (this.TintColor == Colors.Black)
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
                        this.TintColor.ToSKColor(), //SKColors.Red,// the color, also `(SKColor)0xFFFF0000` is valid

                        SKBlendMode.SrcIn); // use the source color

                    canvas.DrawPicture(_svgPicture, paint);
                }
            }
        }
        #endregion END EVENT HANDLER



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
