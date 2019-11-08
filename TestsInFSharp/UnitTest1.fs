module TestsInFSharp

open NUnit.Framework

open CSharpMath.Editor

let private mathKeyboardInputs =
    [|  MathKeyboardInput.A
        MathKeyboardInput.B
        MathKeyboardInput.C
        MathKeyboardInput.Alpha
        MathKeyboardInput.Sine
        MathKeyboardInput.ArcCotangent
        MathKeyboardInput.Subscript
        MathKeyboardInput.Power
        MathKeyboardInput.Left
        MathKeyboardInput.Right
        MathKeyboardInput.Up
        MathKeyboardInput.Down
        MathKeyboardInput.Equals
        MathKeyboardInput.Plus
        MathKeyboardInput.Minus
        MathKeyboardInput.Divide
        MathKeyboardInput.LeftRoundBracket
        MathKeyboardInput.RightRoundBracket
        MathKeyboardInput.BothRoundBrackets
    |]

// can use Hedgehog or FSCheck instead for random testing

let getRandomMathKeyboardInput =
    let r = System.Random()
    let n = mathKeyboardInputs.Length
    fun () -> mathKeyboardInputs.[r.Next(n)]

/// adds 100 random keypresses to a MathKeyboard and gets LaTeX and checks that there is no crash
let test100keypresses() =
    let keyboard = CSharpMath.Rendering.MathKeyboard()
    let mutable reverseInputs:MathKeyboardInput list = []
    for _ = 1 to 100 do
        let ki = getRandomMathKeyboardInput()
        reverseInputs <- ki::reverseInputs
        try
            getRandomMathKeyboardInput() |> keyboard.KeyPress
            keyboard.LaTeX |> ignore
        with _ ->
            failwith <| sprintf "KeyboardInputs: %A" (reverseInputs |> List.rev)

[<Test>]
let ``random inputs don't crash editor``() =
    for i = 1 to 1000 do
        test100keypresses()