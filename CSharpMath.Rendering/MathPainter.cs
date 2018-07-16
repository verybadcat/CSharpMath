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
  public abstract class MathPainter<TCanvas, TColor> : ICanvasPainter<TCanvas, MathSource, TColor> {
    #region Constructors
    public MathPainter(float fontSize = 20f) {
      FontSize = fontSize;
      LocalTypefaces.CollectionChanged += CollectionChanged;
      ErrorColor = UnwrapColor(new Color(255, 0, 0));
      TextColor = UnwrapColor(new Color(0, 0, 0));
      BackgroundColor = UnwrapColor(new Color(0, 0, 0, 0));
    }
    void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Redisplay(0);
    #endregion Constructors

    #region Fields
    //_field == private field, __field == property-only field
    protected bool _displayChanged = true;
    protected MathListDisplay<TFonts, Glyph> _displayList;
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
    public TColor BackgroundColor { get; set; }
    public (TColor glyph, TColor textRun)? GlyphBoxColor { get; set; }
    public PaintStyle PaintStyle { get; set; } = PaintStyle.Fill;
    public float Magnification { get; set; } = 1;

    public RectangleF? Measure {
      get {
        if (MathList != null && _displayList == null) UpdateDisplay();
        return _displayList?.ComputeDisplayBounds();
      }
    }

    public string ErrorMessage => Source.Error;
    #endregion Non-redisplaying properties

    #region Redisplaying properties
    protected void Redisplay<T>(T assignment) => _displayChanged = true;
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => __size; set => Redisplay(__size = value); } float __size = 20f;
    public ObservableCollection<Typeface> LocalTypefaces { get; } = new ObservableCollection<Typeface>();
    LineStyle __style = LineStyle.Display; public LineStyle LineStyle { get => __style; set => Redisplay(__style = value); }
    MathSource __source = new MathSource(); public MathSource Source { get => __source; set => Redisplay(__source = value); }
    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }
    #endregion Redisplaying properties

    #region Methods
    private static PointF GetDisplayPosition(MathListDisplay<TFonts, Glyph> displayList,
      float fontSize, bool bottomLeftCoords, 
      float width, float height,
      TextAlignment alignment, Thickness padding, float offsetX, float offsetY) {
      float x, y;
      float displayWidth = displayList.Width;
      if ((alignment & TextAlignment.Left) != 0)
        x = padding.Left;
      else if ((alignment & TextAlignment.Right) != 0)
        x = width - padding.Right - displayWidth;
      else
        x = padding.Left + (width - padding.Left - padding.Right - displayWidth) / 2;
      float contentHeight = displayList.Ascent + displayList.Descent;
      if (contentHeight < fontSize / 2) {
        contentHeight = fontSize / 2;
      }
      if (!bottomLeftCoords) {
        //Canvas is inverted!
        if ((alignment & (TextAlignment.Top | TextAlignment.Bottom)) != 0) {
          alignment ^= TextAlignment.Top;
          alignment ^= TextAlignment.Bottom;
        }
        //invert y-coordinate as canvas is inverted
        offsetY *= -1;
      }
      if ((alignment & TextAlignment.Top) != 0)
        y = padding.Top + displayList.Descent;
      else if ((alignment & TextAlignment.Bottom) != 0)
        y = height - padding.Bottom - displayList.Ascent;
      else {
        float availableHeight = height - padding.Top - padding.Bottom;
        y = ((availableHeight - contentHeight) / 2) + padding.Top + displayList.Descent;
      }
      return new PointF(x + offsetX, (y + offsetY) - (bottomLeftCoords ? 0 : height));
    }

    public void UpdateDisplay() {
      var fonts = new TFonts(LocalTypefaces, FontSize);
      _displayList = TypesettingContext.Instance.CreateLine(MathList, fonts, LineStyle);
      _displayChanged = false;
    }

    public abstract Color WrapColor(TColor color);
    public abstract TColor UnwrapColor(Color color);

    public abstract ICanvas WrapCanvas(TCanvas canvas);

    protected abstract bool CoordinatesFromBottomLeftInsteadOfTopLeft { get; }

    public void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      if (MathList == null) return;
      if (_displayChanged) UpdateDisplay();
      var c = WrapCanvas(canvas);
      Draw(c, GetDisplayPosition(_displayList, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
    }

    public void Draw(TCanvas canvas, float x, float y) =>
      Draw(WrapCanvas(canvas), new PointF(x, CoordinatesFromBottomLeftInsteadOfTopLeft ? y : -y));

    public void Draw(TCanvas canvas, PointF position) {
      if (CoordinatesFromBottomLeftInsteadOfTopLeft) position.Y *= -1;
      Draw(WrapCanvas(canvas), position);
    }

    private void Draw(ICanvas canvas, PointF position) {
      if (MathList != null) {
        canvas.Save();
        if (!CoordinatesFromBottomLeftInsteadOfTopLeft) {
          //invert the canvas vertically
          canvas.Scale(1, -1);
        }
        canvas.Scale(Magnification, Magnification);
        canvas.DefaultColor = WrapColor(TextColor);
        canvas.CurrentColor = WrapColor(BackgroundColor);
        canvas.FillColor();
        canvas.CurrentStyle = PaintStyle;
        if (_displayChanged) UpdateDisplay();
        _displayList.Position = position;
        _context.Canvas = canvas;
        T? Nullable<T>(T nonnull) where T : struct => new T?(nonnull);
        _context.GlyphBoxColor = GlyphBoxColor.HasValue ? Nullable((WrapColor(GlyphBoxColor.Value.glyph), WrapColor(GlyphBoxColor.Value.textRun))) : null;
        _displayList.Draw(_context);
        canvas.Restore();
      } else if (DisplayErrorInline && ErrorMessage.IsNonEmpty()) {
        canvas.Save();
        canvas.CurrentColor = WrapColor(BackgroundColor);
        canvas.FillColor();
        var size = ErrorFontSize ?? FontSize;
        canvas.CurrentColor = WrapColor(ErrorColor);
        canvas.FillText(ErrorMessage, 0, size, size);
        canvas.Restore();
      }
    }
    #endregion Methods
  }
}