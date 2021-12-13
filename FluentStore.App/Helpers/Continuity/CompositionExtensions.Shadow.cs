using System;
using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Windows.UI;

namespace FluentStore.Helpers.Continuity.Extensions
{
    public static partial class CompositionExtensions
    {
        public static void StartDropShadowBlurRadiusAnimation(this DropShadow shadow, UIElement shadowContainer, Color shadowColor, 
            Vector3? shadowOffset = null, float shadowOpacity = 1.0f, float blurRadius = 16.0f, int duration = 800)
        {
            var compositor = shadow.Compositor;

            shadow.Color = shadowColor;

            if (shadowOffset == null)
            {
                shadowOffset = Vector3.Zero;
            }

            shadow.Opacity = shadowOpacity;

            var shadowBlurAnimation = compositor.CreateScalarKeyFrameAnimation();
            shadowBlurAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            shadowBlurAnimation.InsertKeyFrame(1.0f, blurRadius);
            shadow.StartAnimation("BlurRadius", shadowBlurAnimation);

            var shadowOffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            shadowOffsetAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            shadowOffsetAnimation.InsertKeyFrame(1.0f, shadowOffset.Value);
            shadow.StartAnimation("Offset", shadowOffsetAnimation);

            var sprite = compositor.CreateSpriteVisual();
            sprite.Size = shadowContainer.RenderSize.ToVector2();
            sprite.Shadow = shadow;

            ElementCompositionPreview.SetElementChildVisual(shadowContainer, sprite);
        }

        public static void StopDropShadowBlurRadiusAnimation(this DropShadow shadow, FrameworkElement shadowContainer, int duration = 400)
        {
            var compositor = shadow.Compositor;

            var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) => ElementCompositionPreview.SetElementChildVisual(shadowContainer, null);

            var shadowBlurAnimation = compositor.CreateScalarKeyFrameAnimation();
            shadowBlurAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            shadowBlurAnimation.InsertKeyFrame(1.0f, 0.0f);
            shadow.StartAnimation("BlurRadius", shadowBlurAnimation);

            var shadowOffsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            shadowOffsetAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            shadowOffsetAnimation.InsertKeyFrame(1.0f, Vector3.Zero);
            shadow.StartAnimation("Offset", shadowOffsetAnimation);

            batch.End();
        }
    }
}
