using SkiaSharp;

namespace _3Dice.Models
{
    public class EnhancedDice3D
    {
        public Vector3D Position { get; set; } = new();
        public Vector3D Velocity { get; set; } = new();
        public Vector3D AngularVelocity { get; set; } = new();
        public Vector3D Rotation { get; set; } = new();
        public int Sides { get; set; }
        public float Size { get; set; } = 50f;
        public SKColor BaseColor { get; set; } = SKColors.White;
        public bool IsRolling { get; set; } = true;
        public int FinalValue { get; set; } = 1;
        
        private readonly Random _random = new();
        private float _bounceTime = 0f;
        private float _glowIntensity = 0f;
        private const float Gravity = 1200f; // Stronger gravity for more dramatic effect
        private const float Friction = 0.985f;
        private const float Restitution = 0.4f;

        public EnhancedDice3D(int sides, Vector3D position)
        {
            Sides = sides;
            Position = position;
            
            // More dramatic initial velocities
            Velocity = new Vector3D(
                (_random.NextSingle() - 0.5f) * 600f,
                (_random.NextSingle() - 0.5f) * 400f - 200f,
                (_random.NextSingle() - 0.5f) * 400f
            );

            // More spinning
            AngularVelocity = new Vector3D(
                (_random.NextSingle() - 0.5f) * 15f,
                (_random.NextSingle() - 0.5f) * 15f,
                (_random.NextSingle() - 0.5f) * 15f
            );

            // Enhanced colors with better visual appeal
            BaseColor = sides switch
            {
                4 => SKColor.Parse("#4CAF50"),   // Material Green
                6 => SKColor.Parse("#2196F3"),   // Material Blue  
                8 => SKColor.Parse("#FF9800"),   // Material Orange
                10 => SKColor.Parse("#9C27B0"),  // Material Purple
                12 => SKColor.Parse("#F44336"),  // Material Red
                20 => SKColor.Parse("#607D8B"),  // Material Blue Grey
                100 => SKColor.Parse("#795548"), // Material Brown
                _ => SKColor.Parse("#FFFFFF")    // White
            };
        }

        public void Update(float deltaTime, float boundsWidth, float boundsHeight)
        {
            if (!IsRolling) return;

            _bounceTime += deltaTime;

            // Apply physics with enhanced effects
            Velocity.Y += Gravity * deltaTime;
            Position += Velocity * deltaTime;
            Rotation += AngularVelocity * deltaTime;

            // Calculate table surface position (70% down the screen to create perspective)
            var tableSurface = boundsHeight * 0.7f;
            var bounced = false;
            
            // Side walls collision (invisible walls to keep dice on screen)
            if (Position.X - Size/2 <= 0 || Position.X + Size/2 >= boundsWidth)
            {
                if (Position.X - Size/2 <= 0) Position.X = Size/2;
                if (Position.X + Size/2 >= boundsWidth) Position.X = boundsWidth - Size/2;
                
                Velocity.X *= -Restitution;
                AngularVelocity.X *= Friction;
                bounced = true;
            }

            // Table surface collision (this is the rolling surface)
            if (Position.Y + Size/2 >= tableSurface)
            {
                Position.Y = tableSurface - Size/2;
                
                // More realistic table bounce
                if (Velocity.Y > 0)
                {
                    Velocity.Y *= -Restitution;
                    AngularVelocity.Y *= Friction;
                    bounced = true;
                }
            }

            // Top boundary (prevent dice from going off-screen upward)
            if (Position.Y - Size/2 <= 0)
            {
                Position.Y = Size/2;
                if (Velocity.Y < 0)
                {
                    Velocity.Y *= -Restitution;
                }
            }

            // Create glow effect on bounce
            if (bounced)
            {
                _glowIntensity = 1.0f;
            }

            // Decay glow
            _glowIntensity *= 0.95f;

            // Apply friction (stronger on table surface)
            var frictionMultiplier = (Position.Y + Size/2 >= tableSurface - 5) ? 0.95f : 1.0f;
            Velocity *= Friction * frictionMultiplier;
            AngularVelocity *= Friction * frictionMultiplier;

            // Stop rolling when velocity is very low and dice is on table surface
            if (Velocity.Length < 30f && AngularVelocity.Length < 0.5f && 
                _bounceTime > 3f && Position.Y + Size/2 >= tableSurface - 5)
            {
                IsRolling = false;
                Velocity = new Vector3D();
                AngularVelocity = new Vector3D();
                Position.Y = tableSurface - Size/2; // Ensure dice sits perfectly on table
                FinalValue = CalculateFinalValue();
                _glowIntensity = 0.3f; // Final glow when settled
            }
        }

        private int CalculateFinalValue()
        {
            // Better final value calculation based on rotation
            var rotationSum = Math.Abs(Rotation.X) + Math.Abs(Rotation.Y) + Math.Abs(Rotation.Z);
            var normalizedRotation = (rotationSum * 1000) % 1000;
            var value = ((int)(normalizedRotation * Sides / 1000)) + 1;
            return Math.Max(1, Math.Min(Sides, value));
        }

        public void Draw(SKCanvas canvas)
        {
            DrawShadow(canvas);
            DrawDice(canvas);
            
            if (!IsRolling)
            {
                DrawFaceValue(canvas);
            }
            
            if (_glowIntensity > 0.01f)
            {
                DrawGlow(canvas);
            }
        }

        private void DrawShadow(SKCanvas canvas)
        {
            // Calculate shadow position based on table surface
            var tableSurface = canvas.LocalClipBounds.Height * 0.7f;
            var shadowY = tableSurface + 5; // Shadow slightly below table surface
            var shadowOffset = Math.Max(2f, (tableSurface - Position.Y) * 0.1f); // Shadow size based on height
            
            var shadowPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha((byte)(80 * Math.Max(0.2f, 1.0f - (tableSurface - Position.Y) / 200f))),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadowOffset)
            };

            var shadowRect = new SKRect(
                Position.X - Size/2 - shadowOffset,
                shadowY - shadowOffset,
                Position.X + Size/2 + shadowOffset,
                shadowY + shadowOffset
            );

            canvas.DrawOval(shadowRect, shadowPaint);
        }

        private void DrawDice(SKCanvas canvas)
        {
            var centerX = Position.X;
            var centerY = Position.Y;
            var halfSize = Size / 2;

            // Enhanced 3D projection with better depth
            var rotX = Rotation.X;
            var rotY = Rotation.Y;
            var rotZ = Rotation.Z;

            // Calculate 3D transformation points
            var depth = halfSize * 0.7f;
            
            // Front face with gradient
            var frontRect = new SKRect(
                centerX - halfSize,
                centerY - halfSize,
                centerX + halfSize,
                centerY + halfSize
            );

            var frontGradient = SKShader.CreateLinearGradient(
                new SKPoint(frontRect.Left, frontRect.Top),
                new SKPoint(frontRect.Right, frontRect.Bottom),
                new SKColor[] { BaseColor, BaseColor.WithAlpha(200) },
                null,
                SKShaderTileMode.Clamp);

            var frontPaint = new SKPaint
            {
                Shader = frontGradient,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawRoundRect(frontRect, Size * 0.1f, Size * 0.1f, frontPaint);

            // Top face (isometric with lighting)
            var topPath = new SKPath();
            topPath.MoveTo(centerX - halfSize, centerY - halfSize);
            topPath.LineTo(centerX - halfSize + depth * 0.5f, centerY - halfSize - depth * 0.5f);
            topPath.LineTo(centerX + halfSize + depth * 0.5f, centerY - halfSize - depth * 0.5f);
            topPath.LineTo(centerX + halfSize, centerY - halfSize);
            topPath.Close();

            var topColor = BaseColor.WithAlpha((byte)(BaseColor.Alpha * 0.9));
            var topPaint = new SKPaint
            {
                Color = topColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawPath(topPath, topPaint);

            // Right face (isometric with shading)
            var rightPath = new SKPath();
            rightPath.MoveTo(centerX + halfSize, centerY - halfSize);
            rightPath.LineTo(centerX + halfSize + depth * 0.5f, centerY - halfSize - depth * 0.5f);
            rightPath.LineTo(centerX + halfSize + depth * 0.5f, centerY + halfSize - depth * 0.5f);
            rightPath.LineTo(centerX + halfSize, centerY + halfSize);
            rightPath.Close();

            var rightColor = BaseColor.WithAlpha((byte)(BaseColor.Alpha * 0.7));
            var rightPaint = new SKPaint
            {
                Color = rightColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawPath(rightPath, rightPaint);

            // Enhanced edges with better definition
            var strokePaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha(150),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            };

            canvas.DrawRoundRect(frontRect, Size * 0.1f, Size * 0.1f, strokePaint);
            canvas.DrawPath(topPath, strokePaint);
            canvas.DrawPath(rightPath, strokePaint);

            // Add dice dots/pips for better visual appeal
            if (!IsRolling)
            {
                DrawDicePips(canvas, frontRect);
            }
        }

        private void DrawDicePips(SKCanvas canvas, SKRect faceRect)
        {
            if (Sides != 6) return; // Only draw pips for D6

            var pipPaint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var pipSize = Size * 0.08f;
            var centerX = faceRect.MidX;
            var centerY = faceRect.MidY;
            var offset = Size * 0.2f;

            // Draw pips based on face value
            switch (FinalValue)
            {
                case 1:
                    canvas.DrawCircle(centerX, centerY, pipSize, pipPaint);
                    break;
                case 2:
                    canvas.DrawCircle(centerX - offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY + offset, pipSize, pipPaint);
                    break;
                case 3:
                    canvas.DrawCircle(centerX - offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX, centerY, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY + offset, pipSize, pipPaint);
                    break;
                case 4:
                    canvas.DrawCircle(centerX - offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX - offset, centerY + offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY + offset, pipSize, pipPaint);
                    break;
                case 5:
                    canvas.DrawCircle(centerX - offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX, centerY, pipSize, pipPaint);
                    canvas.DrawCircle(centerX - offset, centerY + offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY + offset, pipSize, pipPaint);
                    break;
                case 6:
                    canvas.DrawCircle(centerX - offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY - offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX - offset, centerY, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY, pipSize, pipPaint);
                    canvas.DrawCircle(centerX - offset, centerY + offset, pipSize, pipPaint);
                    canvas.DrawCircle(centerX + offset, centerY + offset, pipSize, pipPaint);
                    break;
            }
        }

        private void DrawFaceValue(SKCanvas canvas)
        {
            // Only draw numbers for non-D6 dice
            if (Sides == 6) return;

            var font = new SKFont
            {
                Size = Size * 0.4f,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };

            var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true
            };

            // Add text shadow for better readability
            var shadowPaint = new SKPaint
            {
                Color = SKColors.Black.WithAlpha(150),
                IsAntialias = true
            };

            var fontSize = font.Size;

            // Draw shadow
            canvas.DrawText(
                FinalValue.ToString(),
                Position.X + 2,
                Position.Y + fontSize / 3 + 2,
                SKTextAlign.Center,
                font,
                shadowPaint
            );

            // Draw main text
            canvas.DrawText(
                FinalValue.ToString(),
                Position.X,
                Position.Y + fontSize / 3,
                SKTextAlign.Center,
                font,
                paint
            );
        }

        private void DrawGlow(SKCanvas canvas)
        {
            var glowPaint = new SKPaint
            {
                Color = BaseColor.WithAlpha((byte)(100 * _glowIntensity)),
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, Size * 0.3f * _glowIntensity)
            };

            var glowRect = new SKRect(
                Position.X - Size/2,
                Position.Y - Size/2,
                Position.X + Size/2,
                Position.Y + Size/2
            );

            canvas.DrawRoundRect(glowRect, Size * 0.1f, Size * 0.1f, glowPaint);
        }
    }
}