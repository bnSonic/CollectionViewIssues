using System;
using System.Diagnostics;
using Android.Views;
using CollectionViewIssues.Components;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using View = Android.Views.View;


namespace CollectionViewIssues.Platforms.Android.Handler;

public class GridWithSwipeHandler : LayoutHandler
{
    GestureDetector _detector;
    HorizontalPanListener _listener;

    
    protected override LayoutViewGroup CreatePlatformView()
    {
        var platformView = new SwipeInterceptLayout(Context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
        };

        _listener = new HorizontalPanListener();
        _detector = new GestureDetector(Context, _listener);
        platformView.SetGestureDetector(_detector);

        platformView.Touch += OnTouch;

        return platformView;
    }
    // 2. Wieder ablösen (wichtig bei Recycling)
    protected override void DisconnectHandler(LayoutViewGroup platformView)
    {
        platformView?.SetOnTouchListener(null);
        platformView.Touch -= OnTouch;
        base.DisconnectHandler(platformView);
    }

    private void OnTouch(object sender, View.TouchEventArgs e)
    {
        _detector.OnTouchEvent(e.Event);

        if (e.Event.Action == MotionEventActions.Up || e.Event.Action == MotionEventActions.Cancel)
        {
            // Finger wurde losgelassen
            // oder Geste wurde abgebrochen (wichtig bei Grid in einer ScrollView/CollectionView; 
            // hier wird die Geste abgebrochen wenn rauf/runter gescrollt wird - wir wollen aber
            // dann trotzdem die horizontale Geste erkennen und auswerten)
            if (_listener != null)
            {
                // kurios: X ist POSITIV wenn nach links gewischt wird und NEGATIV wenn nach rechts gewischt wird
                // Das ist genau umgekehrt wie bei iOS, wo X positiv ist wenn nach rechts gewischt wird
                // und negativ wenn nach links gewischt wird.
                if (_listener.TotalX > 30)
                {
                    // Swipe nach links
                    Debug.WriteLine("Swipe nach links erkannt");
                    HandleSwipeLeft(sender, EventArgs.Empty);
                }
                else if (_listener.TotalX < -30)
                {
                    // Swipe nach rechts
                    Debug.WriteLine("Swipe nach rechts erkannt");
                    HandleSwipeRight(sender, EventArgs.Empty);
                }
                else
                {
                    Debug.WriteLine("Kein signifikanter Swipe erkannt");
                }
            }
        }
    }


    // ────────────────────────────────────────────────────────────
    // Kleiner Listener: erkennt "fast horizontale" Bewegung.
    // ────────────────────────────────────────────────────────────
    class HorizontalPanListener : Java.Lang.Object, GestureDetector.IOnGestureListener
    {
        float _startX, _startY;
        bool _disallowed;

        public float TotalX { get; set; }
        
        public bool OnDown(MotionEvent e)
        {
            TotalX = 0;
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {

        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            TotalX += distanceX;
            System.Diagnostics.Debug.WriteLine($"HorizontalPanListener: OnScroll: TotalX={TotalX}, distanceX={distanceX}, distanceY={distanceY}");
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {

        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    _startX = e.GetX();
                    _startY = e.GetY();
                    break;

                case MotionEventActions.Move:
                    var dx = Math.Abs(e.GetX() - _startX);
                    var dy = Math.Abs(e.GetY() - _startY);

                    if (!_disallowed && dx > dy * 1.2f)
                    {
                        v.Parent?.RequestDisallowInterceptTouchEvent(true);
                        _disallowed = true;
                    }
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    if (_disallowed)
                        v.Parent?.RequestDisallowInterceptTouchEvent(false);
                    _disallowed = false;
                    break;
            }
            // false = Event nicht verbrauchen (PanGestureRecognizer feuert weiter)
            return false;
        }
    }



    void HandleSwipeLeft(object sender, EventArgs e) => (VirtualView as GridWithSwipe)?.OnSwipeLeft();
    void HandleSwipeRight(object sender, EventArgs e) => (VirtualView as GridWithSwipe)?.OnSwipeRight();


}