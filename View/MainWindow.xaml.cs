using CryptoWPFX.Class;
using CryptoWPFX.Model;
using CryptoWPFX.Model.API;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core.Extensions;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static CryptoWPFX.Model.API.CoinGeckoApi;

namespace CryptoWPFX
{

    public partial class MainWindow : Window
    {
        SQLiteDB SQL = new SQLiteDB();

        string TokenActiveID = "";
        private CoinGeckoApi coinGeckoAPI = new CoinGeckoApi();
        private List<CryptoCurrency> topCurrencies = new List<CryptoCurrency>();
        List<Coin> ListFavoritesCrypto = new List<Coin>();

        JsonElement ConvertCurrencyToken;

        
        public MainWindow()
        {
            InitializeComponent();
            LoadBorder.Visibility = Visibility.Visible;

            
        }
        
        static string InsertSeparator(string input)
        {
            if (input.Length <= 3)
                return input;

            if (input.ToLower().Contains('e'))
            {
                string[] numConvert = input.ToLower().Split('e');
                string[] span = input.Split('-');
                string resultSpan = (numConvert[0].ToDouble() / Math.Pow(10, span[span.Length - 1].ToDouble())).ToString("G");
                //decimal number = decimal.Parse(resultSpan.Replace(".", ","));
                string result = resultSpan;
                return result;
            }
            else
            {
                decimal number = decimal.Parse(input.Replace(".", ","));
                string result = number.ToString("N");
                return result;
            }
        }

        // метод для аббривеатуры цифр (1M, 1B, 1T)
        static string AbbreviateNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            long number;
            if (!long.TryParse(input, out number))
                throw new ArgumentException("Invalid input: not a valid number");

            double num = (double)number;

            if (number >= 1000000000000)
                return (num / 1000000000000).ToString("0.###") + " T";
            if (number >= 1000000000)
                return (num / 1000000000).ToString("0.###") + " B";
            if (number >= 1000000)
                return (num / 1000000).ToString("0.###") + " M";
            if (number >= 1000)
                return (num / 1000).ToString("0.###") + " K";

            return number.ToString();
        }

        // обработчик кнопки увеличения и уменьшения окна
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            FullScreenState();
        }

        // обработчик закрытия окна
        private void ScreenClose_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        // обработчик для перетаскивания окна мышью
        private void ScreenStateAndDragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                FullScreenState();
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        // загрузка списка криптовалют на главной странице
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int rotateIndex = CoinGeckoApi.GetFearAndGreedIndex();

                FearGreedIndexRotate.Angle += (rotateIndex * 1.8);
                FearGreedText.Content = rotateIndex;

                // Получаем список топ N криптовалют
                topCurrencies = await coinGeckoAPI.GetTopNCurrenciesAsync(500, 1);
                foreach (CryptoCurrency currencies in topCurrencies)
                {
                    if (currencies.price_change_percentage_24h <= 0)
                    {
                        currencies.ColorPercettage = Brushes.Red;
                    }
                    else
                    {
                        currencies.ColorPercettage = Brushes.LimeGreen;
                    }
                }
                // Привязываем список к DataGrid
                DataGridMain.ItemsSource = topCurrencies;

                // Привязываем к комбо-боксам
                //cmbFromCurrency.ItemsSource = topCurrencies;
                //cmbFromCurrency.DisplayMemberPath = "Symbol";
                //cmbToCurrency.ItemsSource = topCurrencies;
                //cmbToCurrency.DisplayMemberPath = "Symbol";

                RefreshFavorites();

                LoadBorder.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Обработка возможных ошибок, например, вывод в консоль
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private List<CryptoCurrency> filteredCryptos = new List<CryptoCurrency>();

        private void UpdateCryptoListCmbToCurrency()
        {
            string searchQuery = cmbToCurrency.Text.Trim().ToLower();
            filteredCryptos.Clear();

            if (string.IsNullOrEmpty(searchQuery))
            {
                foreach (var crypto in topCurrencies)
                {
                    filteredCryptos.Add(crypto);
                }
            }
            else
            {
                // Иначе фильтруем по запросу
                foreach (var crypto in topCurrencies)
                {
                    if (crypto.Name.ToLower().Contains(searchQuery) || crypto.Symbol.ToLower().Contains(searchQuery))
                    {
                        filteredCryptos.Add(crypto);
                    }
                }
            }
            // Обновляем список ListBox
            lstCrypto.ItemsSource = null;
            lstCrypto.ItemsSource = filteredCryptos;
        }

        private void UpdateCryptoListCmbFromCurrencyy()
        {
            string searchQuery = cmbFromCurrency.Text.Trim().ToLower();
            filteredCryptos.Clear();

            if (string.IsNullOrEmpty(searchQuery))
            {
                foreach (var crypto in topCurrencies)
                {
                    filteredCryptos.Add(crypto);
                }
            }
            else
            {
                // Иначе фильтруем по запросу
                foreach (var crypto in topCurrencies)
                {
                    if (crypto.Name.ToLower().Contains(searchQuery) || crypto.Symbol.ToLower().Contains(searchQuery))
                    {
                        filteredCryptos.Add(crypto);
                    }
                }
            }
            // Обновляем список ListBox
            lstCryptoFrom.ItemsSource = null;
            lstCryptoFrom.ItemsSource = filteredCryptos;
        }

        private void txtSearch_TextChangedFrom(object sender, TextChangedEventArgs e)
        {
            UpdateCryptoListCmbFromCurrencyy();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCryptoListCmbToCurrency();
        }

        private void lstCryptoToCurrency(object sender, SelectionChangedEventArgs e)
        {
            // Обработка выбора криптовалюты
            if (lstCrypto.SelectedItem != null)
            {
                var selectedCrypto = (CryptoCurrency)lstCrypto.SelectedItem;
                if (selectedCrypto != null)
                {
       
                    lstCrypto.Visibility = Visibility.Hidden;
                    cmbToCurrency.Text = selectedCrypto.Symbol;
                }
            }
            else
            {
                lstCrypto.Visibility = Visibility.Visible;
            }
        }

        private void lstCryptoFromCurrency(object sender, SelectionChangedEventArgs e)
        {
            // Обработка выбора криптовалюты
            if (lstCryptoFrom.SelectedItem != null)
            {
                var selectedCrypto = (CryptoCurrency)lstCryptoFrom.SelectedItem;
                if (selectedCrypto!=null)
                {


                    lstCryptoFrom.Visibility=Visibility.Hidden;
                    cmbFromCurrency.Text = selectedCrypto.Symbol;
                }
            }
            else
            {
                lstCryptoFrom.Visibility = Visibility.Visible;
            }
        }



        // Функция для полного экрана
        public void FullScreenState()
        {
            if (WindowState == WindowState.Maximized)
            {
                BorderScreen.CornerRadius = new CornerRadius(30);
                WindowState = WindowState.Normal;
            }
            else
            {
                BorderScreen.CornerRadius = new CornerRadius(0);
                WindowState = WindowState.Maximized;
            }
        }

        // обработчик анимации при наведении на кнопку открытия токена (криптовалюты в таблице)
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border borderNextDataGrid)
            {
                borderNextDataGrid.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7163ba"));
                borderNextDataGrid.VerticalAlignment = VerticalAlignment.Center;
                borderNextDataGrid.HorizontalAlignment = HorizontalAlignment.Center;
                borderNextDataGrid.Width = 70;
                borderNextDataGrid.Height = 45;
                if (borderNextDataGrid.Child is System.Windows.Controls.Label labelBorderNext)
                {
                    labelBorderNext.Content = "Открыть";
                    labelBorderNext.FontSize = 15;
                    labelBorderNext.VerticalAlignment = VerticalAlignment.Center;
                    labelBorderNext.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
        }

        // обработчик анимации при отведении с кнопки открытия токена (криптовалюты в таблице)
        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border borderNextDataGrid)
            {
                borderNextDataGrid.BorderBrush = Brushes.Transparent;
                if (borderNextDataGrid.Child is System.Windows.Controls.Label labelBorderNext)
                {
                    labelBorderNext.Content = "▶";
                    labelBorderNext.FontSize = 25;
                }
            }
        }


        // прокрутка таблица на главной
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scroll = sender as ScrollViewer;
            if (e.Delta > 0)
            {
                scroll.LineUp();
            }
            else
            {
                scroll.LineDown();
            }
            e.Handled = true;
        }

        private void ScrennHide_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        private async void BorderRefreshInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем список топ N криптовалют
            topCurrencies = await coinGeckoAPI.GetTopNCurrenciesAsync(500, 1);
            foreach (CryptoCurrency currencies in topCurrencies)
            {
                if (currencies.price_change_percentage_24h <= 0)
                {
                    currencies.ColorPercettage = Brushes.Red;
                }
                else
                {
                    currencies.ColorPercettage = Brushes.LimeGreen;
                }
            }
            DataGridMain.ItemsSource = null;
            // Привязываем список к DataGrid
            DataGridMain.ItemsSource = topCurrencies;

            RefreshFavorites();
        }
        private void borderClickDataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainScrolCrypto.Visibility = Visibility.Visible;
            ScrolFavorites.Visibility = Visibility.Collapsed;
            ConverterCoin.Visibility = Visibility.Collapsed;
            borderCoinInput.Visibility = Visibility.Visible;
            borderHeaderDataGrid.Visibility = Visibility.Visible;
            borderClickDataGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4E00AC"));
            borderClickDataGridMainPoolCrypto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7163ba"));
            borderConverterCoin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7163ba"));

            if (borderClickDataGrid.Child is FontAwesome.WPF.ImageAwesome font && borderClickDataGridMainPoolCrypto.Child is FontAwesome.WPF.ImageAwesome font2 && borderConverterCoin.Child is FontAwesome.WPF.ImageAwesome font3)
            {
                font.Foreground = Brushes.White;
                font2.Foreground = Brushes.Black;
                font3.Foreground = Brushes.Black;
            }

        }

        private async void borderClickDataGridMainPoolCrypto_MouseDown(object sender, MouseButtonEventArgs e)
        {
                MainScrolCrypto.Visibility = Visibility.Collapsed;
                ScrolFavorites.Visibility = Visibility.Visible;
                ConverterCoin.Visibility = Visibility.Collapsed;
                borderCoinInput.Visibility = Visibility.Visible;
                borderHeaderDataGrid.Visibility = Visibility.Visible;
                borderClickDataGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7163ba"));
                borderClickDataGridMainPoolCrypto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4E00AC"));
                borderConverterCoin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7163ba"));

                if (borderClickDataGrid.Child is FontAwesome.WPF.ImageAwesome font && borderClickDataGridMainPoolCrypto.Child is FontAwesome.WPF.ImageAwesome font2 && borderConverterCoin.Child is FontAwesome.WPF.ImageAwesome font3)
                {
                    font.Foreground = Brushes.Black;
                    font2.Foreground = Brushes.White;
                    font3.Foreground = Brushes.Black;
                }

            RefreshFavorites();
        }

        private void borderConverterCoin_MouseDown(object sender, MouseButtonEventArgs e)
        {
                ScrolFavorites.Visibility = Visibility.Collapsed;
                MainScrolCrypto.Visibility = Visibility.Collapsed;
                ConverterCoin.Visibility = Visibility.Visible;
                borderCoinInput.Visibility = Visibility.Collapsed;
                borderHeaderDataGrid.Visibility = Visibility.Collapsed;
                borderClickDataGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7163ba"));
                borderClickDataGridMainPoolCrypto.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7163ba"));
                borderConverterCoin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4E00AC"));

                if (borderClickDataGrid.Child is FontAwesome.WPF.ImageAwesome font && borderClickDataGridMainPoolCrypto.Child is FontAwesome.WPF.ImageAwesome font2 && borderConverterCoin.Child is FontAwesome.WPF.ImageAwesome font3)
                {
                    font.Foreground = Brushes.Black;
                    font2.Foreground = Brushes.Black;
                    font3.Foreground = Brushes.White;
                }
        }

        // поиск криптовалют
        private void coinInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = coinInput.Text.ToLower();
            List<CryptoCurrency> filteredList = topCurrencies
        .Where(coin =>
            coin.Symbol.ToLower().Contains(searchText) ||
            coin.Name.ToLower().Contains(searchText) ||
            coin.Name.ToLower().StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
        .ToList();

            DataGridMain.ItemsSource = filteredList;

        }

        // обработчик нажатия на кнопку для открытия токена в таблице
        private void Click_OpenToken(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            OpenToken(border.GetValue(AutomationProperties.AutomationIdProperty).ToString());

        }

        // метод открытия информации о токене
        private async void OpenToken(string TokenID)
        {
            TokenActiveID = TokenID;
            MainView.Visibility = Visibility.Collapsed;
            TokenView.Visibility = Visibility.Visible;
            var InfoToken = await CoinGeckoApi.GetInfoTokenToID(TokenID, "usd");



            foreach (UIElement lbl in TimeSetChartPanel.Children)
            {
                if (lbl is Label)
                {
                    Label label = lbl as Label;
                    if (label.Content.ToString() == "1 день")
                    {
                        label.Foreground = Brushes.White;
                    }
                    else
                    {
                        label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA8A8A8"));
                    }
                }
            }
            if (SQL.GetListFavorites().Contains(TokenID))
            {
                FavoriteStar.Icon = FontAwesome.WPF.FontAwesomeIcon.Star;
            }
            else
            {
                FavoriteStar.Icon = FontAwesome.WPF.FontAwesomeIcon.StarOutline;
            }
            //try
            //{ 

            NameToken.Content = InfoToken[CoinGeckoApi.CoinField.Name.ToString().ToLower()].ToString().ToUpper();
            SymbolToken.Content = InfoToken[CoinGeckoApi.CoinField.Symbol.ToString().ToLower()].ToString().ToUpper();
            PrecentToken.Content = $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_24h.ToString().ToLower()].ToString().Replace(".", ",")), 2)}%";
            if (PrecentToken.Content.ToString()[0] == '-')
            {
                PrecentToken.Foreground = Brushes.Red;
            }

            mountainRenderSeries.Stroke = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.ColorChartLine);
            ColorPickerLine.SelectedColor = (Color)ColorConverter.ConvertFromString(Properties.Settings.Default.ColorChartLine);

            //построение графика
            var series = new XyDataSeries<DateTime, double>();
            series = await CoinGeckoApi.GetActualChartToken(TokenID, "usd", "1");
            mountainRenderSeries.DataSeries = series;
            ChartToken.AnimateZoomExtentsCommand.Execute(null);

            // Создаем новый объект BitmapImage
            BitmapImage bitmap = new BitmapImage();

            // Устанавливаем URI изображения
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(InfoToken[CoinGeckoApi.CoinField.Image.ToString().ToLower()].ToString());
            bitmap.EndInit();

            LogoToken.Source = bitmap;
            MaxPriceToken.Content = InsertSeparator(InfoToken[CoinGeckoApi.CoinField.High_24h.ToString().ToLower()].ToString());
            MinPriceToken.Content = InsertSeparator(InfoToken[CoinGeckoApi.CoinField.Low_24h.ToString().ToLower()].ToString());
            ATHToken.Content = "ATH: " + InsertSeparator(InfoToken[CoinGeckoApi.CoinField.Ath.ToString().ToLower()].ToString());
            VolumeToken.Content = AbbreviateNumber(InfoToken[CoinGeckoApi.CoinField.Market_Cap.ToString().ToLower()].ToString());

            Percent_1h.Content = InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_1h_In_Currency.ToString().ToLower()] == null ? "X" : $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_1h_In_Currency.ToString().ToLower()].ToString().Replace(".", ",")), 2)}%";
            Percent_24h.Content = InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_24h_In_Currency.ToString().ToLower()] == null ? "X" : $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_24h_In_Currency.ToString().ToLower()].ToString().Replace(".", ",")), 2)}%";
            Percent_7d.Content = InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_7d_In_Currency.ToString().ToLower()] == null ? "X" : $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_7d_In_Currency.ToString().ToLower()].ToString().Replace(".", ",")), 2)}%";
            Percent_14d.Content = InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_14d_In_Currency.ToString().ToLower()] == null ? "X" : $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_14d_In_Currency.ToString().ToLower()].ToString().Replace(".", ",")), 2)}%";
            Percent_30d.Content = InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_30d_In_Currency.ToString().ToLower()] == null ? "X" : $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_30d_In_Currency.ToString().ToLower()].ToString().Replace(".", ",")), 2)}%";
            Percent_1year.Content = InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_1y_In_Currency.ToString().ToLower()] == null ? "X" : $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Price_Change_Percentage_1y_In_Currency.ToString().ToLower()].ToString().Replace(".", ",")), 2)}%";

            foreach (var label in GridPercentAll.Children)
            {
                if (label is System.Windows.Controls.Label lbl)
                {
                    if (lbl.Content.ToString()[0] == '-')
                    {
                        lbl.Foreground = Brushes.Red;
                    }
                }
            }
            string[] prices = InfoToken[CoinGeckoApi.CoinField.Current_Price.ToString().ToLower()].ToString().Split('.');

            if (prices[0].Length <= 1 || prices[0] == "0" || prices[0].ToLower().Contains("e"))
            {
                PriceToken.Content = InsertSeparator(InfoToken[CoinGeckoApi.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","));
            }
            else if (prices[0].Length < 4)
            {
                PriceToken.Content = $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ",")), 6)}";
            }
            else if (prices[0].Length < 2)
            {
                PriceToken.Content = $"{Math.Round(Convert.ToDouble(InfoToken[CoinGeckoApi.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ",")), 8)}";
            }
            else
            {
                PriceToken.Content = InsertSeparator(InfoToken[CoinGeckoApi.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","));
            }

            void TopBurseSecurity(ref System.Windows.Controls.Label lab, string name)
            {
                lab.FontWeight = FontWeights.Bold;
                if (name.ToLower().Contains("bybit"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7a600"));
                else if (name.ToLower().Contains("binance"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f6cd2f"));
                else if (name.ToLower().Contains("okx"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff700"));
                else if (name.ToLower().Contains("mexc"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1972e2"));
                else if (name.ToLower().Contains("bingx"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#435af6"));
                else if (name.ToLower().Contains("kucoin"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#23af91"));
                else if (name.ToLower().Contains("gate.io"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#17e6a1"));
                else if (name.ToLower().Contains("htx"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008fdd"));
                else if (name.ToLower().Contains("bitget"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1ea1b4"));
                else if (name.ToLower().Contains("coinbase"))
                    lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0052ff"));

                else
                {
                    lab.Foreground = Brushes.White;
                    lab.FontWeight = FontWeights.Normal;
                }

            }

            PanelBurse.Children.Clear();
            List<TickerData> tickerDatas = await GetActualBurse(TokenID);
            foreach (TickerData tickerData in tickerDatas)
            {
                if (tickerData.TradeURL == null)
                {
                    continue;
                }
                // Создание элементов
                Grid grid = new Grid();

                // Определение колонок
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                // Создание и добавление Label 1
                System.Windows.Controls.Label label1 = new System.Windows.Controls.Label();
                label1.Content = tickerData.Name;
                TopBurseSecurity(ref label1, tickerData.Name);
                label1.Margin = new Thickness(5);
                grid.Children.Add(label1);

                // Создание и добавление Label 2
                System.Windows.Controls.Label label2 = new System.Windows.Controls.Label();
                label2.Content = $"{tickerData.Base}/{tickerData.Target}";
                label2.Margin = new Thickness(5);
                label2.Foreground = Brushes.White;
                label2.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetColumn(label2, 1);
                grid.Children.Add(label2);

                // Создание и добавление Label 3
                System.Windows.Controls.Label label3 = new System.Windows.Controls.Label();
                label3.Content = tickerData.LastPrice;
                label3.Margin = new Thickness(5);
                label3.Foreground = Brushes.White;
                label3.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetColumn(label3, 2);
                grid.Children.Add(label3);

                // Создание и добавление FontAwesome
                var icon = new FontAwesome.WPF.FontAwesome();
                icon.Icon = FontAwesome.WPF.FontAwesomeIcon.ArrowRight;
                icon.FontSize = 15;
                icon.VerticalAlignment = VerticalAlignment.Center;
                icon.Foreground = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                icon.SetValue(AutomationProperties.AutomationIdProperty, tickerData.TradeURL);
                icon.MouseDown += Icon_MouseDownGetBurse;
                icon.MouseEnter += Icon_MouseEnter;
                Grid.SetColumn(icon, 3);
                grid.Children.Add(icon);

                PanelBurse.Children.Add(grid);
            }
            //}
            //catch
            //{
            //    MessageBorder.Visibility = Visibility.Visible;
            //    MessageText.Text = "Слишком много запросов, попробуйте позже...";
            //    TokenView.Visibility = Visibility.Collapsed;
            //    MainView.Visibility = Visibility.Visible;
            //}

        }

        // анимация при наведении на кнопку для перехода на биржу
        private void Icon_MouseEnter(object sender, MouseEventArgs e)
        {
            var icon = sender as FontAwesome.WPF.FontAwesome;
            // Создаем анимацию смещения
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0; // начальное положение
            animation.To = 10; // конечное положение
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(400)); // продолжительность анимации
            animation.AutoReverse = true; // автоматически вернуться обратно

            // Создаем объект TranslateTransform для анимации смещения
            TranslateTransform translateTransform = new TranslateTransform();

            // Применяем анимацию к свойству X TranslateTransform
            translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);

            // Применяем TranslateTransform к RenderTransform элемента
            icon.RenderTransform = translateTransform;
        }

        // кнопка для перехода на биржу
        private void Icon_MouseDownGetBurse(object sender, MouseButtonEventArgs e)
        {
            var icon = sender as FontAwesome.WPF.FontAwesome;
            // Открываем ссылку в браузере
            //Process.Start(icon.GetValue(AutomationProperties.AutomationIdProperty).ToString());
            // Открываем ссылку в браузере по умолчанию
            Process.Start(new ProcessStartInfo
            {
                FileName = icon.GetValue(AutomationProperties.AutomationIdProperty).ToString(),
                UseShellExecute = true
            });
        }

        // кнопка для просмотра курса токена в других валютах
        private async void Click_ConvertTokenPrice(object sender, MouseButtonEventArgs e)
        {
            if (BorderConvertTokenPrice.Visibility == Visibility.Hidden)
            {
                BorderConvertTokenPrice.Visibility = Visibility.Visible;
                ConvertCurrencyToken = await CoinGeckoApi.GetInfoTokenToIDFull(TokenActiveID);
                ConvertTokenPrice.Children.Clear();
                foreach (JsonProperty property in ConvertCurrencyToken.EnumerateObject())
                {
                    ConvertTokenPrice.Children.Add(new System.Windows.Controls.Label
                    {
                        Content = $"{InsertSeparator(property.Value.GetDouble().ToString())} {property.Name.ToUpper()}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.White
                    });
                }
            }
            else
            {
                BorderConvertTokenPrice.Visibility = Visibility.Hidden;
            }

        }

        // обработчик изменения времени графика
        private async void Click_TimeSetChart(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Label label = sender as System.Windows.Controls.Label;

            foreach (UIElement lbl in TimeSetChartPanel.Children)
            {
                if (lbl is Label)
                {
                    Label lab = lbl as Label;
                    if (label.Content.ToString() == lab.Content.ToString())
                    {
                        lab.Foreground = Brushes.White;
                    }
                    else
                    {
                        lab.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA8A8A8"));
                    }
                }
            }
            string[] strings = label.Content.ToString().Split(' ');
            var series = new XyDataSeries<DateTime, double>();
            series = await CoinGeckoApi.GetActualChartToken(TokenActiveID, "usd", strings[0]);
            if (mountainRenderSeries != null && mountainRenderSeries.DataSeries != null)
            {
                mountainRenderSeries.DataSeries.Clear();
            }
            mountainRenderSeries.DataSeries = series;
            ChartToken.AnimateZoomExtentsCommand.Execute(null);
        }

        // кнопка для возврата на главную страницу со страницы токена
        private void ClickBackTheMainView(object sender, MouseButtonEventArgs e)
        {
            TokenView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
        }

        // обработчик кнопки закрытия окна об ошибке
        private void MessageClose(object sender, MouseButtonEventArgs e)
        {
            MessageBorder.Visibility = Visibility.Collapsed;
        }

        // поиск курса токена в других валютах (на странице с токеном)
        private void SearchCryptoConvert(object sender, TextChangedEventArgs e)
        {
            TextBox text = sender as TextBox;
            ConvertTokenPrice.Children.Clear();
            foreach (JsonProperty property in ConvertCurrencyToken.EnumerateObject())
            {
                if (property.Name.ToUpper().Contains(text.Text.ToUpper()))
                {
                    ConvertTokenPrice.Children.Add(new System.Windows.Controls.Label
                    {
                        Content = $"{InsertSeparator(property.Value.GetDouble().ToString())} {property.Name.ToUpper()}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.White
                    });
                }
            }
            if (ConvertTokenPrice.Children.Count <= 0)
            {
                ConvertTokenPrice.Children.Add(new System.Windows.Controls.Label
                {
                    Content = "Не найдено",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.White
                });
            }
        }

        // сортировка главной таблицы с монетами
        private void ClickSortMainDataGrid(object sender, MouseButtonEventArgs e)
        {
            var SortText = sender as TextBlock;
            var top = new List<CryptoCurrency>();
            var ListFavorites = ListFavoritesCrypto;

            if (SortText.Text.Split(" ")[0] == "Цена")
            {
                if (SortText.Text[SortText.Text.Length - 1] == '●')
                {
                    top = topCurrencies.OrderByDescending(c => c.Price).ToList();
                    ListFavorites = ListFavoritesCrypto.OrderByDescending(c => c.current_price).ToList();
                    SortText.Text = "Цена ▽";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '▽')
                {
                    top = topCurrencies.OrderBy(c => c.Price).ToList();
                    ListFavorites = ListFavoritesCrypto.OrderBy(c => c.current_price).ToList();
                    SortText.Text = "Цена △";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '△')
                {
                    top = topCurrencies;
                    ListFavorites = ListFavoritesCrypto;
                    SortText.Text = "Цена ●";
                }
                foreach (TextBlock item in GridNameDataGridColumn.Children)
                {
                    if (item.Text.Split()[0] != "Цена" && item.Text.Split()[0] != "Монета" && item.Text.Split()[0] != "Переход")
                    {
                        item.Text = item.Text.Replace(item.Text.ToCharArray()[item.Text.Length - 1], '●');
                    }
                }
            }
            else if (SortText.Text.Split(" ")[0] == "Название")
            {
                if (SortText.Text[SortText.Text.Length - 1] == '●')
                {
                    top = topCurrencies.OrderByDescending(c => c.Name).ToList();
                    ListFavorites = ListFavorites.OrderByDescending(c => c.Name).ToList();
                    SortText.Text = "Название ▽";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '▽')
                {
                    top = topCurrencies.OrderBy(c => c.Name).ToList();
                    ListFavorites = ListFavorites.OrderBy(c => c.Name).ToList();
                    SortText.Text = "Название △";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '△')
                {
                    top = topCurrencies;
                    ListFavorites = ListFavoritesCrypto;
                    SortText.Text = "Название ●";
                }
                foreach (TextBlock item in GridNameDataGridColumn.Children)
                {
                    if (item.Text.Split()[0] != "Название" && item.Text.Split()[0] != "Монета" && item.Text.Split()[0] != "Переход")
                    {
                        item.Text = item.Text.Replace(item.Text.ToCharArray()[item.Text.Length - 1], '●');
                    }
                }
            }
            else if (SortText.Text.Split(" ")[0] == "Символы")
            {
                if (SortText.Text[SortText.Text.Length - 1] == '●')
                {
                    top = topCurrencies.OrderByDescending(c => c.Symbol).ToList();
                    ListFavorites = ListFavorites.OrderByDescending(c => c.Symbol).ToList();
                    SortText.Text = "Символы ▽";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '▽')
                {
                    top = topCurrencies.OrderBy(c => c.Symbol).ToList();
                    ListFavorites = ListFavorites.OrderBy(c => c.Symbol).ToList();
                    SortText.Text = "Символы △";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '△')
                {
                    top = topCurrencies;
                    ListFavorites = ListFavoritesCrypto;
                    SortText.Text = "Символы ●";
                }
                foreach (TextBlock item in GridNameDataGridColumn.Children)
                {
                    if (item.Text.Split()[0] != "Символы" && item.Text.Split()[0] != "Монета" && item.Text.Split()[0] != "Переход")
                    {
                        item.Text = item.Text.Replace(item.Text.ToCharArray()[item.Text.Length - 1], '●');
                    }
                }
            }
            else if (SortText.Text.Split(" ")[0] == "Объем")
            {
                if (SortText.Text[SortText.Text.Length - 1] == '●')
                {
                    top = topCurrencies.OrderByDescending(c => c.Volume).ToList();
                    ListFavorites = ListFavorites.OrderByDescending(c => c.Market_Cap_Change_24h).ToList();
                    SortText.Text = "Объем торгов 24ч ▽";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '▽')
                {
                    top = topCurrencies.OrderBy(c => c.Volume).ToList();
                    ListFavorites = ListFavorites.OrderBy(c => c.Market_Cap_Change_24h).ToList();
                    SortText.Text = "Объем торгов 24ч △";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '△')
                {
                    top = topCurrencies;
                    ListFavorites = ListFavoritesCrypto;
                    SortText.Text = "Объем торгов 24ч ●";
                }
                foreach (TextBlock item in GridNameDataGridColumn.Children)
                {
                    if (item.Text.Split()[0] != "Объем" && item.Text.Split()[0] != "Монета" && item.Text.Split()[0] != "Переход")
                    {
                        item.Text = item.Text.Replace(item.Text.ToCharArray()[item.Text.Length - 1], '●');
                    }
                }
            }
            else if (SortText.Text.Split(" ")[0] == "Проценты")
            {
                if (SortText.Text[SortText.Text.Length - 1] == '●')
                {
                    top = topCurrencies.OrderByDescending(c => c.price_change_percentage_24h).ToList();
                    ListFavorites = ListFavorites.OrderByDescending(c => c.Price_Change_Percentage_24h_In_Currency).ToList();
                    SortText.Text = "Проценты 24ч ▽";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '▽')
                {
                    top = topCurrencies.OrderBy(c => c.price_change_percentage_24h).ToList();
                    ListFavorites = ListFavorites.OrderBy(c => c.Price_Change_Percentage_24h_In_Currency).ToList();
                    SortText.Text = "Проценты 24ч △";
                }
                else if (SortText.Text[SortText.Text.Length - 1] == '△')
                {
                    top = topCurrencies;
                    ListFavorites = ListFavoritesCrypto;
                    SortText.Text = "Проценты 24ч ●";
                }
                foreach (TextBlock item in GridNameDataGridColumn.Children)
                {
                    if (item.Text.Split()[0] != "Проценты" && item.Text.Split()[0] != "Монета" && item.Text.Split()[0] != "Переход")
                    {
                        item.Text = item.Text.Replace(item.Text.ToCharArray()[item.Text.Length - 1], '●');
                    }
                }
            }

            DataGridMain.ItemsSource = top;
            if (ListFavoritesCrypto.Count > 0)
            {
                DataGridFavorites.ItemsSource = ListFavorites;
            }
        }

        private async void Click_Favorites(object sender, MouseButtonEventArgs e)
        {
            FontAwesome.WPF.FontAwesome? icon = sender as FontAwesome.WPF.FontAwesome;
            var token = await CoinGeckoApi.GetInfoTokenToID(TokenActiveID, "usd");
            if (icon.Icon == FontAwesome.WPF.FontAwesomeIcon.StarOutline)
            {
                icon.Icon = FontAwesome.WPF.FontAwesomeIcon.Star;

                //// Создаем новую анимацию поворота
                DoubleAnimation FontSizeAnimation = new DoubleAnimation
                {
                    From = 30, // Начальный угол поворота (в градусах)
                    To = 40, // Конечный угол поворота (в градусах)
                    Duration = TimeSpan.FromMilliseconds(200),
                    AutoReverse = true
                };

                // Запускаем анимацию
                icon.BeginAnimation(TextBlock.FontSizeProperty, FontSizeAnimation);

                SQL.AddFavorites(token[CoinGeckoApi.CoinField.Id.ToString().ToLower()].ToString());
            }
            else if (icon.Icon == FontAwesome.WPF.FontAwesomeIcon.Star)
            {
                icon.Icon = FontAwesome.WPF.FontAwesomeIcon.StarOutline;

                //// Создаем новую анимацию поворота
                DoubleAnimation FontSizeAnimation = new DoubleAnimation
                {
                    From = 30, // Начальный угол поворота (в градусах)
                    To = 40, // Конечный угол поворота (в градусах)
                    Duration = TimeSpan.FromMilliseconds(200),
                    AutoReverse = true
                };

                // Запускаем анимацию
                icon.BeginAnimation(TextBlock.FontSizeProperty, FontSizeAnimation);

                SQL.DelFavorites(token[CoinGeckoApi.CoinField.Id.ToString().ToLower()].ToString());
            }

            RefreshFavorites();
        }

        private async void RefreshFavorites()
        {
            try
            {
                Coin[] list = await CoinGeckoApi.GetTokensInfoToIDs(SQL.GetListFavorites());
                ListFavoritesCrypto.Clear();
                foreach (Coin coin in list)
                {
                    if (coin.Price_Change_Percentage_24h_In_Currency < 0)
                    {
                        coin.colorPrice = Brushes.Red;
                    }
                    ListFavoritesCrypto.Add(coin);
                }
                DataGridFavorites.ItemsSource = null;
                DataGridFavorites.ItemsSource = ListFavoritesCrypto;
            }
            catch
            {
                DataGridFavorites.ItemsSource = null;
            }
        }


        private void ColorPickerLine_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (ColorPickerLine != null && ColorPickerLine.SelectedColor != null && mountainRenderSeries != null)
            {
                mountainRenderSeries.Stroke = (Color)ColorPickerLine.SelectedColor;
                Properties.Settings.Default.ColorChartLine = ColorPickerLine.SelectedColor.ToString();
                Properties.Settings.Default.Save();
            }
        }

        private void FavoriteStar_MouseEnter(object sender, MouseEventArgs e)
        {
            var star = sender as FontAwesome.WPF.FontAwesome;
            if (star.Icon == FontAwesome.WPF.FontAwesomeIcon.StarOutline)
            {
                star.Icon = FontAwesome.WPF.FontAwesomeIcon.Star;
            }
            else
            {
                star.Icon = FontAwesome.WPF.FontAwesomeIcon.StarOutline;
            }
        }

        private void FavoriteStar_MouseLeave(object sender, MouseEventArgs e)
        {
            var star = sender as FontAwesome.WPF.FontAwesome;
            if (star.Icon == FontAwesome.WPF.FontAwesomeIcon.StarOutline)
            {
                star.Icon = FontAwesome.WPF.FontAwesomeIcon.Star;
            }
            else
            {
                star.Icon = FontAwesome.WPF.FontAwesomeIcon.StarOutline;
            }
        }

        private void borderClickDataGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            SolidColorBrush expectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4E00AC"));
            if (border.Background is SolidColorBrush && ((SolidColorBrush)border.Background).Color == expectedColor.Color)
            {
                // Ваш код здесь
            }
            else { border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5203B0")); }
        }

        private void borderClickDataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            SolidColorBrush expectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4E00AC"));
            if (border.Background is SolidColorBrush && ((SolidColorBrush)border.Background).Color != expectedColor.Color)
            {
                border.Background = Brushes.Transparent;
            }


        }

        //private void cmbToCurrency_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    lstCrypto.Visibility = Visibility.Collapsed;
        //}

        //private void cmbToCurrency_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    lstCrypto.Visibility = Visibility.Visible;
        //}

        //private void cmbFromCurrency_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    lstCryptoFrom.Visibility = Visibility.Collapsed;
        //}

        //private void cmbFromCurrency_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    lstCryptoFrom.Visibility = Visibility.Visible;
        //}

        private async void converter_click(object sender, RoutedEventArgs e)
        {
            decimal amount;
            if (!decimal.TryParse(txtAmount.Text, out amount) || amount <= 0)
            {
                MessageBorder.Visibility = Visibility.Visible;
                MessageText.Text = ("Выберите колво");
                return;
            }

            if (lstCryptoFrom.SelectedItem == null || lstCrypto.SelectedItem == null)
            {
                MessageBorder.Visibility = Visibility.Visible;
                MessageText.Text = ("Выберите монету");
                return;
            }

            CryptoCurrency fromCurrency = (CryptoCurrency)lstCryptoFrom.SelectedItem;
            CryptoCurrency toCurrency = (CryptoCurrency)lstCrypto.SelectedItem;

            //var dic = await coinGeckoAPI.GetInfoTokenToID("", "usd");

            var fromCurrencyPrice = await CoinGeckoApi.GetInfoTokenToID(fromCurrency.Id, "usd"); // Получить цену в USD
            var toCurrencyPrice = await CoinGeckoApi.GetInfoTokenToID(toCurrency.Id, "usd"); // Получить цену в USD

            decimal? fromPriceCoin = Math.Round(Convert.ToDecimal(fromCurrencyPrice[CoinGeckoApi.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ",")), 6);
            decimal? toCurrencyPruce = Math.Round(Convert.ToDecimal(toCurrencyPrice[CoinGeckoApi.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ",")), 6);

            decimal result = (amount / (decimal)fromPriceCoin) * (decimal)toCurrencyPruce;

            lblResult.Text = $"Резутльат конвертирования: {result}";
        }
    }
}