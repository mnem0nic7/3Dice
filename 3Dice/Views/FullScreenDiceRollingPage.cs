using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using _3Dice.Models;

namespace _3Dice.Views
{
    public partial class FullScreenDiceRollingPage : ContentPage
    {
        private readonly List<EnhancedDice3D> _dice = new();
        private readonly List<Particle> _particles = new();
        private DateTime _lastUpdate = DateTime.Now;
        private readonly Random _random = new();
        private bool _isRolling = false;
        private bool _showResults = false;
        private string _resultsText = "";
        private float _canvasWidth = 1;
        private float _canvasHeight = 1;
        
        private SKCanvasView _canvasView;
        private Grid _overlayGrid;
        private Button _backButton;
        private Label _titleLabel;

        public FullScreenDiceRollingPage(List<DiceType> diceTypes)
        {
            InitializeComponent();
            StartRolling(diceTypes);
        }

        private void InitializeComponent()
        {
            // Full screen setup
            NavigationPage.SetHasNavigationBar(this, false);
            
            // Create main canvas that fills the screen
            _canvasView = new SKCanvasView
            {
                BackgroundColor = Colors.Black
            };
            _canvasView.PaintSurface += OnPaintSurface;

            // Create overlay UI
            _backButton = new Button
            {
                Text = "? Back",
                FontSize = 18,
                BackgroundColor = Colors.Black.WithAlpha(0.7f),
                TextColor = Colors.White,
                CornerRadius = 20,
                Padding = new Thickness(20, 10),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(20, 50, 0, 0)
            };
            _backButton.Clicked += OnBackClicked;

            _titleLabel = new Label
            {
                Text = "?? Rolling Dice... ??",
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 60, 0, 0),
                BackgroundColor = Colors.Black.WithAlpha(0.7f),
                Padding = new Thickness(20, 10)
            };

            _overlayGrid = new Grid();
            _overlayGrid.Children.Add(_canvasView);
            _overlayGrid.Children.Add(_backButton);
            _overlayGrid.Children.Add(_titleLabel);

            Content = _overlayGrid;

            // Start high-performance animation loop
            this.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick); // 60 FPS
        }

        private void StartRolling(List<DiceType> diceTypes)
        {
            _dice.Clear();
            _particles.Clear();
            _isRolling = true;
            _showResults = false;

            // Create dice with better positioning for full screen
            var totalDice = diceTypes.Sum(d => d.Count);
            var dicePerRow = Math.Min(6, (int)Math.Ceiling(Math.Sqrt(totalDice)));
            var currentDice = 0;

            foreach (var diceType in diceTypes.Where(d => d.Count > 0))
            {
                for (int i = 0; i < diceType.Count; i++)
                {
                    var row = currentDice / dicePerRow;
                    var col = currentDice % dicePerRow;
                    
                    // Spread dice across screen with random initial positions
                    var startX = 100 + col * 120 + _random.NextSingle() * 50;
                    var startY = 100 + row * 120 + _random.NextSingle() * 50;

                    var dice = new EnhancedDice3D(diceType.Sides, new Vector3D(startX, startY, 0))
                    {
                        Size = 50f + _random.NextSingle() * 20f // Varied sizes for visual interest
                    };

                    _dice.Add(dice);
                    currentDice++;
                }
            }

            _lastUpdate = DateTime.Now;
        }

        private bool OnTimerTick()
        {
            var now = DateTime.Now;
            var deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            if (_isRolling || _particles.Count > 0)
            {
                // Update dice physics
                var stillRolling = false;
                foreach (var dice in _dice)
                {
                    dice.Update(deltaTime, _canvasWidth, _canvasHeight);
                    if (dice.IsRolling)
                        stillRolling = true;
                }

                // Update particles
                for (int i = _particles.Count - 1; i >= 0; i--)
                {
                    _particles[i].Update(deltaTime);
                    if (_particles[i].IsExpired)
                        _particles.RemoveAt(i);
                }

                // Check if rolling finished
                if (_isRolling && !stillRolling && _dice.Count > 0)
                {
                    _isRolling = false;
                    _showResults = true;
                    CreateResultsText();
                    CreateCelebrationParticles();
                    
                    // Update title
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _titleLabel.Text = "?? Results Ready! ??";
                    });
                }

                // Force redraw
                _canvasView.InvalidateSurface();
            }

            return true; // Continue timer
        }

        private void CreateResultsText()
        {
            var results = _dice.GroupBy(d => d.Sides)
                              .Select(g => new { 
                                  Sides = g.Key, 
                                  Values = g.Select(d => d.FinalValue).ToList() 
                              })
                              .ToList();

            var allResults = new List<string>();
            int grandTotal = 0;

            foreach (var group in results)
            {
                int groupTotal = group.Values.Sum();
                grandTotal += groupTotal;

                string diceTypeName = $"D{group.Sides}";
                string groupResult;
                
                if (group.Values.Count == 1)
                {
                    groupResult = $"{diceTypeName}: {group.Values[0]}";
                }
                else
                {
                    string individualRolls = string.Join(", ", group.Values);
                    groupResult = $"{diceTypeName} ({group.Values.Count}x): [{individualRolls}] = {groupTotal}";
                }
                
                allResults.Add(groupResult);
            }

            if (results.Count == 1 && results[0].Values.Count == 1)
            {
                _resultsText = $"Result: {allResults[0]}";
            }
            else
            {
                _resultsText = $"Results:\n{string.Join("\n", allResults)}\n\nGrand Total: {grandTotal}";
            }
        }

        private void CreateCelebrationParticles()
        {
            // Create celebration particles
            for (int i = 0; i < 50; i++)
            {
                _particles.Add(new Particle
                {
                    Position = new Vector3D(_canvasWidth / 2, _canvasHeight / 2, 0),
                    Velocity = new Vector3D(
                        (_random.NextSingle() - 0.5f) * 300f,
                        (_random.NextSingle() - 0.5f) * 300f - 100f,
                        0
                    ),
                    Color = GetRandomParticleColor(),
                    Life = 2.0f + _random.NextSingle() * 2.0f,
                    Size = 3f + _random.NextSingle() * 5f
                });
            }
        }

        private SKColor GetRandomParticleColor()
        {
            var colors = new[] {
                SKColors.Gold, SKColors.Yellow, SKColors.Orange,
                SKColors.Red, SKColors.Pink, SKColors.Purple,
                SKColors.Blue, SKColors.Green, SKColors.White
            };
            return colors[_random.Next(colors.Length)];
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;
            _canvasWidth = info.Width;
            _canvasHeight = info.Height;

            canvas.Clear(SKColors.Black);

            // Draw dynamic background gradient
            DrawBackground(canvas, info);

            // Draw particles (behind dice)
            foreach (var particle in _particles)
            {
                particle.Draw(canvas);
            }

            // Draw dice with enhanced graphics
            foreach (var dice in _dice)
            {
                dice.Draw(canvas);
            }

            // Draw rolling status or results
            if (_isRolling)
            {
                DrawRollingText(canvas, info);
            }
            else if (_showResults)
            {
                DrawResults(canvas, info);
            }
        }

        private void DrawBackground(SKCanvas canvas, SKImageInfo info)
        {
            // Create a dynamic gradient background
            var paint = new SKPaint();
            
            var colors = new SKColor[]
            {
                SKColor.Parse("#0f0f23"),  // Dark blue
                SKColor.Parse("#1a1a2e"),  // Darker blue
                SKColor.Parse("#16213e")   // Deep blue
            };

            var positions = new float[] { 0f, 0.5f, 1f };

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(info.Width, info.Height),
                colors,
                positions,
                SKShaderTileMode.Clamp);

            paint.Shader = shader;
            canvas.DrawRect(info.Rect, paint);

            // Add some subtle texture
            DrawStars(canvas, info);
        }

        private void DrawStars(SKCanvas canvas, SKImageInfo info)
        {
            var paint = new SKPaint
            {
                Color = SKColors.White.WithAlpha(100),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // Draw some random stars for ambiance
            var starCount = 20;
            for (int i = 0; i < starCount; i++)
            {
                var x = (i * 137.5f) % info.Width; // Pseudo-random but consistent
                var y = (i * 219.3f) % info.Height;
                var size = 1f + (i % 3);
                canvas.DrawCircle(x, y, size, paint);
            }
        }

        private void DrawRollingText(SKCanvas canvas, SKImageInfo info)
        {
            var font = new SKFont
            {
                Size = 32,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };

            var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true
            };

            // Pulsing effect
            var pulse = (float)(Math.Sin(DateTime.Now.Millisecond * 0.01) * 0.2 + 0.8);
            paint.Color = SKColors.White.WithAlpha((byte)(255 * pulse));

            canvas.DrawText(
                "Rolling...",
                info.Width / 2,
                info.Height / 2 + 200,
                SKTextAlign.Center,
                font,
                paint
            );
        }

        private void DrawResults(SKCanvas canvas, SKImageInfo info)
        {
            // Semi-transparent background for results
            var bgPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha(180),
                Style = SKPaintStyle.Fill
            };

            var resultRect = new SKRect(50, info.Height / 2 - 100, info.Width - 50, info.Height / 2 + 100);
            canvas.DrawRoundRect(resultRect, 20, 20, bgPaint);

            // Results text
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

            var lines = _resultsText.Split('\n');
            var startY = info.Height / 2 - (lines.Length * 15);

            foreach (var line in lines)
            {
                canvas.DrawText(
                    line,
                    info.Width / 2,
                    startY,
                    SKTextAlign.Center,
                    font,
                    paint
                );
                startY += 35;
            }
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}