using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Collections.ObjectModel;
using _3Dice.Models;

namespace _3Dice.Views
{
    public partial class DiceRollingView : ContentView
    {
        private readonly List<Dice3D> _dice = new();
        private DateTime _lastUpdate = DateTime.Now;
        private readonly Random _random = new();
        private bool _isRolling = false;

        public event EventHandler<DiceRolledEventArgs>? DiceRolled;

        public DiceRollingView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var skiaView = new SKCanvasView
            {
                HeightRequest = 300,
                BackgroundColor = Colors.LightGray
            };

            skiaView.PaintSurface += OnPaintSurface;

            Content = new Frame
            {
                Content = skiaView,
                BackgroundColor = Colors.DarkGreen,
                CornerRadius = 15,
                Padding = 10,
                HasShadow = true
            };

            // Start animation loop using modern approach
            this.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick); // ~60 FPS
        }

        public void RollDice(List<DiceType> diceTypes)
        {
            if (_isRolling) return;

            _dice.Clear();
            _isRolling = true;

            var width = 300f; // Canvas width
            var currentX = 50f;

            foreach (var diceType in diceTypes.Where(d => d.Count > 0))
            {
                for (int i = 0; i < diceType.Count; i++)
                {
                    var dice = new Dice3D(diceType.Sides, new Vector3D(
                        currentX + _random.NextSingle() * 100,
                        50 + _random.NextSingle() * 50,
                        0
                    ));

                    _dice.Add(dice);
                    currentX += 40;

                    // Wrap to next row if needed
                    if (currentX > width - 50)
                    {
                        currentX = 50;
                    }
                }
            }

            _lastUpdate = DateTime.Now;
        }

        private bool OnTimerTick()
        {
            var now = DateTime.Now;
            var deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            if (_isRolling)
            {
                var stillRolling = false;
                foreach (var dice in _dice)
                {
                    dice.Update(deltaTime, 300, 300);
                    if (dice.IsRolling)
                        stillRolling = true;
                }

                if (!stillRolling && _dice.Count > 0)
                {
                    _isRolling = false;
                    var results = _dice.GroupBy(d => d.Sides)
                                      .Select(g => new DiceGroupResult 
                                      { 
                                          Sides = g.Key, 
                                          Values = g.Select(d => d.FinalValue).ToList() 
                                      })
                                      .ToList();
                    
                    DiceRolled?.Invoke(this, new DiceRolledEventArgs(results));
                }

                // Force redraw
                if (Content is Frame frame && frame.Content is SKCanvasView canvasView)
                {
                    canvasView.InvalidateSurface();
                }
            }

            return true; // Continue timer
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            canvas.Clear(SKColors.DarkGreen);

            // Draw table surface with proper perspective
            DrawTableSurface(canvas, info);

            // Draw dice
            foreach (var dice in _dice)
            {
                dice.Draw(canvas);
            }

            // Draw rolling text if rolling
            if (_isRolling && _dice.Any(d => d.IsRolling))
            {
                var font = new SKFont
                {
                    Size = 24,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                };

                var paint = new SKPaint
                {
                    Color = SKColors.White,
                    IsAntialias = true
                };

                canvas.DrawText(
                    "Rolling...",
                    info.Width / 2,
                    30,
                    SKTextAlign.Center,
                    font,
                    paint
                );
            }
        }

        private void DrawTableSurface(SKCanvas canvas, SKImageInfo info)
        {
            // Calculate table surface position (70% down for consistency)
            var tableSurface = info.Height * 0.7f;

            // Background above table
            var bgPaint = new SKPaint
            {
                Color = SKColor.Parse("#1a3009"), // Dark green background
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(new SKRect(0, 0, info.Width, tableSurface), bgPaint);

            // Table surface (felt texture)
            var tableGradient = SKShader.CreateLinearGradient(
                new SKPoint(0, tableSurface),
                new SKPoint(0, info.Height),
                new SKColor[] 
                { 
                    SKColor.Parse("#2d5016"), // Felt green
                    SKColor.Parse("#1a3009")  // Darker green
                },
                null,
                SKShaderTileMode.Clamp);

            var tablePaint = new SKPaint
            {
                Shader = tableGradient,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawRect(new SKRect(0, tableSurface, info.Width, info.Height), tablePaint);

            // Table edge
            var edgePaint = new SKPaint
            {
                Color = SKColor.Parse("#4a7c2a"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            };

            canvas.DrawLine(0, tableSurface, info.Width, tableSurface, edgePaint);

            // Subtle texture
            var texturePaint = new SKPaint
            {
                Color = SKColor.Parse("#2d5016").WithAlpha(30),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true
            };

            for (int i = 0; i < info.Width; i += 15)
            {
                canvas.DrawLine(i, tableSurface, i, info.Height, texturePaint);
            }
        }
    }

    public class DiceRolledEventArgs : EventArgs
    {
        public List<DiceGroupResult> Results { get; }

        public DiceRolledEventArgs(List<DiceGroupResult> results)
        {
            Results = results;
        }
    }

    public class DiceGroupResult
    {
        public int Sides { get; set; }
        public List<int> Values { get; set; } = new();
    }
}