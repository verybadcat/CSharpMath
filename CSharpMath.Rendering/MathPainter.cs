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
  public abstract class MathPainter<TCanvas> : IPainter<TCanvas, MathSource, Color> {
    #region Constructors
    public MathPainter(float fontSize = 20f) {
      FontSize = fontSize;
      LocalTypefaces.CollectionChanged += CollectionChanged;
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
    public Color ErrorColor { get; set; } = new Color(255, 0, 0);
    public Color TextColor { get; set; } = new Color(0, 0, 0);
    public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0);
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
    (Color glyph, Color textRun)? __box; public (Color glyph, Color textRun)? GlyphBoxColor { get => __box; set => Redisplay(__box = value); }
    MathSource __source = new MathSource(); public MathSource Source { get => __source; set => Redisplay(__source = value); }
    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }
    #endregion Redisplaying properties

    #region Methods
    private static PointF GetDisplayPosition(MathListDisplay<TFonts, Glyph> displayList,
      TextAlignment alignment, float fontSize,
      float width, float height, Thickness padding, float offsetX, float offsetY) {
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
      //Canvas is inverted!
      if ((alignment & TextAlignment.Top) != 0)
        y = height - padding.Bottom - displayList.Ascent;
      else if ((alignment & TextAlignment.Bottom) != 0)
        y = padding.Top + displayList.Descent;
      else {
        float availableHeight = height - padding.Top - padding.Bottom;
        y = ((availableHeight - contentHeight) / 2) + padding.Top + displayList.Descent;
      }
      return new PointF(x + offsetX, y + offsetY);
    }

    public void UpdateDisplay() {
      var fonts = new TFonts(LocalTypefaces, FontSize);
      _displayList = TypesettingContext.Instance.CreateLine(MathList, fonts, LineStyle);
      _displayChanged = false;
    }

    public abstract ICanvas CreateCanvasWrapper(TCanvas canvas);

    public void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      if (MathList == null) return;
      if (_displayChanged) UpdateDisplay();
      var c = CreateCanvasWrapper(canvas);
      //invert y-coordinate as canvas is inverted
      Draw(c, GetDisplayPosition(_displayList, alignment, FontSize, c.Width, c.Height, padding, offsetX, -offsetY));
    }

    public void Draw(TCanvas canvas, float x, float y) =>
      //invert y-coordinate as canvas is inverted
      Draw(CreateCanvasWrapper(canvas), new PointF(x, -y));

    public void Draw(TCanvas canvas, PointF position)
    {
      position.Y *= -1;
      //invert y-coordinate as canvas is inverted
      Draw(CreateCanvasWrapper(canvas), position);
    }

    private void Draw(ICanvas canvas, PointF position) {
      if (MathList != null) {
        canvas.Save();
        //invert the canvas vertically
        canvas.Scale(1, -1);
        //canvas is inverted so negate vertical position
        canvas.Translate(0, -canvas.Height);
        canvas.Scale(Magnification, Magnification);
        canvas.DefaultColor = TextColor;
        canvas.CurrentColor = BackgroundColor;
        canvas.FillColor();
        canvas.CurrentStyle = PaintStyle;
        if (_displayChanged) UpdateDisplay();
        _displayList.Position = position;
        _context.Canvas = canvas;
        _context.GlyphBoxColor = GlyphBoxColor;
        _displayList.Draw(_context);
        canvas.Restore();
      } else if (DisplayErrorInline && ErrorMessage.IsNonEmpty()) {
        canvas.Save();
        canvas.CurrentColor = BackgroundColor;
        canvas.FillColor();
        var size = ErrorFontSize ?? FontSize;
        canvas.CurrentColor = ErrorColor;
        canvas.FillText(ErrorMessage, 0, size, size);
        canvas.Restore();
      }
    }
    #endregion Methods
  }
}