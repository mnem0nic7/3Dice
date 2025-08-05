using SkiaSharp;

namespace _3Dice.Models
{
    public class Dice3D
    {
        public Vector3D Position { get; set; } = new();
        public Vector3D Velocity { get; set; } = new();
        public Vector3D AngularVelocity { get; set; } = new();
        public Vector3D Rotation { get; set; } = new();
        public int Sides { get; set; }
        public float Size { get; set; } = 30f;
        public SKColor Color { get; set; } = SKColors.White;
        public bool IsRolling { get; set; } = true;
        public int FinalValue { get; set; } = 1;
        
        private readonly Random _random = new();
        private float _bounceTime = 0f;
        private const float Gravity = 980f; // pixels per second squared
        private const float Friction = 0.98f;
        private const float Restitution = 0.3f; // Bounce factor

        public Dice3D(int sides, Vector3D position)
        {
            Sides = sides;
            Position = position;
            
            // Set random initial velocity
            Velocity = new Vector3D(
                (_random.NextSingle() - 0.5f) * 400f,
                (_random.NextSingle() - 0.5f) * 200f - 100f,
                (_random.NextSingle() - 0.5f) * 300f
            );

            // Set random angular velocity
            AngularVelocity = new Vector3D(
                (_random.NextSingle() - 0.5f) * 10f,
                (_random.NextSingle() - 0.5f) * 10f,
                (_random.NextSingle() - 0.5f) * 10f
            );

            // Set color based on dice type
            Color = sides switch
            {
                4 => SKColors.LightGreen,
                6 => SKColors.LightBlue,
                8 => SKColors.Orange,
                10 => SKColors.Purple,
                12 => SKColors.Red,
                20 => SKColors.Gray,
                100 => SKColors.Brown,
                _ => SKColors.White
            };
        }

        public void Update(float deltaTime, float boundsWidth, float boundsHeight)
        {
            if (!IsRolling) return;

            _bounceTime += deltaTime;

            // Apply physics
            Velocity.Y += Gravity * deltaTime; // Gravity
            Position += Velocity * deltaTime;
            Rotation += AngularVelocity * deltaTime;

            // Bounce off bounds
            if (Position.X - Size/2 <= 0 || Position.X + Size/2 >= boundsWidth)
            {
                if (Position.X - Size/2 <= 0) Position.X = Size/2;
                if (Position.X + Size/2 >= boundsWidth) Position.X = boundsWidth - Size/2;
                
                Velocity.X *= -Restitution;
                AngularVelocity.X *= Friction;
            }

            if (Position.Y - Size/2 <= 0 || Position.Y + Size/2 >= boundsHeight)
            {
                if (Position.Y - Size/2 <= 0) Position.Y = Size/2;
                if (Position.Y + Size/2 >= boundsHeight) Position.Y = boundsHeight - Size/2;
                
                Velocity.Y *= -Restitution;
                AngularVelocity.Y *= Friction;
            }

            // Apply friction
            Velocity *= Friction;
            AngularVelocity *= Friction;

            // Stop rolling when velocity is very low and enough time has passed
            if (Velocity.Length < 50f && AngularVelocity.Length < 1f && _bounceTime > 2f)
            {
                IsRolling = false;
                Velocity = new Vector3D();
                AngularVelocity = new Vector3D();
                
                // Calculate final value based on rotation
                FinalValue = CalculateFinalValue();
            }
        }

        private int CalculateFinalValue()
        {
            // Simple calculation based on rotation - in a real implementation,
            // you'd want more sophisticated face detection
            var rotationSum = Math.Abs(Rotation.X) + Math.Abs(Rotation.Y) + Math.Abs(Rotation.Z);
            var value = ((int)(rotationSum * 100) % Sides) + 1;
            return Math.Max(1, Math.Min(Sides, value));
        }

        public void Draw(SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Color = Color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var strokePaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            };

            // Draw 3D cube effect with rotation
            DrawCube(canvas, paint, strokePaint);
            
            // Draw the face value if not rolling
            if (!IsRolling)
            {
                DrawFaceValue(canvas);
            }
        }

        private void DrawCube(SKCanvas canvas, SKPaint fillPaint, SKPaint strokePaint)
        {
            var centerX = Position.X;
            var centerY = Position.Y;
            var halfSize = Size / 2;

            // Calculate 3D projection points (simple isometric projection)
            var rotX = Math.Cos(Rotation.X);
            var rotY = Math.Cos(Rotation.Y);
            var rotZ = Math.Cos(Rotation.Z);

            // Front face
            var frontRect = new SKRect(
                centerX - halfSize,
                centerY - halfSize,
                centerX + halfSize,
                centerY + halfSize
            );

            canvas.DrawRect(frontRect, fillPaint);
            canvas.DrawRect(frontRect, strokePaint);

            // Top face (isometric)
            var topPath = new SKPath();
            topPath.MoveTo(centerX - halfSize, centerY - halfSize);
            topPath.LineTo(centerX, centerY - halfSize - halfSize * 0.5f);
            topPath.LineTo(centerX + halfSize, centerY - halfSize - halfSize * 0.5f);
            topPath.LineTo(centerX + halfSize, centerY - halfSize);
            topPath.Close();

            var topPaint = new SKPaint
            {
                Color = fillPaint.Color.WithAlpha((byte)(fillPaint.Color.Alpha * 0.8)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawPath(topPath, topPaint);
            canvas.DrawPath(topPath, strokePaint);

            // Right face (isometric)
            var rightPath = new SKPath();
            rightPath.MoveTo(centerX + halfSize, centerY - halfSize);
            rightPath.LineTo(centerX + halfSize + halfSize * 0.5f, centerY - halfSize * 0.5f);
            rightPath.LineTo(centerX + halfSize + halfSize * 0.5f, centerY + halfSize * 0.5f);
            rightPath.LineTo(centerX + halfSize, centerY + halfSize);
            rightPath.Close();

            var rightPaint = new SKPaint
            {
                Color = fillPaint.Color.WithAlpha((byte)(fillPaint.Color.Alpha * 0.6)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawPath(rightPath, rightPaint);
            canvas.DrawPath(rightPath, strokePaint);
        }

        private void DrawFaceValue(SKCanvas canvas)
        {
            var font = new SKFont
            {
                Size = Size * 0.4f,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };

            var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            // For simplicity, just use the font size for vertical centering
            var fontSize = font.Size;

            canvas.DrawText(
                FinalValue.ToString(),
                Position.X,
                Position.Y + fontSize / 3, // Simple vertical centering approximation
                SKTextAlign.Center,
                font,
                paint
            );
        }
    }
}