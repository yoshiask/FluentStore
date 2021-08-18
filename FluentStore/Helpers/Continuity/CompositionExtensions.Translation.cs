using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace FluentStore.Helpers.Continuity.Extensions
{
    public static partial class CompositionExtensions
    {
        public static void StartTranslationAnimation(this UIElement element, AnimationAxis axis, float? from = null, float to = 0,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null, Action completed = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch = null;

            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = element.Visual();
            var compositor = visual.Compositor;

            if (completed != null)
            {
                batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, e) => completed();
            }

            visual.StartAnimation($"Translation.{axis}", compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, easing, iterationBehavior));

            batch?.End();
        }

        public static void StartTranslationAnimation(this UIElement element, Vector2? from = null, Vector2? to = null,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null, Action completed = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch = null;

            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = element.Visual();
            var compositor = visual.Compositor;

            if (completed != null)
            {
                batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, e) => completed();
            }

            if (to == null)
            {
                to = Vector2.Zero;
            }

            visual.StartAnimation("Translation.XY", compositor.CreateVector2KeyFrameAnimation(from, to.Value, duration, delay, easing, iterationBehavior));

            batch?.End();
        }

        public static void StartTranslationAnimation(this UIElement element, Vector3? from = null, Vector3? to = null,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null, Action completed = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch = null;

            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = element.Visual();
            var compositor = visual.Compositor;

            if (completed != null)
            {
                batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, e) => completed();
            }

            if (to == null)
            {
                to = Vector3.Zero;
            }

            visual.StartAnimation("Translation", compositor.CreateVector3KeyFrameAnimation(from, to.Value, duration, delay, easing, iterationBehavior));

            batch?.End();
        }

        public static Task StartTranslationAnimationAsync(this UIElement element, AnimationAxis axis, float? from = null, float to = 0,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch;

            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = element.Visual();
            var compositor = visual.Compositor;

            var taskSource = new TaskCompletionSource<bool>();

            void Completed(object o, CompositionBatchCompletedEventArgs e)
            {
                batch.Completed -= Completed;
                taskSource.SetResult(true);
            }

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Completed;

            visual.StartAnimation($"Translation.{axis}", compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, easing, iterationBehavior));

            batch.End();

            return taskSource.Task;
        }

        public static Task StartTranslationAnimationAsync(this UIElement element, Vector2? from = null, Vector2? to = null,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch;

            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = element.Visual();
            var compositor = visual.Compositor;

            var taskSource = new TaskCompletionSource<bool>();

            void Completed(object o, CompositionBatchCompletedEventArgs e)
            {
                batch.Completed -= Completed;
                taskSource.SetResult(true);
            }

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Completed;

            if (to == null)
            {
                to = Vector2.Zero;
            }

            visual.StartAnimation("Translation.XY", compositor.CreateVector2KeyFrameAnimation(from, to.Value, duration, delay, easing, iterationBehavior));

            batch.End();

            return taskSource.Task;
        }

        public static Task StartTranslationAnimationAsync(this UIElement element, Vector3? from = null, Vector3? to = null,
            double duration = 800, int delay = 0, CompositionEasingFunction easing = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch;

            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            var visual = element.Visual();
            var compositor = visual.Compositor;

            var taskSource = new TaskCompletionSource<bool>();

            void Completed(object o, CompositionBatchCompletedEventArgs e)
            {
                batch.Completed -= Completed;
                taskSource.SetResult(true);
            }

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Completed;

            if (to == null)
            {
                to = Vector3.Zero;
            }

            visual.StartAnimation("Translation", compositor.CreateVector3KeyFrameAnimation(from, to.Value, duration, delay, easing, iterationBehavior));

            batch.End();

            return taskSource.Task;
        }
    }
}