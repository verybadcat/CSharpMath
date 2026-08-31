open System
open System.Diagnostics
open SkiaSharp
open ModeMath

type Metric = {
    Name: string
    Iterations: int
    ElapsedNanoseconds: int64
    AllocatedBytes: int64
    RetainedBytes: int64
    Gen0: int
    Gen1: int
    Gen2: int
}

let corpus =
    [| @"x^2 + y_1"
       @"\frac{-b \pm \sqrt{b^2 - 4ac}}{2a}"
       @"\int_0^1 x^2\,dx" |]

let collect () =
    GC.Collect()
    GC.WaitForPendingFinalizers()
    GC.Collect()

let measure name iterations action =
    collect ()
    let retainedBefore = GC.GetTotalMemory(true)
    let allocatedBefore = GC.GetTotalAllocatedBytes(true)
    let gen0 = GC.CollectionCount(0)
    let gen1 = GC.CollectionCount(1)
    let gen2 = GC.CollectionCount(2)
    let stopwatch = Stopwatch.StartNew()
    for _ = 1 to iterations do action ()
    stopwatch.Stop()
    let allocated = GC.GetTotalAllocatedBytes(true) - allocatedBefore
    collect ()
    {
        Name = name
        Iterations = iterations
        ElapsedNanoseconds = stopwatch.ElapsedTicks * 1_000_000_000L / Stopwatch.Frequency
        AllocatedBytes = allocated
        RetainedBytes = GC.GetTotalMemory(true) - retainedBefore
        Gen0 = GC.CollectionCount(0) - gen0
        Gen1 = GC.CollectionCount(1) - gen1
        Gen2 = GC.CollectionCount(2) - gen2
    }

let print metric =
    printfn
        "{\"name\":\"%s\",\"iterations\":%d,\"elapsedNanoseconds\":%d,\"allocatedBytes\":%d,\"retainedBytes\":%d,\"gen0\":%d,\"gen1\":%d,\"gen2\":%d}"
        metric.Name metric.Iterations metric.ElapsedNanoseconds metric.AllocatedBytes
        metric.RetainedBytes metric.Gen0 metric.Gen1 metric.Gen2

let modeRead latex =
    match Latex.Read latex with
    | Ok formula -> formula
    | Error error -> failwith error.Message

let csharpRead latex =
    CSharpMath.Atom.LaTeXParser.MathListFromLaTeX(latex).Match(
        (fun formula -> formula),
        (fun error -> failwith error))

let coldModeMath () =
    measure "modemath-cold-render" 1 (fun () ->
        let formula = modeRead corpus.[1]
        let placed = (Layout 32f<px>).Of formula
        use painter = Painter.Embedded()
        use surface = SKSurface.Create(SKImageInfo(800, 240))
        use paint = new SKPaint()
        painter.Draw(placed, surface.Canvas, 0f<px>, placed.Ascent, paint)
        surface.Canvas.Flush())
    |> print

let coldCSharpMath () =
    measure "csharpmath-cold-render" 1 (fun () ->
        let painter = CSharpMath.SkiaSharp.MathPainter(FontSize = 24f, LaTeX = corpus.[1])
        use surface = SKSurface.Create(SKImageInfo(800, 240))
        painter.Draw(surface.Canvas, CSharpMath.Rendering.FrontEnd.TextAlignment.TopLeft)
        surface.Canvas.Flush())
    |> print

let steadyModeMath () =
    let parsed = corpus |> Array.map modeRead
    let layout = Layout 32f<px>
    let placed = parsed |> Array.map layout.Of
    use painter = Painter.Embedded()
    use surface = SKSurface.Create(SKImageInfo(800, 240))
    use paint = new SKPaint()
    measure "modemath-parse-batch" 10_000 (fun () -> corpus |> Array.iter (modeRead >> ignore)) |> print
    measure "modemath-layout-batch" 5_000 (fun () -> parsed |> Array.iter (layout.Of >> ignore)) |> print
    measure "modemath-draw-batch" 2_000 (fun () ->
        placed |> Array.iter (fun formula -> painter.Draw(formula, surface.Canvas, 0f<px>, formula.Ascent, paint)))
    |> print
    measure "modemath-editor-x2" 2_000 (fun () ->
        let editor = Editor(layout, MA.Empty)
        editor.Press(MathKey.Character 'x') |> ignore
        editor.Press(MathKey.Superscript) |> ignore
        editor.Press(MathKey.Character '2') |> ignore)
    |> print

let steadyCSharpMath () =
    let parsed = corpus |> Array.map csharpRead
    let painter = CSharpMath.SkiaSharp.MathPainter(FontSize = 24f)
    parsed |> Array.iter (fun formula -> painter.Content <- formula; painter.Measure() |> ignore)
    let painters =
        parsed
        |> Array.map (fun formula ->
            let prepared = CSharpMath.SkiaSharp.MathPainter(FontSize = 24f, Content = formula)
            prepared.Measure() |> ignore
            prepared)
    use surface = SKSurface.Create(SKImageInfo(800, 240))
    let fonts = CSharpMath.Rendering.BackEnd.Fonts(Array.empty, 24f)
    measure "csharpmath-parse-batch" 10_000 (fun () -> corpus |> Array.iter (csharpRead >> ignore)) |> print
    measure "csharpmath-layout-batch" 5_000 (fun () ->
        parsed |> Array.iter (fun formula -> painter.Content <- formula; painter.Measure() |> ignore))
    |> print
    measure "csharpmath-draw-batch" 2_000 (fun () ->
        painters |> Array.iter (fun prepared -> prepared.Draw(surface.Canvas, 0f, 120f)))
    |> print
    measure "csharpmath-editor-x2" 2_000 (fun () ->
        use keyboard =
            new CSharpMath.Editor.MathKeyboard<CSharpMath.Rendering.BackEnd.Fonts, CSharpMath.Rendering.BackEnd.Glyph>(
                CSharpMath.Rendering.BackEnd.TypesettingContext.Instance, fonts, 1_000_000_000.)
        keyboard.KeyPress(CSharpMath.Editor.MathKeyboardInput.SmallX)
        keyboard.KeyPress(CSharpMath.Editor.MathKeyboardInput.Power)
        keyboard.KeyPress(CSharpMath.Editor.MathKeyboardInput.D2))
    |> print

match Environment.GetCommandLineArgs() |> Array.tryLast with
| Some "cold-modemath" -> coldModeMath ()
| Some "cold-csharpmath" -> coldCSharpMath ()
| Some "steady-modemath" -> steadyModeMath ()
| Some "steady-csharpmath" -> steadyCSharpMath ()
| _ -> failwith "Expected cold-modemath, cold-csharpmath, steady-modemath, or steady-csharpmath"
