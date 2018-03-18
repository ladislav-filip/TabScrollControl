using System;
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
        /// připravený seznam sekcí, pro zobrazení v komponentě
        /// </summary>
        private List<Tuple<object, UserControl>> m_viewSections = new List<Tuple<object, UserControl>>();

        public static readonly DependencyProperty SekceAsTabProperty = DependencyProperty.Register("SekceAsTab", typeof(bool), typeof(TabScrollContainer), new PropertyMetadata(OnSekceAsTabChanged));

        public static readonly DependencyProperty SekceProperty = DependencyProperty.Register("Sekce", typeof(List<object>), typeof(TabScrollContainer), new PropertyMetadata(OnSekceChanged));

        public static readonly DependencyProperty IndexVisibleProperty = DependencyProperty.Register("IndexVisible", typeof(bool), typeof(TabScrollContainer), new PropertyMetadata(OnIndexVisibleChanged));

        public static readonly DependencyProperty IndextTitleProperty = DependencyProperty.Register("IndextTitle", typeof(string), typeof(TabScrollContainer), new PropertyMetadata(OnIndextTitleChanged));

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
            
            m_viewSections = new List<Tuple<object, UserControl>>();

            var index = 0;
            // vytvoříme tlačítka v osnově
            foreach (var s in Sekce)
            {
                var btn = new Button {Content = s.ToString()};
                btn.Tag = index++;
                btn.Click += BtnSekceClick;
                PanelIndex.Children.Add(btn);
                // ...a připravíme si sekce s jejich UIElementy pro zobrazení
                m_viewSections.Add(new Tuple<object, UserControl>(s, CreateUserControl(s)));
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
            TabMain.Items.Clear();
            StackMain.Children.Clear();
            if (Sekce == null) return;

            // taby - vygenerujeme jednotlivé záložky
            if (SekceAsTab)
            {
                TabMain.Visibility = Visibility.Visible;
                ScrollMain.Visibility = Visibility.Collapsed;
                foreach (var s in Sekce)
                {
                    var tb = new TabItem { Header = s.ToString() };
                    tb.Content = m_viewSections.Single(p => p.Item1 == s).Item2;
                    TabMain.Items.Add(tb);
                }
            }
            else
            {
                // scroll - vygenerujeme jednotlive sekce
                TabMain.Visibility = Visibility.Collapsed;
                ScrollMain.Visibility = Visibility.Visible;
                foreach (var s in Sekce)
                {
                    // TODO: zde bude vhodná optimalizace, kdy se celá ta infrastruktura kolem jedné sekce vytvoří pouze 1x
                    var sp = new StackPanel();
                    sp.Orientation = Orientation.Vertical;
                    var tb = new TextBlock();
                    tb.Text = s.ToString();
                    sp.Children.Add(tb);
                    var br = new Border();
                    var exp = new Expander();
                    exp.IsExpanded = true;
                    exp.Header = s.ToString();
                    br.Child = exp;
                    exp.Content = m_viewSections.Single(p => p.Item1 == s).Item2;                    
                    sp.Children.Add(br);
                    StackMain.Children.Add(sp);
                }
            }
        }

        /// <summary>
        /// Vytvoří user control pro danou sekci
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static UserControl CreateUserControl(object s)
        {
            UserControl uc = null;
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
        public string IndextTitle
        {
            get => (string)GetValue(IndextTitleProperty);
            set => SetValue(IndextTitleProperty, value);
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
        private static void OnIndextTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            var text = (string)e.NewValue;
            ctrl.IndexTitle.Text = text;
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
