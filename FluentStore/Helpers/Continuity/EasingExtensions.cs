using System.Numerics;
using Windows.UI.Composition;

namespace FluentStore.Helpers.Continuity.Extensions
{
    public static class EasingExtensions
    {
        #region Cubic

        public static CubicBezierEasingFunction EaseInCubic(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.55f, 0.055f), new Vector2(0.675f, 0.19f));
        }

        public static CubicBezierEasingFunction EaseOutCubic(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.215f, 0.61f), new Vector2(0.355f, 1.0f));
        }

        public static CubicBezierEasingFunction EaseInOutCubic(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.645f, 0.045f), new Vector2(0.355f, 1.0f));
        }

        #endregion

        #region Back

        public static CubicBezierEasingFunction EaseInBack(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.6f, -0.28f), new Vector2(0.735f, 0.045f));
        }

        public static CubicBezierEasingFunction EaseOutBack(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.175f, 0.885f), new Vector2(0.32f, 1.275f));
        }

        public static CubicBezierEasingFunction EaseOutStrongBack(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.175f, 0.885f), new Vector2(0.52f, 3.275f));
        }

        public static CubicBezierEasingFunction EaseInOutBack(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.68f, -0.55f), new Vector2(0.265f, 1.55f));
        }

        #endregion

        #region Quad

        public static CubicBezierEasingFunction EaseInSine(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.47f, 0f), new Vector2(0.745f, 0.715f));
        }

        public static CubicBezierEasingFunction EaseOutSine(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.39f, 0.575f), new Vector2(0.565f, 1.0f));
        }

        public static CubicBezierEasingFunction EaseInOutSine(this Compositor compositor)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2(0.445f, 0.05f), new Vector2(0.55f, 0.95f));
        }

        #endregion
    }
}
