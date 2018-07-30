using System;
using System.Drawing;

using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.Rendering;
using CSharpMath.Interfaces;
using TFonts = CSharpMath.Rendering.Fonts;
using CSharpMath.FrontEnd;
using CSharpMath.Structures;

using Typography.OpenFont;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CSharpMath.Rendering {
  public abstract class Painter<TCanvas, TSource, TColor> : ICanvasPainter<TCanvas, TSource, TColor> where TSource : struct, ISource {
    public const float DefaultFontSize = 20f;
    
    #region Constructors
    public Painter(float fontSize = DefaultFontSize) {
      FontSize = fontSize;
      LocalTypefaces.CollectionChanged += TypefacesChanged;
      ErrorColor = UnwrapColor(new Color(255, 0, 0));
      TextColor = UnwrapColor(new Color(0, 0, 0));
      HighlightColor = UnwrapColor(new Color(0, 0, 0, 0));
    }
    #endregion Constructors

    #region Non-redisplaying properties
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get; set; }
    public bool DisplayErrorInline { get; set; } = true;
    public TColor ErrorColor { get; set; }
    public TColor TextColor { get; set; }
    public TColor HighlightColor { get; set; }
    public (TColor glyph, TColor textRun)? GlyphBoxColor { get; set; }
    public PaintStyle PaintStyle { get; set; } = PaintStyle.Fill;
    public float Magnification { get; set; } = 1;

    public string ErrorMessage => Source.ErrorMessage;
    #endregion Non-redisplaying properties

    #region Redisplaying properties
    //_field == private field, __field == property-only field
    protected abstract void SetRedisplay();
    protected TFonts Fonts { get; private set; } = new TFonts(Array.Empty<Typeface>(), DefaultFontSize);
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => Fonts.PointSize; set { Fonts = new TFonts(Fonts, value); SetRedisplay(); } }
    public ObservableRangeCollection<Typeface> LocalTypefaces { get; } = new ObservableRangeCollection<Typeface>();
    void TypefacesChanged(object sender, NotifyCollectionChangedEventArgs e) { Fonts = new TFonts(LocalTypefaces, FontSize); SetRedisplay(); }
    LineStyle __style = LineStyle.Display; public LineStyle LineStyle { get => __style; set { __style = value; SetRedisplay(); } }
    TSource __source = default; public TSource Source { get => __source; set { __source = value; SetRedisplay(); } }
    #endregion Redisplaying properties

    protected abstract bool CoordinatesFromBottomLeftInsteadOfTopLeft { get; }

    #region Methods

    public abstract Color WrapColor(TColor color);
    public abstract TColor UnwrapColor(Color color);

    public abstract ICanvas WrapCanvas(TCanvas canvas);

    public abstract void Draw(TCanvas canvas, TextAlignment alignment, Thickness padding = default, float offsetX = 0, float offsetY = 0);

    protected abstract void UpdateDisplay(float canvasWidth);

    protected abstract RectangleF? MeasureCore(float canvasWidth);
   
    protected void Draw(ICanvas canvas, IDisplay<TFonts, Glyph> display, PointF? position = null) {
      if (Source.IsValid) {
        if(position != null) display.Position = position.Value;
        canvas.Save();
        if (!CoordinatesFromBottomLeftInsteadOfTopLeft) {
          //invert the canvas vertically
          canvas.Scale(1, -1);
        }
        canvas.Scale(Magnification, Magnification);
        canvas.DefaultColor = WrapColor(TextColor);
        canvas.CurrentColor = WrapColor(HighlightColor);
        canvas.CurrentStyle = PaintStyle;
        var measure = MeasureCore(canvas.Width) ??
          throw new InvalidOperationException($"{nameof(MeasureCore)} returned null. Any conditions leading to this should have already been checked via {nameof(Source)}.{nameof(Source.IsValid)}.");
        canvas.FillRect(display.Position.X + measure.X, display.Position.Y -
          (CoordinatesFromBottomLeftInsteadOfTopLeft ? display.Ascent : display.Descent),
          measure.Width, measure.Height);
        canvas.CurrentColor = null;
        T? Nullable<T>(T nonnull) where T : struct => new T?(nonnull);
        display.Draw(new GraphicsContext {
          Canvas = canvas,
          GlyphBoxColor = GlyphBoxColor.HasValue ? Nullable((WrapColor(GlyphBoxColor.Value.glyph), WrapColor(GlyphBoxColor.Value.textRun))) : null
        });
        canvas.Restore();
      } else DrawError(canvas);
    }
    protected void DrawError(ICanvas canvas) {
      if (DisplayErrorInline && ErrorMessage.IsNonEmpty()) {
        canvas.Save();
        var size = ErrorFontSize ?? FontSize;
        canvas.CurrentColor = WrapColor(ErrorColor);
        canvas.FillText(ErrorMessage, 0, size, size);
        canvas.Restore();
      }
    }
    #endregion Methods
  }
}