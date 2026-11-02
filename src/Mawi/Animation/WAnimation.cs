using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Animation
{
    public class WAnimation
    {
        public List<WKeyframe> Keyframes = new List<WKeyframe>();

        public double Duration;
        public double Delay;
        public double Iterations = 1;
        public double IterationStart;
        public PlaybackDirection Direction;
        public FillMode Fill;
        public IterationComposite IterationComposite;
        public CompositeOperation Composite;
        public string Easing;

        public void Write(StringBuilder sb)
        {
            sb.Append('[');
            for (int i = 0; i < Keyframes.Count; i++)
            {
                if (i != 0)
                {
                    sb.Append(",");
                }
                Keyframes[i].Write(sb);
            }
            sb.Append("],{");
            
            sb.Append("duration:").Append(Duration.ToString(CultureInfo.InvariantCulture));
            if (Delay != 0)
            {
                sb.Append(",delay:").Append(Delay.ToString(CultureInfo.InvariantCulture));
            }
            if (Iterations != 1)
            {
                sb.Append(",iterations:").Append(Iterations.ToString(CultureInfo.InvariantCulture));
            }
            if (IterationStart != 0)
            {
                sb.Append(",iterationStart:").Append(IterationStart.ToString(CultureInfo.InvariantCulture));
            }
            if (Direction != PlaybackDirection.Normal)
            {
                sb.Append(",direction:'").Append(Direction.ToJsString()).Append("'");
            }
            if (Fill != FillMode.None)
            {
                sb.Append(",fill:'").Append(Fill.ToJsString()).Append("'");
            }
            if (Composite != CompositeOperation.Replace)
            {
                sb.Append(",composite:'").Append(Composite.ToJsString()).Append("'");
            }
            if (IterationComposite != IterationComposite.Replace)
            {
                sb.Append(",iterationComposite:'").Append(IterationComposite.ToJsString()).Append("'");
            }
            if (!string.IsNullOrEmpty(Easing)) 
            { 
                sb.Append(",easing:'").Append(Easing).Append("'");
            }
            sb.Append("}");
        }

        public void SetProperties(object obj)
        {
            if (Fill != FillMode.None)
            {
                var iterEven = Iterations % 2 == 0;

                var forward = Direction == PlaybackDirection.Normal ||
                    (Direction == PlaybackDirection.Alternate && iterEven) ||
                    (Direction == PlaybackDirection.AlternateReverse && !iterEven);

                if (forward && (Fill == FillMode.Both || Fill == FillMode.Forwards))
                {
                    var props = new Dictionary<string, WKeyframe.WProperty>();
                    for (int i = 0; i < Keyframes.Count; i++)
                    {
                        Keyframes[i].ProcessProperties(props);
                    }

                    foreach (var prop in props.Values)
                    {
                        prop.Property.SetValue(obj, prop.OriginalValue);
                    }
                }
                else if (Fill == FillMode.Both || Fill == FillMode.Backwards)
                {
                    var props = new Dictionary<string, WKeyframe.WProperty>();
                    for (int i = Keyframes.Count - 1; i >= 0; i--)
                    {
                        Keyframes[i].ProcessProperties(props);
                    }

                    foreach (var prop in props.Values)
                    {
                        prop.Property.SetValue(obj, prop.OriginalValue);
                    }
                }
            }
        }
    }

    public static class WAnimationExtensions
    {
        public static string ToJsString(this FillMode fillMode)
        {
            return fillMode switch
            {
                FillMode.None => "none",
                FillMode.Forwards => "forwards",
                FillMode.Backwards => "backwards",
                FillMode.Both => "both",
                _ => throw new ArgumentOutOfRangeException(nameof(fillMode), fillMode, null)
            };
        }
        public static string ToJsString(this PlaybackDirection direction)
        {
            return direction switch
            {
                PlaybackDirection.Normal => "normal",
                PlaybackDirection.Reverse => "reverse",
                PlaybackDirection.Alternate => "alternate",
                PlaybackDirection.AlternateReverse => "alternate-reverse",
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static string ToJsString(this IterationComposite composite)
        {
            return composite switch
            {
                IterationComposite.Replace => "replace",
                IterationComposite.Accumulate => "accumulate",
                _ => throw new ArgumentOutOfRangeException(nameof(composite), composite, null)
            };
        }


        public static string ToJsString(this CompositeOperation compositeOperation)
        {
            return compositeOperation switch
            {
                CompositeOperation.Replace => "replace",
                CompositeOperation.Add => "add",
                CompositeOperation.Accumulate => "accumulate",
                _ => throw new NotImplementedException()
            };
        }   
    }


    public static class WEasing
    {
        public const string Linear = "linear";
        public const string Ease = "ease";
        public const string EaseIn = "ease-in";
        public const string EaseOut = "ease-out";
        public const string EaseInOut = "ease-in-out";
        public const string StepStart = "step-start";
        public const string StepEnd = "step-end";
    }

    public enum CompositeOperation
    {
        Replace = 0,
        Add,
        Accumulate
    }

    public enum IterationComposite
    {
        Replace = 0,
        Accumulate
    }

    public enum PlaybackDirection
    {
        Normal = 0,
        Reverse,
        Alternate,
        AlternateReverse
    }

    public enum FillMode
    {
        None = 0,
        Forwards,
        Backwards,
        Both
    }
}
