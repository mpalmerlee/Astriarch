using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace SystemMaster.ControlLibrary
{
    public class IntegerTickSlider : Slider
    {
        private Thumb _HorizontalThumb;
        private FrameworkElement left;
        private FrameworkElement right;

        public IntegerTickSlider() 
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _HorizontalThumb = GetTemplateChild("HorizontalThumb") as Thumb;

            left = GetTemplateChild("LeftTrack") as FrameworkElement;
            right = GetTemplateChild("RightTrack") as FrameworkElement;

            if (left != null) left.MouseLeftButtonDown += new MouseButtonEventHandler(OnMoveThumbToMouse);
            if (right != null) right.MouseLeftButtonDown += new MouseButtonEventHandler(OnMoveThumbToMouse);
        }

        private void OnMoveThumbToMouse(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            double value = (p.X - (_HorizontalThumb.ActualWidth / 2)) / (ActualWidth - _HorizontalThumb.ActualWidth) * Maximum;
            if (value != Value)
                Value = value;
        }

        //from:
        //http://forums.silverlight.net/forums/p/131164/293048.aspx#293048
        //and referenced from:
        //http://forums.silverlight.net/forums/p/65041/205301.aspx#205301
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            double minimum = Minimum;
            double maximum = Maximum;

            double smallChange = SmallChange;
            double value = Value;

            double snapValue = 0;
            if (SmallChange != 0)
            {
                snapValue = (Math.Round((value - minimum) / smallChange) * smallChange) + minimum;

                if(snapValue != value)
                    SetValue(ValueProperty, snapValue);
            }
            if (value < minimum)
            {
                SetValue(ValueProperty, minimum);
            }
            if (value > maximum)
            {
                SetValue(ValueProperty, maximum);
            }

        }
    }
}
