//Do not modify this file directly. Instead, modify this at
//CSharpMath\CSharpMath.Utils\iosMathDemo\Builder.cs and re-generate
//this file by executing the method in that file in the CSharpMath.Utils project.

using CSharpMath.Enumerations;
using CSharpMath.Rendering;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Color = Xamarin.Forms.Color;

namespace CSharpMath.Forms.Example {
  [System.Diagnostics.DebuggerNonUserCode, System.Runtime.CompilerServices.CompilerGenerated]
  public static class MoreExamples {
    public static ReadOnlyCollection<FormsMathView> Views { get; }
    static MoreExamples() {
      var demoLabels = new Dictionary<byte, FormsMathView>();
      var labels = new Dictionary<byte, FormsMathView>();
      
      //  Demo formulae
      
      //  Quadratic formula
      demoLabels[0] = new FormsMathView {
        LaTeX = @"x = \frac{-b \pm \sqrt{b^2-4ac}}{2a}",
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[0].FontSize = 15;
      
      //  This is first label so set the height from the top
      demoLabels[1] = new FormsMathView {
        LaTeX = @"\color{#ff3399}{(a_1+a_2)^2}=a_1^2+2a_1a_2+a_2^2",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[2] = new FormsMathView {
        LaTeX = @"\cos(\theta + \varphi) = \
                                 \cos(\theta)\cos(\varphi) - \sin(\theta)\sin(\varphi)",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[3] = new FormsMathView {
        LaTeX = @"\frac{1}{\left(\sqrt{\phi \sqrt{5}}-\phi\right) e^{\frac25 \pi}} \
                                 = 1+\frac{e^{-2\pi}} {1 +\frac{e^{-4\pi}} {1+\frac{e^{-6\pi}} {1+\frac{e^{-8\pi}} {1+\cdots} } } }",
        HeightRequest = 80,
        FontSize = 15
      };
      demoLabels[4] = new FormsMathView {
        LaTeX = @"\sigma = \sqrt{\frac{1}{N}\sum_{i=1}^N (x_i - \mu)^2}",
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[5] = new FormsMathView {
        LaTeX = @"\neg(P\land Q) \iff (\neg P)\lor(\neg Q)",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[6] = new FormsMathView {
        LaTeX = @"\log_b(x) = \frac{\log_a(x)}{\log_a(b)}",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[7] = new FormsMathView {
        LaTeX = @"\lim_{x\to\infty}\left(1 + \frac{k}{x}\right)^x = e^k",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[8] = new FormsMathView {
        LaTeX = @"\int_{-\infty}^\infty \! e^{-x^2} dx = \sqrt{\pi}",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[9] = new FormsMathView {
        LaTeX = @"\frac 1 n \sum_{i=1}^{n}x_i \geq \sqrt[n]{\prod_{i=1}^{n}x_i}",
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[10] = new FormsMathView {
        LaTeX = @"f^{(n)}(z_0) = \frac{n!}{2\pi i}\oint_\gamma\frac{f(z)}{(z-z_0)^{n+1}}\,dz",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[11] = new FormsMathView {
        LaTeX = @"i\hbar\frac{\partial}{\partial t}\mathbf\Psi(\mathbf{x},t) = 
                           -\frac{\hbar}{2m}\nabla^2\mathbf\Psi(\mathbf{x},t) + 
                           V(\mathbf{x})\mathbf\Psi(\mathbf{x},t)",
        HeightRequest = 40,
        FontSize = 15
      };
      demoLabels[12] = new FormsMathView {
        LaTeX = @"\left(\sum_{k=1}^n a_k b_k \right)^2 \le \left(\sum_{k=1}^n a_k^2\right)\left(\sum_{k=1}^n b_k^2\right)",
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[13] = new FormsMathView {
        LaTeX = @"{n \brace k} = \frac{1}{k!}\sum_{j=0}^k (-1)^{k-j}\binom{k}{j}(k-j)^n",
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[14] = new FormsMathView {
        LaTeX = @"f(x) = \int\limits_{-\infty}^\infty\!\hat f(\xi)\,e^{2 \pi i \xi x}\,\mathrm{d}\xi",
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[15] = new FormsMathView {
        LaTeX = @"\begin{gather}
                           \dot{x} = \sigma(y-x) \\
                           \dot{y} = \rho x - y - xz \\
                           \dot{z} = -\beta z + xy
                           \end{gather}",
        HeightRequest = 70,
        FontSize = 15
      };
      demoLabels[16] = new FormsMathView {
        LaTeX = @"\vec \bf V_1 \times \vec \bf V_2 =  \begin{vmatrix}
                           \hat \imath &\hat \jmath &\hat k \\
                           \frac{\partial X}{\partial u} &  \frac{\partial Y}{\partial u} & 0 \\
                           \frac{\partial X}{\partial v} &  \frac{\partial Y}{\partial v} & 0
                           \end{vmatrix}",
        HeightRequest = 70,
        FontSize = 15
      };
      demoLabels[17] = new FormsMathView {
        LaTeX = @"\begin{eqalign}
                           \nabla \cdot \vec{\bf{E}} & = \frac {\rho} {\varepsilon_0} \\
                           \nabla \cdot \vec{\bf{B}} & = 0 \\
                           \nabla \times \vec{\bf{E}} &= - \frac{\partial\vec{\bf{B}}}{\partial t} \\
                           \nabla \times \vec{\bf{B}} & = \mu_0\vec{\bf{J}} + \mu_0\varepsilon_0 \frac{\partial\vec{\bf{E}}}{\partial t}
                           \end{eqalign}",
        HeightRequest = 140,
        FontSize = 15
      };
      demoLabels[18] = new FormsMathView {
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
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[19] = new FormsMathView {
        LaTeX = @"\frak Q(\lambda,\hat{\lambda}) = 
                           -\frac{1}{2} \mathbb P(O \mid \lambda ) \sum_s \sum_m \sum_t \gamma_m^{(s)} (t) +\\ 
                           \quad \left( \log(2 \pi ) + \log \left| \cal C_m^{(s)} \right| + 
                           \left( o_t - \hat{\mu}_m^{(s)} \right) ^T \cal C_m^{(s)-1} \right) 
                           ",
        HeightRequest = 90,
        FontSize = 15
      };
      demoLabels[20] = new FormsMathView {
        LaTeX = @"f(x) = \begin{cases}
                           \frac{e^x}{2} & x \geq 0 \\
                           1 & x < 0
                           \end{cases}",
        HeightRequest = 60,
        FontSize = 15
      };
      demoLabels[21] = new FormsMathView {
        LaTeX = @"\color{#ff3333}{c}\color{#9933ff}{o}\color{#ff0080}{l}+\color{#99ff33}{\frac{\color{#ff99ff}{o}}{\color{#990099}{r}}}-\color{#33ffff}{\sqrt[\color{#3399ff}{e}]{\color{#3333ff}{d}}}",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Test formulae
      labels[0] = new FormsMathView {
        LaTeX = @"3+2-5 = 0",
        HeightRequest = 40,
        FontSize = 15
      };
      labels[0].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      
      //  Infix and prefix Operators
      labels[1] = new FormsMathView {
        LaTeX = @"12+-3 > +14",
        HeightRequest = 40,
        FontSize = 15
      };
      labels[1].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      labels[1].TextAlignment = TextAlignment.Center;
      
      //  Punct, parens
      labels[2] = new FormsMathView {
        LaTeX = @"(-3-5=-8, -6-7=-13)",
        HeightRequest = 40,
        FontSize = 15
      };
      
      //  Latex commands
      labels[3] = new FormsMathView {
        LaTeX = @"5\times(-2 \div 1) = -10",
        HeightRequest = 40,
        FontSize = 15
      };
      labels[3].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      labels[3].TextAlignment = TextAlignment.Right;
      labels[4] = new FormsMathView {
        LaTeX = @"-h - (5xy+2) = z",
        HeightRequest = 40,
        FontSize = 15
      };
      
      //  Text mode fraction
      labels[5] = new FormsMathView {
        LaTeX = @"\frac12x + \frac{3\div4}2y = 25",
        HeightRequest = 60,
        FontSize = 15
      };
      labels[5].LineStyle = LineStyle.Text;
      
      //  Display mode fraction
      labels[6] = new FormsMathView {
        LaTeX = @"\frac{x+\frac{12}{5}}{y}+\frac1z = \frac{xz+y+\frac{12}{5}z}{yz}",
        HeightRequest = 60,
        FontSize = 15
      };
      labels[6].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      
      //  fraction in fraction in text mode
      labels[7] = new FormsMathView {
        LaTeX = @"\frac{x+\frac{12}{5}}{y}+\frac1z = \frac{xz+y+\frac{12}{5}z}{yz}",
        HeightRequest = 60,
        FontSize = 15
      };
      labels[7].BackgroundColor = Color.FromHsla(0.15, 0.2, 1.0, 1.0);
      labels[7].LineStyle = LineStyle.Text;
      
      //  Exponents and subscripts
      
      //  Large font
      labels[8] = new FormsMathView {
        LaTeX = @"\frac{x^{2+3y}}{x^{2+4y}} = x^y \times \frac{z_1^{y+1}}{z_1^{y+1}}",
        HeightRequest = 90,
        FontSize = 15
      };
      labels[8].FontSize = 30;
      labels[8].TextAlignment = TextAlignment.Center;
      
      //  Small font
      labels[9] = new FormsMathView {
        LaTeX = @"\frac{x^{2+3y}}{x^{2+4y}} = x^y \times \frac{z_1^{y+1}}{z_1^{y+1}}",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[9].FontSize = 10;
      labels[9].TextAlignment = TextAlignment.Center;
      
      //  Square root
      labels[10] = new FormsMathView {
        LaTeX = @"5+\sqrt{2}+3",
        HeightRequest = 40,
        FontSize = 15
      };
      
      //  Square root inside square roots and with fractions
      labels[11] = new FormsMathView {
        LaTeX = @"\sqrt{\frac{\sqrt{\frac{1}{2}} + 3}{\sqrt5^x}}+\sqrt{3x}+x^{\sqrt2}",
        HeightRequest = 90,
        FontSize = 15
      };
      
      //  General root
      labels[12] = new FormsMathView {
        LaTeX = @"\sqrt[3]{24} + 3\sqrt{2}24",
        HeightRequest = 40,
        FontSize = 15
      };
      
      //  Fractions and formulae in root
      labels[13] = new FormsMathView {
        LaTeX = @"\sqrt[x+\frac{3}{4}]{\frac{2}{4}+1}",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Non-symbol operators with no limits
      labels[14] = new FormsMathView {
        LaTeX = @"\sin^2(\theta)=\log_3^2(\pi)",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Non-symbol operators with limits
      labels[15] = new FormsMathView {
        LaTeX = @"\lim_{x\to\infty}\frac{e^2}{1-x}=\limsup_{\sigma}5",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Symbol operators with limits
      labels[16] = new FormsMathView {
        LaTeX = @"\sum_{n=1}^{\infty}\frac{1+n}{1-n}=\bigcup_{A\in\Im}C\cup B",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Symbol operators with limits text style
      labels[17] = new FormsMathView {
        LaTeX = @"\sum_{n=1}^{\infty}\frac{1+n}{1-n}=\bigcup_{A\in\Im}C\cup B",
        HeightRequest = 60,
        FontSize = 15
      };
      labels[17].LineStyle = LineStyle.Text;
      
      //  Non-symbol operators with limits text style
      labels[18] = new FormsMathView {
        LaTeX = @"\lim_{x\to\infty}\frac{e^2}{1-x}=\limsup_{\sigma}5",
        HeightRequest = 60,
        FontSize = 15
      };
      labels[18].LineStyle = LineStyle.Text;
      
      //  Symbol operators with no limits
      labels[19] = new FormsMathView {
        LaTeX = @"\int_{0}^{\infty}e^x \,dx=\oint_0^{\Delta}5\Gamma",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Test italic correction for large ops
      labels[20] = new FormsMathView {
        LaTeX = @"\int\int\int^{\infty}\int_0\int^{\infty}_0\int",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Test italic correction for superscript/subscript
      labels[21] = new FormsMathView {
        LaTeX = @"U_3^2UY_3^2U_3Y^2f_1f^2ff",
        HeightRequest = 60,
        FontSize = 15
      };
      
      //  Error
      labels[22] = new FormsMathView {
        LaTeX = @"\notacommand",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[23] = new FormsMathView {
        LaTeX = @"\sqrt{1}",
        HeightRequest = 20,
        FontSize = 15
      };
      labels[24] = new FormsMathView {
        LaTeX = @"\sqrt[|]{1}",
        HeightRequest = 20,
        FontSize = 15
      };
      labels[25] = new FormsMathView {
        LaTeX = @"{n \choose k}",
        HeightRequest = 60,
        FontSize = 15
      };
      labels[26] = new FormsMathView {
        LaTeX = @"{n \choose k}",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[26].LineStyle = LineStyle.Text;
      labels[27] = new FormsMathView {
        LaTeX = @"\left({n \atop k}\right)",
        HeightRequest = 40,
        FontSize = 15
      };
      labels[28] = new FormsMathView {
        LaTeX = @"\left({n \atop k}\right)",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[28].LineStyle = LineStyle.Text;
      labels[29] = new FormsMathView {
        LaTeX = @"\underline{xyz}+\overline{abc}",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[30] = new FormsMathView {
        LaTeX = @"\underline{\frac12}+\overline{\frac34}",
        HeightRequest = 50,
        FontSize = 15
      };
      labels[31] = new FormsMathView {
        LaTeX = @"\underline{x^\overline{y}_\overline{z}+5}",
        HeightRequest = 50,
        FontSize = 15
      };
      
      //  spacing examples from the TeX book
      labels[32] = new FormsMathView {
        LaTeX = @"\int\!\!\!\int_D dx\,dy",
        HeightRequest = 50,
        FontSize = 15
      };
      
      //  no spacing
      labels[33] = new FormsMathView {
        LaTeX = @"\int\int_D dxdy",
        HeightRequest = 50,
        FontSize = 15
      };
      labels[34] = new FormsMathView {
        LaTeX = @"y\,dx-x\,dy",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[35] = new FormsMathView {
        LaTeX = @"y dx - x dy",
        HeightRequest = 30,
        FontSize = 15
      };
      
      //  large spaces
      labels[36] = new FormsMathView {
        LaTeX = @"hello\ from \quad the \qquad other\ side",
        HeightRequest = 30,
        FontSize = 15
      };
      
      //  Accents
      labels[37] = new FormsMathView {
        LaTeX = @"\vec x \; \hat y \; \breve {x^2} \; \tilde x \tilde x^2 x^2 ",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[38] = new FormsMathView {
        LaTeX = @"\hat{xyz} \; \widehat{xyz}\; \vec{2ab}",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[39] = new FormsMathView {
        LaTeX = @"\hat{\frac12} \; \hat{\sqrt 3}",
        HeightRequest = 50,
        FontSize = 15
      };
      
      //  large roots
      labels[40] = new FormsMathView {
        LaTeX = @"\sqrt{1+\sqrt{1+\sqrt{1+\sqrt{1+\sqrt{1+\cdots}}}}}",
        HeightRequest = 80,
        FontSize = 15
      };
      labels[41] = new FormsMathView {
        LaTeX = @"\begin{bmatrix}
                           a & b\\ c & d \\ e & f \\ g &  h \\ i & j
                           \end{bmatrix}",
        HeightRequest = 120,
        FontSize = 15
      };
      labels[42] = new FormsMathView {
        LaTeX = @"x{\scriptstyle y}z",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[43] = new FormsMathView {
        LaTeX = @"x \mathrm x \mathbf x \mathcal X \mathfrak x \mathsf x \bm x \mathtt x \mathit \Lambda \cal g",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[44] = new FormsMathView {
        LaTeX = @"\mathrm{using\ mathrm}",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[45] = new FormsMathView {
        LaTeX = @"\text{using text}",
        HeightRequest = 30,
        FontSize = 15
      };
      labels[46] = new FormsMathView {
        LaTeX = @"\text{Mary has }\$500 + \$200.",
        HeightRequest = 30,
        FontSize = 15
      };
      
      Views = demoLabels.Concat(labels).Select(p => p.Value).ToList().AsReadOnly();
    }
  }
}
