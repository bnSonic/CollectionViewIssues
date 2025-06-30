using System;
using System.Diagnostics;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Platform;

namespace CollectionViewIssues.Platforms.Android.Handler;

public class SwipeInterceptLayout : LayoutViewGroup
{
    private GestureDetector _detector;
    
    public SwipeInterceptLayout(Context context) : base(context) { }
    
    public void SetGestureDetector(GestureDetector detector)
    {
        _detector = detector;
    }
    
    public override bool DispatchTouchEvent(MotionEvent ev)
    {
        _detector?.OnTouchEvent(ev);

        return base.DispatchTouchEvent(ev);
    }

    float _startX, _startY;
    bool _isSwiping;
    public override bool OnInterceptTouchEvent(MotionEvent ev)
    {
        //return false;
        switch (ev.Action)
        {
            case MotionEventActions.Down:
                _startX = ev.GetX();
                _startY = ev.GetY();
                _isSwiping = false;
                break;
            case MotionEventActions.Move:
                float dx = Math.Abs(ev.GetX() - _startX);
                float dy = Math.Abs(ev.GetY() - _startY);

                if (dx > dy && dx > 30) // 30 = Schwelle für horizontale Bewegung
                {
                    _isSwiping = true;
                    return true; // Intercept: Wir übernehmen das Event!
                }

                break;
        }
        return base.OnInterceptTouchEvent(ev);
    }

}