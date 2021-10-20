using System;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

namespace FluentStore.Helpers.Continuity.Extensions
{
    public static partial class CompositionExtensions
    {
        public static void StartRotationAnimation(this UIElement element, Vector3? rotationAxis = null, Vector3? centerPoint = null, 
            float? from = null, float to = 0, double duration = 800, int delay = 0, CompositionEasingFunction easing = null, Action completed = null, 
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch = null;

            var visual = element.Visual();
            var compositor = visual.Compositor;

            if (centerPoint == null)
            {
                var size = element.RenderSize.ToVector2();
                centerPoint = new Vector3(size / 2, 0.0f);
            }
            visual.CenterPoint = centerPoint.Value;

            if (completed != null)
            {
                batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, e) => completed();
            }

            if (rotationAxis.HasValue)
            {
                visual.RotationAxis = rotationAxis.Value;
            }

            visual.StartAnimation("RotationAngleInDegrees", 
                compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, easing, iterationBehavior));

            batch?.End();
        }

        public static void StartRotationAnimation(this Visual visual, Vector3 rotationAxis, Vector3? centerPoint = null, 
            float? from = null, float to = 0, double duration = 800, int delay = 0, CompositionEasingFunction easing = null, Action completed = null, 
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch = null;
            var compositor = visual.Compositor;

            if (centerPoint == null)
            {
                centerPoint = new Vector3(visual.Size / 2, 0.0f);
            }
            visual.CenterPoint = centerPoint.Value;

            if (completed != null)
            {
                batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (s, e) => completed();
            }

            visual.RotationAxis = rotationAxis;

            visual.StartAnimation("RotationAngleInDegrees", 
                compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, easing, iterationBehavior));

            batch?.End();
        }

        public static Task StartRotationAnimationAsync(this UIElement element, Vector3? rotationAxis = null, Vector3? centerPoint = null,
            float? from = null, float to = 0, double duration = 800, int delay = 0, CompositionEasingFunction easing = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch;

            var visual = element.Visual();
            var compositor = visual.Compositor;

            if (centerPoint == null)
            {
                var size = element.RenderSize.ToVector2();
                centerPoint = new Vector3(size / 2, 0.0f);
            }
            visual.CenterPoint = centerPoint.Value;

            var taskSource = new TaskCompletionSource<bool>();

            void Completed(object o, CompositionBatchCompletedEventArgs e)
            {
                batch.Completed -= Completed;
                taskSource.SetResult(true);
            }

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Completed;


            if (rotationAxis.HasValue)
            {
                visual.RotationAxis = rotationAxis.Value;
            }

            visual.StartAnimation("RotationAngleInDegrees",
                compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, easing, iterationBehavior));

            batch.End();

            return taskSource.Task;
        }

        public static Task StartRotationAnimationAsync(this Visual visual, Vector3 rotationAxis, Vector3? centerPoint = null,
            float? from = null, float to = 0, double duration = 800, int delay = 0, CompositionEasingFunction easing = null,
            AnimationIterationBehavior iterationBehavior = AnimationIterationBehavior.Count)
        {
            CompositionScopedBatch batch;
            var compositor = visual.Compositor;

            if (centerPoint == null)
            {
                centerPoint = new Vector3(visual.Size / 2, 0.0f);
            }
            visual.CenterPoint = centerPoint.Value;

            var taskSource = new TaskCompletionSource<bool>();

            void Completed(object o, CompositionBatchCompletedEventArgs e)
            {
                batch.Completed -= Completed;
                taskSource.SetResult(true);
            }

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Completed;


            visual.RotationAxis = rotationAxis;

            visual.StartAnimation("RotationAngleInDegrees",
                compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, easing, iterationBehavior));

            batch.End();

            return taskSource.Task;
        }
    }
}
