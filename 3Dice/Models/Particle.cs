using SkiaSharp;

namespace _3Dice.Models
{
    public class Particle
    {
        public Vector3D Position { get; set; } = new();
        public Vector3D Velocity { get; set; } = new();
        public SKColor Color { get; set; } = SKColors.White;
        public float Life { get; set; } = 1.0f;
        public float MaxLife { get; set; } = 1.0f;
        public float Size { get; set; } = 5.0f;
        public bool IsExpired => Life <= 0;
        
        private const float Gravity = 200f;

        public Particle()
        {
            MaxLife = Life;
        }

        public void Update(float deltaTime)
        {
            if (IsExpired) return;

            // Apply simple physics
            Velocity.Y += Gravity * deltaTime;
            Position += Velocity * deltaTime;
            
            // Decay life
            Life -= deltaTime;
        }

        public void Draw(SKCanvas canvas)
        {
            if (IsExpired) return;

            var alpha = (byte)(255 * (Life / MaxLife));
            var particleColor = Color.WithAlpha(alpha);
            
            var paint = new SKPaint
            {
                Color = particleColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // Draw as a small circle with glow
            canvas.DrawCircle(Position.X, Position.Y, Size, paint);
            
            // Add glow effect
            var glowPaint = new SKPaint
            {
                Color = particleColor.WithAlpha((byte)(alpha / 3)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, Size * 0.5f)
            };
            
            canvas.DrawCircle(Position.X, Position.Y, Size * 1.5f, glowPaint);
        }
    }
}