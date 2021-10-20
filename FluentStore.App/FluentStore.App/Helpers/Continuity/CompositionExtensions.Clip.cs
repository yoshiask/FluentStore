using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

namespace FluentStore.Helpers.Continuity.Extensions
{
    public static partial class CompositionExtensions
    {
        public static void StartClipAnimation(this FrameworkElement element, ClipAnimationDirection direction, float to,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null, Action completed = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch = null;

            var visual = element.Visual();
            // After we get the Visual of the View, we need to SIZE it 'cause by design the
            // Size is (0,0). Without doing this, clipping will not work.
            visual.Size = element.RenderSize.ToVector2();
            var compositor = visual.Compositor;

            if (visual.Clip == null)
            {
                var clip = compositor.CreateInsetClip();
                visual.Clip = clip;
            }

            if (completed != null)
            {
                batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, e) => completed();
            }

            visual.Clip.StartAnimation($"{direction}Inset",
                compositor.CreateScalarKeyFrameAnimation(null, to, duration, delay, easing, iterationBehavior));

            batch?.End();
        }

        public static void StartClipAnimation(this Visual visual, ClipAnimationDirection direction, float to,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null, Action completed = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch = null;

            if (visual.Size.X.Equals(0) || visual.Size.Y.Equals(0))
            {
                throw new ArgumentException("The visual is not properly sized.");
            }

            var compositor = visual.Compositor;

            if (visual.Clip == null)
            {
                var clip = compositor.CreateInsetClip();
                visual.Clip = clip;
            }

            if (completed != null)
            {
                batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, e) => completed();
            }

            visual.Clip.StartAnimation($"{direction}Inset",
                compositor.CreateScalarKeyFrameAnimation(null, to, duration, delay, easing, iterationBehavior));

            batch?.End();
        }

        public static Task StartClipAnimationAsync(this FrameworkElement element, ClipAnimationDirection direction, float to,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch;

            var visual = element.Visual();
            // After we get the Visual of the View, we need to SIZE it 'cause by design the
            // Size is (0,0). Without doing this, clipping will not work.
            visual.Size = element.RenderSize.ToVector2();
            var compositor = visual.Compositor;

            if (visual.Clip == null)
            {
                var clip = compositor.CreateInsetClip();
                visual.Clip = clip;
            }

            var taskSource = new TaskCompletionSource<bool>();

            void Completed(object o, CompositionBatchCompletedEventArgs e)
            {
                batch.Completed -= Completed;
                taskSource.SetResult(true);
            }

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Completed;

            visual.Clip.StartAnimation($"{direction}Inset",
                compositor.CreateScalarKeyFrameAnimation(null, to, duration, delay, easing, iterationBehavior));

            batch.End();

            return taskSource.Task;
        }

        public static Task StartClipAnimationAsync(this Visual visual, ClipAnimationDirection direction, float to,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch;

            if (visual.Size.X.Equals(0) || visual.Size.Y.Equals(0))
            {
                throw new ArgumentException("The visual is not properly sized.");
            }

            var compositor = visual.Compositor;

            if (visual.Clip == null)
            {
                var clip = compositor.CreateInsetClip();
                visual.Clip = clip;
            }

            var taskSource = new TaskCompletionSource<bool>();

            void Completed(object o, CompositionBatchCompletedEventArgs e)
            {
                batch.Completed -= Completed;
                taskSource.SetResult(true);
            }

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Completed;

            visual.Clip.StartAnimation($"{direction}Inset",
                compositor.CreateScalarKeyFrameAnimation(null, to, duration, delay, easing, iterationBehavior));

            batch.End();

            return taskSource.Task;
        }
    }
}
