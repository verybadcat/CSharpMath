using System;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSharpMath.Rendering.Tests {
  // InMemoryImageData class holding the raw image byte array and label.
  public class InMemoryImageData {
    [LoadColumn(0)]
    public byte[]? Image;

    [LoadColumn(1)]
    public string? Label;
  }

  // ImagePrediction class holding the score and predicted label metrics.
  public class ImagePrediction {
    [ColumnName("Score")]
    public float[]? Score;

    [ColumnName("PredictedLabel")]
    public string? PredictedLabel;
  }
  public class ImageComparer {
    public static void Train() {
      var imageData = new InMemoryImageData {
        Image = System.IO.File.ReadAllBytes(@"C:\Users\hadri\source\repos\CSharpMath\CSharpMath.Rendering.Tests\MathDisplay\Abs.png"),
        Label = "Abs"
      };

      var mlContext = new MLContext(1);
      mlContext.Log += (sender, e) => Console.WriteLine(e.Message);
      var data = mlContext.Data.LoadFromEnumerable(new[] { imageData });

      data = mlContext.Transforms.Conversion
        .MapValueToKey("Label", keyOrdinality: Microsoft.ML.Transforms
          .ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
        .Fit(data)
        .Transform(data);

      var pipeline = mlContext.MulticlassClassification.Trainers
        .ImageClassification(featureColumnName: "Image")
        .Append(mlContext.Transforms.Conversion.MapKeyToValue(
            outputColumnName: "PredictedLabel",
            inputColumnName: "Label"));

      var trainedModel = pipeline.Fit(data);
      //
      var predictor = mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(trainedModel);

      var testImageData = new InMemoryImageData {
        Image = System.IO.File.ReadAllBytes(@"C:\Users\hadri\source\repos\CSharpMath\CSharpMath.Rendering.Tests\MathDisplay\Abs.avalonia.png"),
      };

      var prediction = predictor.Predict(testImageData);

      Console.WriteLine($"Scores : [{string.Join(",", prediction.Score)}], " + $"Predicted Label : {prediction.PredictedLabel}");
    }
  }
}
