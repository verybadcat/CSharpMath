using System;
using System.Drawing;

using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.Rendering;
using CSharpMath.Interfaces;
using TFonts = CSharpMath.Rendering.MathFonts;
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
      LocalTypefaces.CollectionChanged += CollectionChanged;
      ErrorColor = UnwrapColor(new Color(255, 0, 0));
      TextColor = UnwrapColor(new Color(0, 0, 0));
      HighlightColor = UnwrapColor(new Color(0, 0, 0, 0));
    }
    void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Redisplay(0);
    #endregion Constructors

    #region Fields
    //_field == private field, __field == property-only field
    protected bool _displayChanged = true;
    protected IPositionableDisplay<TFonts, Glyph> _display;
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

    public RectangleF? Measure {
      get {
        //UpdateDisplay() null-refs if MathList == null
        if (System.Collections.Generic.EqualityComparer<TSource>.Default.Equals(Source, default)) return null;
        if (!Source.IsEmpty && _displayChanged) UpdateDisplay();
        return _display?.ComputeDisplayBounds();
      }
    }

    public string ErrorMessage => Source.ErrorMessage;
    #endregion Non-redisplaying properties

    #region Redisplaying properties
    protected void Redisplay<T>(T assignment) => _displayChanged = true;
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => __size; set => Redisplay(__size = value); } float __size = 20f;
    public ObservableCollection<Typeface> LocalTypefaces { get; } = new ObservableCollection<Typeface>();
    LineStyle __style = LineStyle.Display; public LineStyle LineStyle { get => __style; set => Redisplay(__style = value); }
    TSource __source = default; public TSource Source { get => __source; set => Redisplay(__source = value); }
    #endregion Redisplaying properties

    #region Methods
    public void UpdateDisplay() {
      var fonts = new TFonts(LocalTypefaces, FontSize);
      _display = CreateDisplay(fonts);
      _displayChanged = false;
    }

    protected abstract IPositionableDisplay<TFonts, Glyph> CreateDisplay(TFonts fonts);

    public abstract Color WrapColor(TColor color);
    public abstract TColor UnwrapColor(Color color);

    public abstract ICanvas WrapCanvas(TCanvas canvas);

    protected abstract bool CoordinatesFromBottomLeftInsteadOfTopLeft { get; }

    public void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (!Source.IsEmpty) DrawError(c);
      else {
        if (_displayChanged) UpdateDisplay();
        Draw(c, IPainterExtensions.GetDisplayPosition(_display, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
      }
    }

    public void Draw(TCanvas canvas, float x, float y) =>
      Draw(WrapCanvas(canvas), new PointF(x, CoordinatesFromBottomLeftInsteadOfTopLeft ? y : -y));

    public void Draw(TCanvas canvas, PointF position) {
      if (CoordinatesFromBottomLeftInsteadOfTopLeft) position.Y *= -1;
      Draw(WrapCanvas(canvas), position);
    }

    private void Draw(ICanvas canvas, PointF position) {
      if (!Source.IsEmpty) {
        if (_displayChanged) UpdateDisplay();
        _display.SetPosition(position);
        canvas.Save();
        if (!CoordinatesFromBottomLeftInsteadOfTopLeft) {
          //invert the canvas vertically
          canvas.Scale(1, -1);
        }
        canvas.Scale(Magnification, Magnification);
        canvas.DefaultColor = WrapColor(TextColor);
        canvas.CurrentColor = WrapColor(HighlightColor);
        canvas.CurrentStyle = PaintStyle;
        var measure = Measure ?? default;
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

    private void DrawError(ICanvas canvas) {
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