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

    #region Fields
    //_field == private field, __field == property-only field
    protected bool _displayChanged = true;
    protected IDisplay<TFonts, Glyph> _display;
    protected readonly GraphicsContext _context = new GraphicsContext();
    #endregion Fields

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
    protected void Redisplay<T>(T assignment) => _displayChanged = true;
    protected TFonts Fonts { get; private set; } = new TFonts(Array.Empty<Typeface>(), DefaultFontSize);
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => Fonts.PointSize; set => Redisplay(Fonts = new TFonts(Fonts, value)); }
    public ObservableRangeCollection<Typeface> LocalTypefaces { get; } = new ObservableRangeCollection<Typeface>();
    void TypefacesChanged(object sender, NotifyCollectionChangedEventArgs e) => Redisplay(Fonts = new TFonts(LocalTypefaces, FontSize));
    LineStyle __style = LineStyle.Display; public LineStyle LineStyle { get => __style; set => Redisplay(__style = value); }
    TSource __source = default; public TSource Source { get => __source; set => Redisplay(__source = value); }
    #endregion Redisplaying properties

    protected abstract bool CoordinatesFromBottomLeftInsteadOfTopLeft { get; }

    #region Methods

    public abstract Color WrapColor(TColor color);
    public abstract TColor UnwrapColor(Color color);

    public abstract ICanvas WrapCanvas(TCanvas canvas);

    protected void UpdateDisplay(float canvasWidth) {
      _display = CreateDisplay(canvasWidth);
      _displayChanged = false;
    }
    protected abstract IDisplay<TFonts, Glyph> CreateDisplay(float canvasWidth);

    protected RectangleF? MeasureCore(float canvasWidth) {
      //UpdateDisplay() null-refs if MathList == null
      if (!Source.IsValid) return null;
      if (Source.IsValid && _displayChanged) UpdateDisplay(canvasWidth);
      return _display?.ComputeDisplayBounds();
    }

    protected void Draw(ICanvas canvas, PointF position) {
      if (Source.IsValid) {
        if (_displayChanged) UpdateDisplay(canvas.Width);
        _display.Position = position;
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
        canvas.FillRect(position.X + measure.X, position.Y -
          (CoordinatesFromBottomLeftInsteadOfTopLeft ? _display.Ascent : _display.Descent),
          measure.Width, measure.Height);
        canvas.CurrentColor = null;
        _context.Canvas = canvas;
        T? Nullable<T>(T nonnull) where T : struct => new T?(nonnull);
        _context.GlyphBoxColor = GlyphBoxColor.HasValue ? Nullable((WrapColor(GlyphBoxColor.Value.glyph), WrapColor(GlyphBoxColor.Value.textRun))) : null;
        _display.Draw(_context);
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