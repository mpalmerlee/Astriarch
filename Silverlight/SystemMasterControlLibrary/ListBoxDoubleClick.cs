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
using System.Threading;

namespace SystemMaster.ControlLibrary
{
    

    //http://www.cnblogs.com/jeromejiang/archive/2009/08/04/1538059.html
    public class ListBoxDoubleClick
    {
        //public event MouseButtonEventHandler Click;
        public event MouseButtonEventHandler DoubleClick;

        private bool clicked { get; set; }

        public FrameworkElement Element { get; set; }

        public int Timeout { get; set; }

        private long lastClickedTime = 0;

        private Point lastClickedPoint = new Point();

        public ListBoxDoubleClick(FrameworkElement control)
        {
            this.clicked = false;
            this.Element = control;

            this.Timeout = 500;

            this.Element.MouseLeftButtonUp += new MouseButtonEventHandler(HandleClick);
        }

        public void HandleClick(object sender, MouseButtonEventArgs e)
        {
            long currentMillis = DateTime.Now.Ticks / 10000;

            if (this.clicked)
            {
                this.clicked = false;
                if (currentMillis - this.lastClickedTime < this.Timeout && this.lastPointClose(e.GetPosition(null)))
                {
                    OnDoubleClick(sender, e);    
                }
            }
            else
            {
                this.clicked = true;
                lastClickedTime = DateTime.Now.Ticks / 10000;
            }
            this.lastClickedPoint = e.GetPosition(null);
        }

        private bool lastPointClose(Point p)
        {
            bool bRet = false;

            if (Math.Abs(p.X - this.lastClickedPoint.X) < 3 && Math.Abs(p.Y - this.lastClickedPoint.Y) < 3)
            {
                bRet = true;
            }
            return bRet;
        }

        /*
        private void ResetThread(object state)
        {
            Thread.Sleep(this.Timeout);

            lock (this)
            {
                if (this.Clicked)
                {
                    this.Clicked = false;
                    OnClick(this, (MouseButtonEventArgs)state);
                }
            }
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler handler = Click;

            if (handler != null)
                this.Element.Dispatcher.BeginInvoke(handler, sender, e);
        }
         * */

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler handler = DoubleClick;

            if (handler != null)
                handler(sender, e);
        }
    }
    /*
    //http://compiledexperience.com/blog/posts/Silverlight-3-Behaviors-Double-Click-Trigger
    public class DoubleClickTrigger : TriggerBase<UIElement>
    {

        private readonly DispatcherTimer timer;

        public DoubleClickTrigger()
        {
            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 200)
            };

            timer.Tick += OnTimerTick;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += OnMouseButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseLeftButtonDown -= OnMouseButtonDown;

            if (timer.IsEnabled)
                timer.Stop();
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
                return;
            }

            timer.Stop();

            InvokeActions(null);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            timer.Stop();
        }
    }
     * */
}
