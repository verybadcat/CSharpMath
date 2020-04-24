namespace CSharpMath.Editor {
  public enum MathKeyboardInput {
    // Navigation
    Up = '⏶', Down = '⏷', Left = '⏴', Right = '⏵',
    Backspace = '⌫', Clear = '⎚', Return = '\n', Dismiss = '❌',

    // Decimals
    D0 = '0', D1 = '1', D2 = '2', D3 = '3', D4 = '4',
    D5 = '5', D6 = '6', D7 = '7', D8 = '8', D9 = '9', Decimal = '.',

    // Basic operators
    Plus = '+', Minus = '-', Multiply = '*', Divide = '÷',
    Ratio = ':', Percentage = '%', Comma = ',', Semicolon = ';', Factorial = '!',
    Infinity = '∞', Angle = '∠', Degree = '°', VerticalBar = '|',
    Logarithm = '㏒', NaturalLogarithm = '㏑', Prime = '\'', PartialDifferential = '∂',
    LeftArrow = '←', UpArrow = '↑', RightArrow = '→', DownArrow = '↓', Space = ' ',

    // More complicated operators
    Slash = '/', Fraction = '⁄', Power = '^', Subscript = '_', SquareRoot = '√', 
    CubeRoot = '∛', NthRoot = '∜', BaseEPower = 'ℯ', LogarithmWithBase = '㏐',

    // Basic Brackets
    LeftRoundBracket = '(', RightRoundBracket = ')',
    LeftSquareBracket = '[', RightSquareBracket = ']',
    LeftCurlyBracket = '{', RightCurlyBracket = '}',

    // Both Brackets
    BothRoundBrackets = '㈾', BothSquareBrackets = '㈿', BothCurlyBrackets = '㉀',
    Absolute = '㉁',

    // Relations
    Equals = '=', NotEquals = '≠',
    LessThan = '<', LessOrEquals = '≤', GreaterThan = '>', GreaterOrEquals = '≥',

    // Capital English alphabets
    A = 'A', B = 'B', C = 'C', D = 'D', E = 'E', F = 'F', G = 'G', H = 'H', I = 'I',
    J = 'J', K = 'K', L = 'L', M = 'M', N = 'N', O = 'O', P = 'P', Q = 'Q', R = 'R',
    S = 'S', T = 'T', U = 'U', V = 'V', W = 'W', X = 'X', Y = 'Y', Z = 'Z',

    // Small English alphabets
    SmallA = 'a', SmallB = 'b', SmallC = 'c', SmallD = 'd', SmallE = 'e',
    SmallF = 'f', SmallG = 'g', SmallH = 'h', SmallI = 'i', SmallJ = 'j',
    SmallK = 'k', SmallL = 'l', SmallM = 'm', SmallN = 'n', SmallO = 'o',
    SmallP = 'p', SmallQ = 'q', SmallR = 'r', SmallS = 's', SmallT = 't',
    SmallU = 'u', SmallV = 'v', SmallW = 'w', SmallX = 'x', SmallY = 'y', SmallZ = 'z',

    // Capital Greek alphabets
    Alpha = 'Α', Beta = 'Β', Gamma = 'Γ', Delta = 'Δ', Epsilon = 'Ε', Zeta = 'Ζ', Eta = 'Η', Theta = 'Θ',
    Iota = 'Ι', Kappa = 'Κ', Lambda = 'Λ', Mu = 'Μ', Nu = 'Ν', Xi = 'Ξ', Omicron = 'Ο', Pi = 'Π',
    Rho = 'Ρ', Sigma = 'Σ', Tau = 'Τ', Upsilon = 'Υ', Phi = 'Φ', Chi = 'Χ', Psi = 'Ψ', Omega = 'Ω',

    // Small Greek alphabets
    SmallAlpha = 'α', SmallBeta = 'β', SmallGamma = 'γ', SmallDelta = 'δ', SmallEpsilon = 'ϵ', SmallEpsilon2 = 'ε',
    SmallZeta = 'ζ', SmallEta = 'η', SmallTheta = 'θ', SmallIota = 'ι', SmallKappa = 'κ', SmallKappa2 = 'ϰ',
    SmallLambda = 'λ', SmallMu = 'μ', SmallNu = 'ν', SmallXi = 'ξ', SmallOmicron = 'ο', SmallPi = 'π',
    SmallPi2 = 'ϖ', SmallRho = 'ρ', SmallRho2 = 'ϱ', SmallSigma = 'σ', SmallSigma2 = 'ς', SmallTau = 'τ',
    SmallUpsilon = 'υ', SmallPhi = 'ϕ', SmallPhi2 = 'φ', SmallChi = 'χ', SmallPsi = 'ψ', SmallOmega = 'ω',

    // Trigonometric functions
    Sine = '␖', Cosine = '℅', Tangent = '␘', Cotangent = '␄', Secant = '␎', Cosecant = '␛',
    ArcSine = '◜', ArcCosine = '◝', ArcTangent = '◟', ArcCotangent = '◞', ArcSecant = '◠', ArcCosecant = '◡',
    
    // Hyperbolic functions
    HyperbolicSine = '◐', HyperbolicCosine = '◑', HyperbolicTangent = '◓',
    HyperbolicCotangent = '◒', HyperbolicSecant = '◔', HyperbolicCosecant = '◕',
    AreaHyperbolicSine = '◴', AreaHyperbolicCosine = '◷', AreaHyperbolicTangent = '◵',
    AreaHyperbolicCotangent = '◶', AreaHyperbolicSecant = '⚆', AreaHyperbolicCosecant = '⚇',

    // Calculus operators
    LimitWithBase = '㋏', 
    Integral = '∫', IntegralLowerLimit= 'ʆ', IntegralUpperLimit = 'ƒ', IntegralBothLimits = 'ʄ',
    Summation = '∑', SummationLowerLimit = 'Ƹ', SummationUpperLimit = 'ⵉ', SummationBothLimits = '⅀',
    Product = '∏', ProductLowerLimit = 'ᚂ', ProductUpperLimit = '⥣', ProductBothLimits = '⍡',
    DoubleIntegral = '∬', TripleIntegral = '∭', QuadrupleIntegral = '⨌',
    ContourIntegral = '∮', DoubleContourIntegral = '∯', TripleContourIntegral = '∰',
    ClockwiseIntegral = '∱', ClockwiseContourIntegral = '∲', CounterClockwiseContourIntegral = '∳'
  }
}
