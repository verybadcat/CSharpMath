module CSharpMath.Rendering.Tests.FSharp.RandomKeyboardInputsTest

open Xunit
open CSharpMath.Editor

// can use Hedgehog or FSCheck instead for random testing
let getRandomMathKeyboardInput =
    let mathKeyboardInputs = typeof<MathKeyboardInput>.GetEnumValues() :?> MathKeyboardInput[]
    let r = System.Random()
    let n = mathKeyboardInputs.Length
    fun () -> mathKeyboardInputs.[r.Next(n)]

/// adds 100 random keypresses to a MathKeyboard and gets LaTeX and checks that there is no crash
let private test100keypresses() =
    use keyboard = new CSharpMath.Rendering.FrontEnd.MathKeyboard()
    let mutable reverseInputs:MathKeyboardInput list = []
    try
        for _ in 1 .. 100 do
            let ki = getRandomMathKeyboardInput()
            reverseInputs <- ki::reverseInputs
            ki |> keyboard.KeyPress
            keyboard.LaTeX |> ignore
        Ok()
    with _ ->
        Error(reverseInputs |> List.rev)

let private tryList(kl:MathKeyboardInput list) =
    use keyboard = new CSharpMath.Rendering.FrontEnd.MathKeyboard()
    for ki in kl do
        ki |> keyboard.KeyPress
        keyboard.LaTeX |> ignore

// kl gives an error; this finds a shortest sublist that gives an error
let rec private findShortening(kl:MathKeyboardInput list) =
    let isError(kl:MathKeyboardInput list) =
        try
            tryList kl
            false
        with _ -> true
    let reductions =
        List.init kl.Length (fun i ->
            kl.[0 .. i-1] @ kl.[i+1 .. kl.Length-1])
    match reductions |> List.tryFind isError with
    | None -> kl
    | Some sl -> findShortening sl

let [<Fact>] ``random inputs don't crash editor``() =
    let results = List.init 100 (fun _ -> test100keypresses())
    let shortestError =
        results
        |> List.choose (function Ok _ -> None | Error e -> Some e)
        |> List.sortBy (fun inputs -> inputs.Length)
    match shortestError with
    | [] -> ()
    | kl::_ ->
        let shortestSublist = findShortening kl
        try tryList shortestSublist
        with ex -> failwith $"Exeption: {ex.Message} inputs: {shortestSublist}"  