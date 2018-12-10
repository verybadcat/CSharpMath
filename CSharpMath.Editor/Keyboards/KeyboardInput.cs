using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  public enum KeyboardInput {
    Up = '⏶', Down = '⏷', Left = '⏴', Right = '⏵', Backspace = '⌫', Clear = '⎚',

    LeftBracket = '(', RightBracket = ')',
    D0 = '0', D1 = '1', D2 = '2', D3 = '3', D4 = '4', D5 = '5', D6 = '6', D7 = '7', D8 = '8', D9 = '9',
    Decimal = '.', Plus = '+', Minus = '−', Multiply = '×', Divide = '÷', 

    A = 'a', B = 'b', C = 'c', D = 'd', E = 'e', F = 'f', G = 'g', H = 'h', I = 'i',
    J = 'j', K = 'k', L = 'l', M = 'm', N = 'n', O = 'o', P = 'p', Q = 'q', R = 'r',
    S = 's', T = 't', U = 'u', V = 'v', W = 'w', X = 'x', Y = 'y', Z = 'z',
    
    LowerA = 'a', LowerB = 'b', LowerC = 'c', LowerD = 'd', LowerE = 'e',
    LowerF = 'f', LowerG = 'g', LowerH = 'h', LowerI = 'i', LowerJ = 'j',
    LowerK = 'k', LowerL = 'l', LowerM = 'm', LowerN = 'n', LowerO = 'o',
    LowerP = 'p', LowerQ = 'q', LowerR = 'r', LowerS = 's', LowerT = 't',
    LowerU = 'u', LowerV = 'v', LowerW = 'w', LowerX = 'x', LowerY = 'y', LowerZ = 'z',

    Alpha = 'Α', Beta = 'Β', Gamma = 'Γ', Delta = 'Δ', Epsilon = 'Ε', Zeta = 'Ζ',
    Eta = 'Η', Theta = 'Θ', Iota = 'Ι', Kappa = 'Κ', Lambda = 'Λ', Mu = 'Μ', Nu = 'Ν',
    Xi = 'Ξ', Omicron = 'Ο', Pi = 'Π', Rho = 'Ρ', Sigma = 'Σ', Tau = 'Τ', Upsilon = 'Υ',
    Phi = 'Φ', Chi = 'Χ', Omega = 'Ω',

    LowerAlpha = 'α', LowerBeta = 'β', LowerGamma = 'γ', LowerDelta = 'δ', LowerEpsilon = 'ε',
    LowerZeta = 'ζ', LowerEta = 'η', LowerTheta = 'θ', LowerIota = 'ι', LowerKappa = 'κ',
    LowerLambda = 'λ', LowerMu = 'μ', LowerNu = 'ν', LowerXi = 'ξ', LowerOmicron = 'ο',
    LowerPi = 'π', LowerRho = 'ρ', LowerSigma = 'σ', LowerSigma2 = 'ς', LowerTau = 'τ',
    LowerUpsilon = 'υ', LowerPhi = 'φ', LowerChi = 'χ', LowerOmega = 'ω',

    Sine = '␖', Cosine = '℅', Tangent = '␘', Cotangent = '␄', Secant = '␎', Cosecant = '␛',
    ArcSine = '◜', ArcCosine = '◝', ArcTangent = '◟',
    ArcCotangent = '◞', ArcSecant = '◠', ArcCosecant = '◡',

    Power = '^', SquareRoot = '√', CubeRoot = '∛', NthRoot = '∜', Absolute = '|',

    HyperbolicSine = '◐', HyperbolicCosine = '◑', HyperbolicTangent = '◓',
    HyperbolicCotangent = '◒', HyperbolicSecant = '◔', HyperbolicCosecant = '◕',
    AreaHyperbolicSine = '◴', AreaHyperbolicCosine = '◷', AreaHyperbolicTangent = '◵',
    AreaHyperbolicCotangent = '◶', AreaHyperbolicSecant = '⚆', AreaHyperbolicCosecant = '⚇',

    BaseEPower = 'ℯ', Logarithm = '㏒', NaturalLog = '㏑', Factorial = '!'
  }
}
