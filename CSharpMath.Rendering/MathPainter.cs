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
  public class MathPainter : IPainter<MathSource, Color> {
    #region Constructors
    public MathPainter(float width, float height, float fontSize = 20f) : this(new SizeF(width, height), fontSize) { }
    public MathPainter(SizeF bounds, float fontSize = 20f) {
      Bounds = bounds;
      FontSize = fontSize;
      LocalTypefaces.CollectionChanged += CollectionChanged;
    }
    void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Redisplay(0);
    #endregion Constructors

    #region Fields
    //_field == private field, __field == property-only field
    protected bool _displayChanged = true;
    protected MathListDisplay<TFonts, Glyph> _displayList;
    protected GraphicsContext _context = new GraphicsContext();
    #endregion Fields

    #region Non-redisplaying properties
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get; set; }
    public bool DisplayErrorInline { get; set; } = true;
    public Color ErrorColor { get; set; } = new Color(255, 0, 0);
    public SizeF Bounds { get; set; }
    public Thickness Padding { get; set; }
    public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;
    public Color TextColor { get; set; } = new Color(0, 0, 0);
    public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0);
    public PaintStyle PaintStyle { get; set; } = PaintStyle.Fill;
    public float DisplacementX { get; set; }
    public float DisplacementY { get; set; }
    public float Magnification { get; set; } = 1;

    public SizeF? DrawingSize {
      get {
        if (MathList != null && _displayList == null) UpdateDisplay();
        return _displayList?.ComputeDisplayBounds().Size + new SizeF(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);
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
    private PointF GetDisplayPosition() {
      float x, y;
      float displayWidth = _displayList.Width;
      if ((TextAlignment & TextAlignment.Left) != 0)
        x = Padding.Left;
      else if ((TextAlignment & TextAlignment.Right) != 0)
        x = Bounds.Width - Padding.Right - displayWidth;
      else
        x = Padding.Left + (Bounds.Width - Padding.Left - Padding.Right - displayWidth) / 2;
      float contentHeight = _displayList.Ascent + _displayList.Descent;
      if (contentHeight < FontSize / 2) {
        contentHeight = FontSize / 2;
      }
      //Canvas is inverted!
      if ((TextAlignment & TextAlignment.Top) != 0)
        y = Bounds.Height - Padding.Bottom - _displayList.Ascent;
      else if ((TextAlignment & TextAlignment.Bottom) != 0)
        y = Padding.Top + _displayList.Descent;
      else {
        float availableHeight = Bounds.Height - Padding.Top - Padding.Bottom;
        y = ((availableHeight - contentHeight) / 2) + Padding.Top + _displayList.Descent;
      }
      return new PointF(x, y);
    }

    public void UpdateDisplay() {
      var fonts = new TFonts(LocalTypefaces, FontSize);
      _displayList = TypesettingContext.Instance.CreateLine(MathList, fonts, LineStyle);
      _displayChanged = false;
    }

    public void Draw(ICanvas canvas) {
      if (MathList != null) {
        canvas.Save();
        //invert the canvas vertically
        canvas.Scale(1, -1);
        canvas.Translate(0, -Bounds.Height);
        canvas.Scale(Magnification, Magnification);
        canvas.DefaultColor = TextColor;
        canvas.CurrentColor = BackgroundColor;
        canvas.FillColor();
        canvas.CurrentStyle = PaintStyle;
        if (_displayChanged) UpdateDisplay();
        _displayList.Position = GetDisplayPosition();
        //canvas is inverted so negate vertical translation
        canvas.Translate(DisplacementX, -DisplacementY);
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
      #endregion Methods
    }
  }
}