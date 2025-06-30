using Grid = Microsoft.Maui.Controls.Grid;

namespace CollectionViewIssues.Components
{
    /// <summary>
    /// Android:
    /// Android verhält sich anders. Hier funktionert das Scrollen einer CollectionView etc. immer, aber die 
    /// PanGesture wird nicht weitergereicht und kommt hier beim Grid einfach niemals an. Ich habe keine 
    /// Plattform-Property gefunden (wie oben für iOS beschrieben) um das zu ändern - unklar, warum MAUI sich
    /// hier auf den Plattformen unterschiedlich verhält)
    /// 
    /// Die Lösung ist ein Android-Spezifischer Handler! 
    /// Siehe DFGridWithSwipeHandler im Droid-Projekt.
    /// Der Handler muss in MainApplication aktiviert werden:
    ///     builder.ConfigureMauiHandlers((handlers) =>
    ///     {
    ///       handlers.AddHandler(typeof(DFGridWithSwipe), typeof(DFGridWithSwipeHandler));
    ///     }
    /// Der Handler nutzt dann Plattform-Spezifischen Code um eine Geste als Swipe-Geste zu erkennen und 
    /// unsere eigenen SwipeLeft/SwipeRight Befehle aufzurufen
    /// </summary>
    public class GridWithSwipe : Grid
    {
        public event EventHandler SwipeLeft;
        public event EventHandler SwipeRight;

        public void OnSwipeLeft() =>
            SwipeLeft?.Invoke(this, null);

        public void OnSwipeRight() =>
            SwipeRight?.Invoke(this, null);


        public GridWithSwipe() : base()
        {
#if IOS       
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            this.GestureRecognizers.Add(panGesture);
#endif            
        }

#if IOS
        double _panTotalX = 0;
        double _panTotalY = 0;
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _panTotalX = 0;
                    _panTotalY = 0;
                    break;
                case GestureStatus.Running:
                    _panTotalX += e.TotalX;
                    _panTotalY += e.TotalY;
                    break;
                case GestureStatus.Completed:
                    // Nur behandeln, wenn horizontal stärker war
                    if (Math.Abs(_panTotalX) > Math.Abs(_panTotalY))
                    {
                       if (_panTotalX < -30) // swipe left
                            OnSwipeLeft();
                        else if (_panTotalX > 30) // swipe right
                            OnSwipeRight();
                    }
                    break;
            }
        }
#endif


    }
}
 