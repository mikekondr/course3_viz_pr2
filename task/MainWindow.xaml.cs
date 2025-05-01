using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace task
{
    public partial class MainWindow : Window
    {
        private Deck _deckControl; // Контрол для колоди карток

        private Queue<int> to_check; // Черга перевірки відкритих карток

        private Timer _timer; // Таймер для перевірки часу
        private int _time; // Час гри

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _deckControl = new Deck(Viewport); // Ініціалізація контролу колоди
            to_check = new Queue<int>(); // Ініціалізація черги перевірки карток

            _timer = new Timer(1000); // Таймер для перевірки часу
            _timer.AutoReset = true;
            _timer.Elapsed += (s, args) => { _time++; };

            StartNewGame();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame(); // Запуск нової гри
        }

        private void StartNewGame()
        {
            Congrats.Visibility = Visibility.Hidden;
            GameTime.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;

            _deckControl.NewDeck(); // Створення нової колоди карток
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // Затримка у 3 секунди

            };
            timer.Tick += (s, e) =>
            {
                _deckControl.CloseAllCards(); // Закриття всіх карток
                _time = 0;
                _timer.Start(); // Запуск таймера
                timer.Stop(); // Зупинка таймера після виконання
            };
            timer.Start(); // Запускаємо таймер
        }

        private void Viewport3D_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Виконуємо хіт-тест для визначення об'єкта, на який натиснули
            Point mousePosition = e.GetPosition((IInputElement)sender);
            HitTestResult hitResult = VisualTreeHelper.HitTest((Viewport3D)sender, mousePosition);

            if (hitResult is RayHitTestResult rayHitResult)
            {
                // Перевіряємо, чи натиснуто на картку
                if (rayHitResult.VisualHit is ModelVisual3D)
                {
                    var cardName = rayHitResult.VisualHit.GetValue(FrameworkElement.NameProperty) as String;
                    var result = _deckControl.MouseClick(cardName);

                    if (result != -1)
                    {
                        to_check.Enqueue(result); // Додаємо індекс картки до черги перевірки

                        while (to_check.Count >= 2)
                        {
                            var first = to_check.Dequeue(); // Витягуємо перший індекс з черги
                            var second = to_check.Dequeue(); // Витягуємо другий індекс з черги

                            if (_deckControl.Cards[first].Rank == _deckControl.Cards[second].Rank
                                && _deckControl.Cards[first].Suit == _deckControl.Cards[second].Suit)
                            {
                                // Якщо картки однакові, то видаляємо їх з колоди
                                DispatcherTimer timer = new DispatcherTimer
                                {
                                    Interval = TimeSpan.FromSeconds(1.5)
                                };
                                timer.Tick += (s, args) =>
                                {
                                    _deckControl.BoomCards(_deckControl.Cards[first], _deckControl.Cards[second]);
                                    timer.Stop(); // Зупиняємо таймер після виконання

                                    if (_deckControl.OnDeck == 0)
                                    {
                                        _timer.Stop(); // Зупиняємо таймер

                                        GameTime.Text = $"Час гри: {_time} сек"; // Виводимо час гри
                                        GameTime.Visibility = Visibility.Visible; // Показуємо час гри
                                        Congrats.Visibility = Visibility.Visible; // Показуємо привітання
                                        button1.Visibility = Visibility.Visible; // Показуємо кнопку для нової гри
                                    }
                                };
                                timer.Start(); // Запускаємо таймер
                            }
                            else
                            {
                                // Якщо картки різні, закриваємо їх
                                DispatcherTimer timer = new DispatcherTimer
                                {
                                    Interval = TimeSpan.FromSeconds(1.5)
                                };
                                timer.Tick += (s, args) =>
                                {
                                    _deckControl.Cards[first].CloseCard();
                                    _deckControl.Cards[second].CloseCard();
                                    timer.Stop(); // Зупиняємо таймер після виконання
                                };
                                timer.Start(); // Запускаємо таймер
                            }
                        }
                    }

                }
            }
        }
    }

    public class Card
    {
        public string Suit { get; set; } // Масть
        public string Rank { get; set; } // Номінал
        public bool Closed
        {
            get => (rotateTransform.Angle == 180);
        }

        public ModelVisual3D model;
        public AxisAngleRotation3D rotateTransform;

        public Card()
        {

            Suit = Card.RandomSuit(); // Випадкова масть
            Rank = Card.RandomRank(); // Випадковий номінал
            model = new ModelVisual3D() { Content = CreateCardModel() }; // Створюємо нову картку
        }

        public Card(string suit, string rank)
        {
            Suit = suit;
            Rank = rank;
            model = new ModelVisual3D() { Content = CreateCardModel() }; // Створюємо нову картку
        }

        private Model3DGroup CreateCardModel()
        {
            // Створюємо групу моделей  
            var modelGroup = new Model3DGroup();

            // Створюємо геометрію  
            var meshGeometry = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                                   {
                                       new Point3D(-15, -25, 0),
                                       new Point3D(15, -25, 0),
                                       new Point3D(15, 25, 0),
                                       new Point3D(-15, 25, 0)
                                   },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 },
                TextureCoordinates = new PointCollection
                                   {
                                       new System.Windows.Point(0, 1),
                                       new System.Windows.Point(1, 1),
                                       new System.Windows.Point(1, 0),
                                       new System.Windows.Point(0, 0)
                                   }
            };

            // Створюємо матеріал для лицьової сторони  
            var frontMaterial = new DiffuseMaterial
            {
                Brush = new ImageBrush
                {
                    ImageSource = new System.Windows.Media.Imaging.BitmapImage(Image())
                }
            };

            // Створюємо матеріал для зворотної сторони  
            var backMaterial = new DiffuseMaterial
            {
                Brush = new ImageBrush
                {
                    ImageSource = new System.Windows.Media.Imaging.BitmapImage(BackImage())
                }
            };

            // Створюємо модель  
            var geometryModel = new GeometryModel3D
            {
                Geometry = meshGeometry,
                Material = frontMaterial,
                BackMaterial = backMaterial
            };


            // Додаємо модель до групи  
            modelGroup.Children.Add(geometryModel);

            // Створюємо ModelVisual3D  
            return modelGroup;
        }

        public void CloseCard()
        {
            // Ініціалізуємо DispatcherTimer для покрокового обертання
            DispatcherTimer rotationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(25) // Інтервал між кроками
            };

            double currentAngle = rotateTransform.Angle; // Поточний кут обертання
            if (currentAngle == 0)
            {
                rotationTimer.Tick += (s, e) =>
                {
                    if (currentAngle < 180)
                    {
                        currentAngle += 10; // Збільшуємо кут на 10 градусів
                        rotateTransform.Angle = currentAngle; // Оновлюємо кут обертання
                        //rotateTransform.Rotation = cardYaxis; // Застосовуємо обертання
                    }
                    else
                    {
                        rotationTimer.Stop(); // Зупиняємо таймер після досягнення 180 градусів
                    }
                };
            }

            rotationTimer.Start(); // Запускаємо таймер
        }

        public void OpenCard()
        {
            // Ініціалізуємо DispatcherTimer для покрокового обертання
            DispatcherTimer rotationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(25) // Інтервал між кроками
            };

            double currentAngle = rotateTransform.Angle; // Поточний кут обертання
            if (currentAngle == 180)
            {
                rotationTimer.Tick += (s, e) =>
                {
                    if (currentAngle > 0)
                    {
                        currentAngle -= 10; // Зменшуємо кут на 10 градусів
                        rotateTransform.Angle = currentAngle; // Оновлюємо кут обертання
                        //rotateTransform.Rotation = cardYaxis; // Застосовуємо обертання
                    }
                    else
                    {
                        rotationTimer.Stop(); // Зупиняємо таймер після досягнення 180 градусів
                    }
                };
            }

            rotationTimer.Start(); // Запускаємо таймер
        }

        public void MouseClick()
        {
            if (Closed)
                OpenCard();
            else
                CloseCard();
        }

        /// 
        /// Службові
        /// 

        public override string ToString()
        {
            return $"{Card._Ranks[Rank]} - {Card._Suits[Suit]}";
        }

        public System.Uri Image()
        {
            string path = $"pack://application:,,,/img/{Suit.ToLower().Substring(0, 1)}{Rank.ToLower().Substring(0, 1)}.jpg";
            return new System.Uri(path, System.UriKind.Absolute);
        }

        public System.Uri BackImage()
        {
            string path = $"pack://application:,,,/img/back.png";
            return new System.Uri(path, System.UriKind.Absolute);
        }

        /// 
        /// Статичні поля для зберігання мастей та номіналів карт
        /// 

        public static readonly List<string> Suits = new List<string>
        {
            "Hearts",
            "Diamonds",
            "Clubs",
            "Spades"
        };

        public static readonly List<string> Ranks = new List<string>
        {
            "2", "3", "4", "5", "6", "7", "8", "9", "10",
            "Jack", "Queen", "King", "Ace"
        };

        private static readonly Dictionary<string, string> _Suits = new Dictionary<string, string>
        {
            { "Hearts", "Черви" },
            { "Diamonds", "Бубни" },
            { "Clubs", "Трефи" },
            { "Spades", "Піки" }
        };

        private static readonly Dictionary<string, string> _Ranks = new Dictionary<string, string>
        {
            { "2", "2" },
            { "3", "3" },
            { "4", "4" },
            { "5", "П'5" },
            { "6", "6" },
            { "7", "7" },
            { "8", "8" },
            { "9", "9" },
            { "10", "10" },
            { "Jack", "Валет" },
            { "Queen", "Дама" },
            { "King", "Король" },
            { "Ace", "Туз" }
        };

        public static string RandomSuit()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[4];
                rng.GetBytes(randomBytes);
                int s = Math.Abs(BitConverter.ToInt32(randomBytes, 0) % Card.Suits.Count); // Випадкова масть  
                return Suits[s];
            }
        }

        public static string RandomRank()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[4];
                rng.GetBytes(randomBytes);
                int r = BitConverter.ToInt32(randomBytes, 0) % Card.Ranks.Count; // Випадковий номінал  
                r = Math.Abs(r);
                return Ranks[r];
            }
        }
    }

    public class Deck
    {
        const int _deckSize = 6; // Розмір колоди карток

        public List<Card> Cards { get; set; } // Список карток у колоді
        public int OnDeck;

        private Viewport3D _viewport3D; // Вікно для відображення карток

        public Deck(Viewport3D viewport3D)
        {
            Cards = new List<Card>();
            _viewport3D = viewport3D;
        }

        public void NewDeck()
        {
            DropCards(); // Очищення колоди

            for (int i = 0; i < _deckSize; i+=2)
            {
                Card c = new Card(); // Створення нової картки
                Cards.Add(c); // Додавання картки до колоди
                Cards.Add(new Card(c.Suit, c.Rank)); // Додавання другої картки до колоди
                OnDeck += 2;
            }

            ShuffleDeck(); // Перемішування колоди
            PlaceCards(); // Розміщення карток у вікні
        }

        public void ShuffleDeck()
        {
            Random rand = new Random();
            for (int i = Cards.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                Card temp = Cards[i];
                Cards[i] = Cards[j];
                Cards[j] = temp;
            }
        }

        public void PlaceCards()
        {
            for (int i = 0; i < _deckSize; i++)
            { 
                Cards[i].model.SetValue(FrameworkElement.NameProperty, "Card" + (i + 1)); // Встановлюємо ім'я картки

                var cardControl = Cards[i].model as ModelVisual3D;
                if (cardControl != null)
                {
                    var transformGroup = new Transform3DGroup();

                    // Встановлюємо позицію картки
                    var translate = new TranslateTransform3D
                    {
                        OffsetX = (i - (_deckSize / 2)) * 35 + 15,
                        OffsetY = 0,
                        OffsetZ = 0
                    };

                    Cards[i].rotateTransform = new AxisAngleRotation3D
                    {
                        Axis = new Vector3D(0, 1, 0),
                        Angle = 0
                    };

                    var rotate = new RotateTransform3D
                    {
                        Rotation = Cards[i].rotateTransform,
                        CenterX = translate.OffsetX,
                        CenterY = translate.OffsetY,
                        CenterZ = translate.OffsetZ,
                    };

                    transformGroup.Children.Add(translate);
                    transformGroup.Children.Add(rotate);

                    Cards[i].model.Transform = transformGroup; // Застосовуємо трансформацію

                    _viewport3D.Children.Add(cardControl); // Додаємо картку до вікна
                }
            }
        }

        public void FlightOut(Card card)
        {
            var model = card.model;

            DispatcherTimer flightTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // Інтервал між кроками  
            };

            double angle = 0; // Початковий кут для синусоїдальної траєкторії  
            double speed = 3; // Швидкість руху картки  
            double amplitude = 50; // Амплітуда синусоїди  

            var transformGroup = model.Transform as Transform3DGroup;
            var translateTransform = transformGroup?.Children.OfType<TranslateTransform3D>().FirstOrDefault();

            if (translateTransform != null)
            {
                flightTimer.Tick += (s, e) =>
                {
                    angle += 0.1; // Збільшуємо кут для синусоїди  
                    translateTransform.OffsetX += speed; // Рухаємо картку по осі X  
                    translateTransform.OffsetY = amplitude * Math.Sin(angle); // Рухаємо картку по синусоїді  
                    translateTransform.OffsetZ = 2; // Встановлюємо Z координату в 0

                    // Якщо картка виходить за межі видимого поля, зупиняємо таймер  
                    if (translateTransform.OffsetX > _viewport3D.Width)
                    {
                        flightTimer.Stop();
                    }
                };

                flightTimer.Start(); // Запускаємо таймер  
            }
        }


        public void DropCards()
        {
            foreach (Card c in Cards)
            {
                _viewport3D.Children.Remove(c.model); // Видалення картки з вікна
            }
            Cards.Clear(); // Очищення списку карток
            OnDeck = 0;
        }

        public int MouseClick(string cardName)
        {
            int index = int.Parse(cardName.Replace("Card", "")) - 1;
            if (index >= 0 && index < _deckSize && Cards[index].Closed)
            {
                Cards[index].MouseClick();
                return index; // Повертаємо індекс картку
            }

            return -1;
        }

        public void CloseAllCards()
        {
            foreach (var card in Cards)
            {
                if (!card.Closed)
                {
                    card.CloseCard();
                }
            }
        }

        public void BoomCards(Card card1, Card card2)
        {
            FlightOut(card1); // Анімація вильоту картки
            FlightOut(card2); // Анімація вильоту картки
            OnDeck -= 2;
        }
    }
}
