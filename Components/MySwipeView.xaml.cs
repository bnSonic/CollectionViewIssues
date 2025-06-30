namespace CollectionViewIssues.Components;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MySwipeView : ContentView
{
	public static readonly BindableProperty ContextViewProperty = BindableProperty.Create(
           nameof(ContextView), typeof(View), typeof(MySwipeView), null,
           propertyChanged: OnContextViewPropertyChanged);

        static void OnContextViewPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var me = ((MySwipeView)bindable);
            if (me == null)
                return;

            //me.xamlContextView.Content = (View)newValue;

            var grid = me.xamlGrid;
            var contextView = newValue as View;

            // Entferne alte ContextView, falls vorhanden
            if (oldValue is View oldContext && grid.Children.Contains(oldContext))
                grid.Children.Remove(oldContext);

            if (contextView != null)
            {
                // ContextView immer an Index 0 einfügen
                grid.Children.Insert(0, contextView);
                Grid.SetColumn(contextView, 0);
            }
        }

        public View ContextView
        {
            get => (View)GetValue(ContextViewProperty);
            set => SetValue(ContextViewProperty, value);
        }

        public static readonly BindableProperty MainViewProperty = BindableProperty.Create(
           nameof(MainView), typeof(View), typeof(MySwipeView), null,
           propertyChanged: OnMainViewPropertyChanged);

        static void OnMainViewPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var me = ((MySwipeView)bindable);
            if (me == null)
                return;

            //me.xamlMainView.Content = (View)newValue;
            var grid = me.xamlGrid;
            var mainView = newValue as View;

            // Entferne alte MainView, falls vorhanden
            if (oldValue is View oldMain && grid.Children.Contains(oldMain))
                grid.Children.Remove(oldMain);

            if (mainView != null)
            {
                // MainView immer an Index 1 einfügen (über ContextView)
                // Falls ContextView noch nicht da ist, einfach anhängen
                if (grid.Children.Count == 0)
                    grid.Children.Add(mainView);
                else if (grid.Children.Count == 1)
                    grid.Children.Add(mainView);
                else
                    grid.Children.Insert(1, mainView);

                Grid.SetColumn(mainView, 0);
            }
        }

        public View MainView
        {
            get => (View)GetValue(MainViewProperty);
            set => SetValue(MainViewProperty, value);
        }

        public MySwipeView()
        {
            InitializeComponent();
        }

        private MyCollectionView MyCollectionView
        {
            get
            {
                var parent = this.Parent;
                while (parent != null)
                {
                    if (parent is MyCollectionView v)
                        return v;

                    parent = parent.Parent;
                }
                return null;
            }
        }

        protected override async void OnBindingContextChanged()
        {

            try
            {
                if (_isRevealed)
                {
                    SwipeRightCommand.Execute(false);
                }
            }
            catch
            { }

            base.OnBindingContextChanged();
        }

        private bool _isRevealed = false;

        private Command _swipeLeftCommand;
        /// <summary>
        /// Parameter: true wenn animiert werden soll, sonst false
        /// </summary>
        public Command SwipeLeftCommand => _swipeLeftCommand ??= new Command(async (animated) =>
        {
            if (_isRevealed)
                return;

            //-- schon ein anderes Grid revealed? dann dieses schließen
            if (MyCollectionView != null && MyCollectionView.CurrentRevealedSwipeGrid is MySwipeView view)
            {
                //await view.SwipeRightCommand.ExecuteAsync(true);
                _ = view.CloseMenu();
            }

            //-- Breite der Buttons
            //var x = xamlContextView?.Content != null ? xamlContextView.Content.Width : 0;
            var x = this.ContextView != null ? this.ContextView.Width : 0;

            //-- CellView nach links verschieben um Buttons sichtbar zu machen
            if (this.MainView != null)
            {
                await this.MainView.TranslateTo(x * -1, 0);
                _isRevealed = true;

                if (MyCollectionView != null)
                {
                    MyCollectionView.CurrentRevealedSwipeGrid = this;
                }
            }
        });

        private Command _swipeRightCommand;
        /// <summary>
        /// Parameter: true wenn animiert werden soll, sonst false
        /// </summary>
        public Command SwipeRightCommand => _swipeRightCommand ??= new Command(async (animated) =>
        {
            if (!_isRevealed)
                return;

            bool anim = (bool)animated;
            if (this.MainView != null)
            {
                if (anim)
                {
                    //-- CellView nach links verschieben um Buttons sichtbar zu machen
                    await this.MainView.TranslateTo(0, 0);
                }
                else
                {
                    this.MainView.TranslationX = 0;
                }
            }
            _isRevealed = false;

            if (MyCollectionView != null)
            {
                MyCollectionView.CurrentRevealedSwipeGrid = null;
            }
        });
        
        private async Task CloseMenu(bool animated = true)
        {
            if (!_isRevealed)
                return;

            if (this.MainView != null)
            {
                if (animated)
                {
                    //-- CellView nach links verschieben um Buttons sichtbar zu machen
                    await this.MainView.TranslateTo(0, 0);
                }
                else
                {
                    this.MainView.TranslationX = 0;
                }
            }
            _isRevealed = false;
        }

        private void DFGridWithSwipe_SwipeLeft(object sender, EventArgs e)
        {
            SwipeLeftCommand.Execute(true);
        }
        private void DFGridWithSwipe_SwipeRight(object sender, EventArgs e)
        {
            SwipeRightCommand.Execute(true);
        }
}