using CSharpMath.Atom;
using CSharpMath.Rendering.FrontEnd;
using TextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;

namespace CSharpMath.Maui.Example {
  public partial class ExamplesPage : ContentPage {
    static Dictionary<byte, MathView> demoLabels { get; } = new();
    static Dictionary<byte, MathView> labels { get; } = new();
    public ExamplesPage() {
      InitializeComponent();
      var mathViews = demoLabels.Concat(labels).Select(p => p.Value);
      foreach (var view in mathViews) {
        view.ErrorFontSize = view.FontSize * 0.8f;
        Stack.Children.Add(view);
      }

      var values = typeof(TextAlignment).GetEnumValues();
      Array.Reverse(values);
      alignment.ItemsSource = values;
      alignment.SelectedIndexChanged += (sender, e) => {
        foreach (var view in mathViews) view.TextAlignment = (TextAlignment)alignment.SelectedItem;
      };
      alignment.SelectedItem = TextAlignment.Top;

      paintStyle.ItemsSource = typeof(PaintStyle).GetEnumValues();
      paintStyle.SelectedIndexChanged += (sender, e) => {
        foreach (var view in mathViews) view.PaintStyle = (PaintStyle)paintStyle.SelectedItem;
      };
      paintStyle.SelectedItem = PaintStyle.Fill;

      lineStyle.ItemsSource = typeof(LineStyle).GetEnumValues();
      lineStyle.SelectedIndexChanged += (sender, e) => {
        foreach (var view in mathViews) view.LineStyle = (LineStyle)lineStyle.SelectedItem;
      };
      lineStyle.SelectedItem = LineStyle.Display;

      drawGlyphBoxes.Toggled += (sender, e) => {
        foreach (var view in mathViews)
          view.GlyphBoxColor = e.Value ? (Colors.Red, Colors.Blue) : null;
      };
      highlight.Toggled += (sender, e) => {
        foreach (var view in mathViews)
          view.HighlightColor = e.Value ? Colors.Yellow : Colors.Transparent;
      };
    }
    static ExamplesPage() {
      // From https://github.com/kostub/iosMath/blob/master/iosMathExample/example/ViewController.m
      //  Demo formulae

      //  Quadratic formula
      demoLabels[0] = new MathView {
        LaTeX = @"\text{ваш вопрос: }x = \frac{-b \pm \sqrt{b^2-4ac}}{2a}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[0].FontSize = 15;

      //  This is first label so set the height from the top
      demoLabels[1] = new MathView {
        LaTeX = @"\color{#ff3399}{(a_1+a_2)^2}=a_1^2+2a_1a_2+a_2^2",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[2] = new MathView {
        LaTeX = @"\cos(\theta + \varphi) = 
                                 \cos(\theta)\cos(\varphi) - \sin(\theta)\sin(\varphi)",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[3] = new MathView {
        LaTeX = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}} 
                                 = 1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }",
        HeightRequest = 150,
        FontSize = 22.5f
      };
      demoLabels[4] = new MathView {
        LaTeX = @"\sigma = \sqrt{\frac{1}{N}\sum_{i=1}^N (x_i - \mu)^2}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[5] = new MathView {
        LaTeX = @"\neg(P\land Q) \iff (\neg P)\lor(\neg Q)",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[6] = new MathView {
        LaTeX = @"\log_b(x) = \frac{\log_a(x)}{\log_a(b)}",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[7] = new MathView {
        LaTeX = @"\lim_{x\to\infty}\left(1 + \frac{k}{x}\right)^x = e^k",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[8] = new MathView {
        LaTeX = @"\int_{-\infty}^\infty \! e^{-x^2} dx = \sqrt{\pi}",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[9] = new MathView {
        LaTeX = @"\frac 1 n \sum_{i=1}^{n}x_i \geq \sqrt[n]{\prod_{i=1}^{n}x_i}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[10] = new MathView {
        LaTeX = @"f^{(n)}(z_0) = \frac{n!}{2\pi i}\oint_\gamma\frac{f(z)}{(z-z_0)^{n+1}}\,dz",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[11] = new MathView {
        LaTeX = @"i\hbar\frac{\partial}{\partial t}\mathbf\Psi(\mathbf{x},t) = 
                           -\frac{\hbar}{2m}\nabla^2\mathbf\Psi(\mathbf{x},t) + 
                           V(\mathbf{x})\mathbf\Psi(\mathbf{x},t)",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      demoLabels[12] = new MathView {
        LaTeX = @"\left(\sum_{k=1}^n a_k b_k \right)^2 \le \left(\sum_{k=1}^n a_k^2\right)\left(\sum_{k=1}^n b_k^2\right)",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[13] = new MathView {
        LaTeX = @"{n \brace k} = \frac{1}{k!}\sum_{j=0}^k (-1)^{k-j}\binom{k}{j}(k-j)^n",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[14] = new MathView {
        LaTeX = @"f(x) = \int\limits_{-\infty}^\infty\!\hat f(\xi)\,e^{2 \pi i \xi x}\,\mathrm{d}\xi",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[15] = new MathView {
        LaTeX = @"\begin{gather}
                           \dot{x} = \sigma(y-x) \\
                           \dot{y} = \rho x - y - xz \\
                           \dot{z} = -\beta z + xy
                           \end{gather}",
        HeightRequest = 131.25,
        FontSize = 22.5f
      };
      demoLabels[16] = new MathView {
        LaTeX = @"\vec \bf V_1 \times \vec \bf V_2 =  \begin{vmatrix}
                           \hat \imath &\hat \jmath &\hat k \\
                           \frac{\partial X}{\partial u} &  \frac{\partial Y}{\partial u} & 0 \\
                           \frac{\partial X}{\partial v} &  \frac{\partial Y}{\partial v} & 0
                           \end{vmatrix}",
        HeightRequest = 131.25,
        FontSize = 22.5f
      };
      demoLabels[17] = new MathView {
        LaTeX = @"\begin{eqalign}
                           \nabla \cdot \vec{\bf{E}} & = \frac {\rho} {\varepsilon_0} \\
                           \nabla \cdot \vec{\bf{B}} & = 0 \\
                           \nabla \times \vec{\bf{E}} &= - \frac{\partial\vec{\bf{B}}}{\partial t} \\
                           \nabla \times \vec{\bf{B}} & = \mu_0\vec{\bf{J}} + \mu_0\varepsilon_0 \frac{\partial\vec{\bf{E}}}{\partial t}
                           \end{eqalign}",
        HeightRequest = 262.5,
        FontSize = 22.5f
      };
      demoLabels[18] = new MathView {
        LaTeX = @"\begin{pmatrix}
                           a & b\\ c & d
                           \end{pmatrix}
                           \begin{pmatrix}
                           \alpha & \beta \\ \gamma & \delta
                           \end{pmatrix} = 
                           \begin{pmatrix}
                           a\alpha + b\gamma & a\beta + b \delta \\
                           c\alpha + d\gamma & c\beta + d \delta 
                           \end{pmatrix}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[19] = new MathView {
        LaTeX = @"\frak Q(\lambda,\hat{\lambda}) = 
                           -\frac{1}{2} \mathbb P(O \mid \lambda ) \sum_s \sum_m \sum_t \gamma_m^{(s)} (t) +\\ 
                           \quad \left( \log(2 \pi ) + \log \left| \cal C_m^{(s)} \right| + 
                           \left( o_t - \hat{\mu}_m^{(s)} \right) ^T \cal C_m^{(s)-1} \right)",
        HeightRequest = 168.75,
        FontSize = 22.5f
      };
      demoLabels[20] = new MathView {
        LaTeX = @"f(x) = \begin{cases}
                           \frac{e^x}{2} & x \geq 0 \\
                           1 & x < 0
                           \end{cases}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      demoLabels[21] = new MathView {
        LaTeX = @"\color{#ff3333}{c}\color{#9933ff}{o}\color{#ff0080}{l}+\color{#99ff33}{\frac{\color{#ff99ff}{o}}{\color{#990099}{r}}}-\color{#33ffff}{\sqrt[\color{#3399ff}{e}]{\color{#3333ff}{d}}}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Test formulae
      labels[0] = new MathView {
        LaTeX = @"3+2-5 = 0",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      labels[0].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);

      //  Infix and prefix Operators
      labels[1] = new MathView {
        LaTeX = @"12+-3 > +14",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      labels[1].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      labels[1].TextAlignment = TextAlignment.Center;

      //  Punct, parens
      labels[2] = new MathView {
        LaTeX = @"(-3-5=-8, -6-7=-13)",
        HeightRequest = 75,
        FontSize = 22.5f
      };

      //  Latex commands
      labels[3] = new MathView {
        LaTeX = @"5\times(-2 \div 1) = -10",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      labels[3].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      labels[3].TextAlignment = TextAlignment.Right;
      labels[4] = new MathView {
        LaTeX = @"-h - (5xy+2) = z",
        HeightRequest = 75,
        FontSize = 22.5f
      };

      //  Text mode fraction
      labels[5] = new MathView {
        LaTeX = @"\frac12x + \frac{3\div4}2y = 25",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      labels[5].LineStyle = LineStyle.Text;

      //  Display mode fraction
      labels[6] = new MathView {
        LaTeX = @"\frac{x+\frac{12}{5}}{y}+\frac1z = \frac{xz+y+\frac{12}{5}z}{yz}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      labels[6].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);

      //  fraction in fraction in text mode
      labels[7] = new MathView {
        LaTeX = @"\frac{x+\frac{12}{5}}{y}+\frac1z = \frac{xz+y+\frac{12}{5}z}{yz}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      labels[7].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      labels[7].LineStyle = LineStyle.Text;

      //  Exponents and subscripts

      //  Large font
      labels[8] = new MathView {
        LaTeX = @"\frac{x^{2+3y}}{x^{2+4y}} = x^y \times \frac{z_1^{y+1}}{z_1^{y+1}}",
        HeightRequest = 168.75,
        FontSize = 22.5f
      };
      labels[8].FontSize = 30;
      labels[8].TextAlignment = TextAlignment.Center;

      //  Small font
      labels[9] = new MathView {
        LaTeX = @"\frac{x^{2+3y}}{x^{2+4y}} = x^y \times \frac{z_1^{y+1}}{z_1^{y+1}}",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[9].FontSize = 10;
      labels[9].TextAlignment = TextAlignment.Center;

      //  Square root
      labels[10] = new MathView {
        LaTeX = @"5+\sqrt{2}+3",
        HeightRequest = 75,
        FontSize = 22.5f
      };

      //  Square root inside square roots and with fractions
      labels[11] = new MathView {
        LaTeX = @"\sqrt{\frac{\sqrt{\frac{1}{2}} + 3}{\sqrt5^x}}+\sqrt{3x}+x^{\sqrt2}",
        HeightRequest = 168.75,
        FontSize = 22.5f
      };

      //  General root
      labels[12] = new MathView {
        LaTeX = @"\sqrt[3]{24} + 3\sqrt{2}24",
        HeightRequest = 75,
        FontSize = 22.5f
      };

      //  Fractions and formulae in root
      labels[13] = new MathView {
        LaTeX = @"\sqrt[x+\frac{3}{4}]{\frac{2}{4}+1}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Non-symbol operators with no limits
      labels[14] = new MathView {
        LaTeX = @"\sin^2(\theta)=\log_3^2(\pi)",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Non-symbol operators with limits
      labels[15] = new MathView {
        LaTeX = @"\lim_{x\to\infty}\frac{e^2}{1-x}=\limsup_{\sigma}5",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Symbol operators with limits
      labels[16] = new MathView {
        LaTeX = @"\sum_{n=1}^{\infty}\frac{1+n}{1-n}=\bigcup_{A\in\Im}C\cup B",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Symbol operators with limits text style
      labels[17] = new MathView {
        LaTeX = @"\sum_{n=1}^{\infty}\frac{1+n}{1-n}=\bigcup_{A\in\Im}C\cup B",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      labels[17].LineStyle = LineStyle.Text;

      //  Non-symbol operators with limits text style
      labels[18] = new MathView {
        LaTeX = @"\lim_{x\to\infty}\frac{e^2}{1-x}=\limsup_{\sigma}5",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      labels[18].LineStyle = LineStyle.Text;

      //  Symbol operators with no limits
      labels[19] = new MathView {
        LaTeX = @"\int_{0}^{\infty}e^x \,dx=\oint_0^{\Delta}5\Gamma",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Test italic correction for large ops
      labels[20] = new MathView {
        LaTeX = @"\int\int\int^{\infty}\int_0\int^{\infty}_0\int",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Test italic correction for superscript/subscript
      labels[21] = new MathView {
        LaTeX = @"U_3^2UY_3^2U_3Y^2f_1f^2ff",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };

      //  Error
      labels[22] = new MathView {
        LaTeX = @"\notacommand",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[23] = new MathView {
        LaTeX = @"\sqrt{1}",
        HeightRequest = 37.5,
        FontSize = 22.5f
      };
      labels[24] = new MathView {
        LaTeX = @"\sqrt[|]{1}",
        HeightRequest = 37.5,
        FontSize = 22.5f
      };
      labels[25] = new MathView {
        LaTeX = @"{n \choose k}",
        HeightRequest = 112.5,
        FontSize = 22.5f
      };
      labels[26] = new MathView {
        LaTeX = @"{n \choose k}",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[26].LineStyle = LineStyle.Text;
      labels[27] = new MathView {
        LaTeX = @"\left({n \atop k}\right)",
        HeightRequest = 75,
        FontSize = 22.5f
      };
      labels[28] = new MathView {
        LaTeX = @"\left({n \atop k}\right)",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[28].LineStyle = LineStyle.Text;
      labels[29] = new MathView {
        LaTeX = @"\underline{xyz}+\overline{abc}",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[30] = new MathView {
        LaTeX = @"\underline{\frac12}+\overline{\frac34}",
        HeightRequest = 93.75,
        FontSize = 22.5f
      };
      labels[31] = new MathView {
        LaTeX = @"\underline{x^\overline{y}_\overline{z}+5}",
        HeightRequest = 93.75,
        FontSize = 22.5f
      };

      //  spacing examples from the TeX book
      labels[32] = new MathView {
        LaTeX = @"\int\!\!\!\int_D dx\,dy",
        HeightRequest = 93.75,
        FontSize = 22.5f
      };

      //  no spacing
      labels[33] = new MathView {
        LaTeX = @"\int\int_D dxdy",
        HeightRequest = 93.75,
        FontSize = 22.5f
      };
      labels[34] = new MathView {
        LaTeX = @"y\,dx-x\,dy",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[35] = new MathView {
        LaTeX = @"y dx - x dy",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };

      //  large spaces
      labels[36] = new MathView {
        LaTeX = @"hello\ from \quad the \qquad other\ side",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };

      //  Accents
      labels[37] = new MathView {
        LaTeX = @"\vec x \; \hat y \; \breve {x^2} \; \tilde x \tilde x^2 x^2",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[38] = new MathView {
        LaTeX = @"\hat{xyz} \; \widehat{xyz}\; \vec{2ab}",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[39] = new MathView {
        LaTeX = @"\hat{\frac12} \; \hat{\sqrt 3}",
        HeightRequest = 93.75,
        FontSize = 22.5f
      };

      //  large roots
      labels[40] = new MathView {
        LaTeX = @"\colorbox{#f0f0e0}{\sqrt{1+\colorbox{#d0c0d0}{\sqrt{1+\colorbox{#a080c0}{\sqrt{1+\colorbox{#7050a0}{\sqrt{1+\colorbox{403060}{\colorbox{#102000}{\sqrt{1+\cdots}}}}}}}}}}}",
        HeightRequest = 150,
        FontSize = 22.5f
      };
      labels[41] = new MathView {
        LaTeX = @"\begin{bmatrix}
                           a & b\\ c & d \\ e & f \\ g &  h \\ i & j
                           \end{bmatrix}",
        HeightRequest = 225,
        FontSize = 22.5f
      };
      labels[42] = new MathView {
        LaTeX = @"x{\scriptstyle y}z",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[43] = new MathView {
        LaTeX = @"x \mathrm x \mathbf x \mathcal X \mathfrak x \mathsf x \bm x \mathtt x \mathit \Lambda \cal g",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[44] = new MathView {
        LaTeX = @"\mathrm{using\ mathrm}",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[45] = new MathView {
        LaTeX = @"\text{using text}",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[46] = new MathView {
        LaTeX = @"\text{Mary has }\$500 + \$200.",
        HeightRequest = 56.25,
        FontSize = 22.5f
      };
      labels[47] = new MathView {
        LaTeX = @"\colorbox{#888888}{\begin{pmatrix}
                       \colorbox{#ff0000}{a} & \colorbox{#00ff00}{b} \\
                       \colorbox{#00aaff}{c} & \colorbox{#f0f0f0}{d}
                       \end{pmatrix}}",
        HeightRequest = 131.25,
        FontSize = 22.5f
      };
    }
  }
}
