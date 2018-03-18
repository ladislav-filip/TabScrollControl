using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TabScrollControl
{
    public partial class TabScrollContainer
    {
        /// <summary>
        /// Rozhraní s kontejnerem obsahujícím obsah sekce
        /// </summary>
        public interface IContainer
        {
            FrameworkElement Sekce { get; set; }
        }

        /// <summary>
        /// Kontejner pro sekce ve scrollu
        /// </summary>
        public class ScrollContainer : StackPanel, IContainer
        {
            private readonly Expander m_container;

            public ScrollContainer()
            {
                Orientation = Orientation.Vertical;
                var br = new Border();
                m_container = new Expander {IsExpanded = true};
                br.Child = m_container;
                Children.Add(br);
            }

            public FrameworkElement Sekce
            {
                get => m_container.Content as FrameworkElement;
                set
                {
                    // nastavím titulek                     
                    m_container.Header = value?.DataContext.ToString() ?? string.Empty;
                    m_container.Content = value;
                }
            }
        }

        /// <summary>
        /// Kontejner pro sekce v tabech
        /// </summary>
        public class TabContainer : TabItem, IContainer
        {
            public TabContainer()
            {
            }

            public FrameworkElement Sekce
            {
                get => Content as FrameworkElement;
                set
                {
                    Header = value?.DataContext.ToString() ?? string.Empty;
                    Content = value;
                }
            }
        }

        /// <summary>
        /// Wrapper na sekce a jejich zobrazení v kontejnerech
        /// </summary>
        public class SekceWrapper
        {
            public object SekceViewModel { get; set; }

            public FrameworkElement SekceView { get; set; }

            public IContainer Tab { get; set; }

            public IContainer Scroll { get; set; }
        }
    

        /// <summary>
        /// připravený seznam sekcí, pro zobrazení v komponentě
        /// </summary>
        private List<SekceWrapper> m_viewSections = new List<SekceWrapper>();

        public static readonly DependencyProperty SekceAsTabProperty = DependencyProperty.Register("SekceAsTab", typeof(bool), typeof(TabScrollContainer), new PropertyMetadata(OnSekceAsTabChanged));

        public static readonly DependencyProperty SekceProperty = DependencyProperty.Register("Sekce", typeof(List<object>), typeof(TabScrollContainer), new PropertyMetadata(OnSekceChanged));

        public static readonly DependencyProperty IndexVisibleProperty = DependencyProperty.Register("IndexVisible", typeof(bool), typeof(TabScrollContainer), new PropertyMetadata(OnIndexVisibleChanged));

        public static readonly DependencyProperty IndexTitleProperty = DependencyProperty.Register("IndexTitle", typeof(string), typeof(TabScrollContainer), new PropertyMetadata(OnIndexTitleChanged));

        /// <summary>
        /// Konstruktor
        /// </summary>
        public TabScrollContainer()
        {
            InitializeComponent();

            IndexVisible = true;

            Loaded += (sender, args) =>
            {
                // jen pro lepší zobrazení v designe režimu
                var isDesign = DesignerProperties.GetIsInDesignMode(this);
                if (isDesign)
                {
                    var sections = new List<object>()
                    {
                        "Sekce 1",
                        "Sekce 2",
                        "Sekce 3",
                        "Sekce 4",
                    };
                    Sekce = sections;
                }
            };
        }

        /// <summary>
        /// Inicializace komponenty - volána primárně při nastavení/přiřazení sekcí
        /// </summary>
        private void Init()
        {            
            TabMain.Items.Clear();
            StackMain.Children.Clear();
            PanelIndex.Children.Clear();
            m_viewSections.Clear();

            if (Sekce == null) return;
            
            m_viewSections = new List<SekceWrapper>();

            var index = 0;
            // vytvoříme tlačítka v osnově
            foreach (var s in Sekce)
            {
                var btn = new Button {Content = s.ToString()};
                btn.Tag = index++;
                btn.Click += BtnSekceClick;
                PanelIndex.Children.Add(btn);
                // ...a připravíme si sekce s jejich UIElementy pro zobrazení
                var wrp = new SekceWrapper
                {
                    SekceViewModel = s,
                    SekceView = CreateUserControl(s)
                };
                m_viewSections.Add(wrp);
            }

            Redraw();
        }

        /// <summary>
        /// Kliknutí na tlačítko sekce v osnově - focusne danou sekci
        /// </summary>
        /// <param name="o"></param>
        /// <param name="routedEventArgs"></param>
        private void BtnSekceClick(object o, RoutedEventArgs routedEventArgs)
        {
            var btn = (Button) o;
            var index = (int) btn.Tag;
            if (SekceAsTab)
            {
                TabMain.SelectedIndex = index;
            }
            else
            {
                ((StackPanel)StackMain.Children[index]).BringIntoView();
            }
        }

        /// <summary>
        /// Překreslení sekcí dle aktuálně zvoleného zobrazení - taby nebo scroll
        /// </summary>
        private void Redraw()
        {
            // nejprve je třeba smazat použité zobrazení, hlavně z důvodu odpojení obsahu sekcí
            if (Sekce == null) return;
            
            if (SekceAsTab)
            {
                TabMain.Visibility = Visibility.Visible;
                ScrollMain.Visibility = Visibility.Collapsed;
                foreach (var s in Sekce)
                {
                    // taby - vygenerujeme jednotlivé záložky
                    SetTabSekce(s);
                }
            }
            else
            {
                // scroll - vygenerujeme jednotlive sekce
                TabMain.Visibility = Visibility.Collapsed;
                ScrollMain.Visibility = Visibility.Visible;
                foreach (var s in Sekce)
                {
                    SetScrollSekce(s);
                }
            }
        }

        private void SetTabSekce(object s)
        {
            var wrp = m_viewSections.Single(p => p.SekceViewModel == s);
            if (wrp.Tab == null)
            {
                wrp.Tab = new TabContainer();
                TabMain.Items.Add(wrp.Tab);
            }
            if (wrp.Scroll != null) wrp.Scroll.Sekce = null;
            wrp.Tab.Sekce = wrp.SekceView;
        }

        private void SetScrollSekce(object s)
        {
            var wrp = m_viewSections.Single(p => p.SekceViewModel == s);
            if (wrp.Scroll == null)
            {
                wrp.Scroll = new ScrollContainer();
                StackMain.Children.Add((UIElement) wrp.Scroll);
            }
            if (wrp.Tab != null) wrp.Tab.Sekce = null;
            wrp.Scroll.Sekce = wrp.SekceView;
        }

        /// <summary>
        /// Vytvoří user control pro danou sekci
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static UserControl CreateUserControl(object s)
        {
            UserControl uc;
            var type = s.GetType();
            // zjistíme jestli je nastaven atribut explicitně určující UIElement, který se má použít pro zobrazení
            var attr = type.GetCustomAttributes(typeof(ViewTypeAttribute), false).FirstOrDefault();
            if (attr != null)
            {
                var attrType = (ViewTypeAttribute) attr;
                uc = (UserControl) attrType.GetInstance();
                uc.DataContext = s;
            }
            else
            {
                // ...pokud nebyl UIElement nastaven, tak implicitně toto
                uc = new UserControl();
                uc.DataContext = s;
            }

            return uc;
        }

        /// <summary>
        /// Příznak zda má být vditelná osnova
        /// </summary>
        public bool IndexVisible
        {
            get => (bool)GetValue(IndexVisibleProperty);
            set => SetValue(IndexVisibleProperty, value);
        }

        /// <summary>
        /// Popisek osnovy
        /// </summary>
        public string IndexTitle
        {
            get => (string)GetValue(IndexTitleProperty);
            set => SetValue(IndexTitleProperty, value);
        }

        /// <summary>
        /// Příznak jestli se mají zobrazit sekce v tabech (TRUE) anebo jako scroll (FALSE)
        /// </summary>
        public bool SekceAsTab
        {
            get => (bool)GetValue(SekceAsTabProperty);
            set => SetValue(SekceAsTabProperty, value);
        }

        /// <summary>
        /// Kolekce sekcí, které se mají v komponentě zobrazovat
        /// </summary>
        public List<object> Sekce
        {
            get => (List<object>)GetValue(SekceProperty);
            set => SetValue(SekceProperty, value);
        }

        /// <summary>
        /// Událost při změně titulku osnovy
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnIndexTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            var text = (string)e.NewValue;
            ctrl.TextIndexTitle.Text = text;
        }

        /// <summary>
        /// Událost při změně viditelnosti osnovy
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnIndexVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            var state = (bool) e.NewValue;
            ctrl.PanelLeft.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Událost při změně typu zobrazení - tab/scroll
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSekceAsTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            ctrl.Redraw();
        }

        /// <summary>
        /// Událost při změně kolekce se sekcemi - POZOR: nereaguje na změny uvnitř kolekce, jako např. přidání nebo odebrání
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSekceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            ctrl.Init();
        }

    }
}
