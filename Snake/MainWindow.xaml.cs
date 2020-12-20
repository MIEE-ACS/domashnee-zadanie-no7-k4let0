using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes; 
using System.Windows.Threading;

/* ----------------------------------------Вариант 15 (основной)-------------------------------------------
 *добавлен "комбо-метр"
 *      добавлена переменная модефикатора
 *      в метод обновления экрана добавлено обновление модефикатора счета
 *      при нажатии start теперь также сбрасывается модефикатор счета и значение в ProgressBar
 *      при столкновении с яблоком к переменной счета прибавляется модефикатор (вместо увеличения на 1);
 *          модефикатор увеличивается на 1 (до 10)
 *          значение в ProgressBar устанавливается на 100
 *      вводится дополнительный таймер, запускаемый при столкновении с яблоком
 *          на каждый тик таймера уменьшается значение в Progress Bar;
 *              при достижении нуля сбрасывается модефикатор, таймер останавливается и обновляется экран
 *          в каждом GameOver таймер останавливается
 *-------------------------------------------Вариант 3-------------------------------------------------------
 * добавлено ускорение змеи при подборе яблок
 *      добавлена переменная коэффициента ускорения k
 *      при нажатии Start значение k устанавливается на 1
 *      при подборе яблока значение k умножается на 0.95 и задается новый шаг основного таймера 300*k
 *добавлен бонус замедления
 *      новый класс BonusSlow (наследуется от PositionedEntity, реализация аналогична с Aplle)
 *      метод обновления экрана дополнен бонусом
 *      в методе Apple.move() теперь проводится проверка на несовпадение с бонусом замедления  
 *      в обработчик тика основного таймера добавлен генератор бонусов с бонусом замедления
 *          если бонуса замедления на полк нет
 *              запускается конструктор класса BonusSlow
 *              спрайт бонуса добавляется на поле
 *      при столкновении головы с бонусом 
 *          значение k умножается на 1.25
 *          спрайт бонуса удаляется с поля
 *          список очищается
 * ----------------------------------------------Вариант 1---------------------------------------------------
 * добавлен бонус супер яблоко
 *      добавил метод set для поля image
 *      в класс apple добавлено поле bool отвечающее за то, является ли яблоко супер
 *      в обработчик подбора яблок добавлен генератор супер яблок
 *          заменяеися спрайт
 *          меняется метка супер яблока
 *      время существования супер яблока привязано к таймеру комбо(одинаковое время - 10 с)
 *          при завершении если яблоко супер супер яблоко заменяется обычным и выполняется apple.move()
 *      в обработчик подбора яблок добавлена проверка является ли подобранное яблоко супер
 *          меняется спрайт на обычный
 *          меняется метка супер яблока
 *          счет увеличивается по другой схеме
 *------------------------------------------------Вариант 4---------------------------------------------------
 * добывлены препятствия
 *      новый класс rock по аналогии с другими бонусами
 *          наследование от PositionedEntity, проверка на несовпадение с другими обьектами при генерации
 *      соответствующие правки с проверками для других бонусов
 *      добавлен list для хранения камней, при нажатии start очищается
 *      в генератор бонусов добавлена генерация камней
 *      добавлен обработчик столкновения с камнем: game over
 *      добавлена переменная эффективного счета, изменяется вместе со счетом
 *      при наборе 250000 очков (проверка effectiveScore >= 250)
 *          поле очищается от камней
 *          очищается list с камнями
 *          переменная эффективного счета уменьшается на 250
 *------------------------------------------------Вариант 16--------------------------------------------------
 * добавлен режим минирования
 *      новый класс mine наследуемый от PositionedEntity, реализация как у bodypart но без доп. полей и метод move() не перегружен
 *      метод обноовления экрана дополнен
 *      в классы apple и бонусов добавлена проверка, что обьект не генерируется на мине
 *      добавлены списки мин и мин на удаление
 *      при поедании яблока с вероятностью 20% в клетке хвоста генерируется мина
 *      добавлена проверка столкновения головы с миной
 *          счет уменьшается на 1000*модефикатор комбо (или обнуляется если очков меньше)
 *          мина заносится в список на удаление
 *          после пробега по всему списку мин из него удаляются мины из списка на удаление, затем сам список удаления очищается
 *      при нажатии start очищаются списки мин
 *--------------------------------------------от себя--------------------------------------------------------
 * запрет разворота головы на 180 градусов (как за одно нажатие, так и за несколько нажатий за один тик)
 *      при обработке нажатия клавиш добавлена проверка:
 *          1) что snake что либо содержит (фикс вылета при нажатии клавиш направления до первого нажатия Start)
 *          2) что змейка состоит только из головы или при перемещении голова не попадет в клетку тела сразу за головой(snake[1])
 *      смена head.direction осуществляется при выполнении проверки
 * изменил текстуры змейки
 *      добавил метод обновления спрайтов каждого элемента змеи и включил его в UpdateField
 * теперь основной таймер запускается не с нажатием Start а с нажатием первой клавиши направления после этого (предотвращает появление бонусов до начала движения змеи)
 */

namespace Snake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rand = new Random();

        //Поле на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        //количество очков
        int score;
        int effectiveScore;
        //таймер по которому 
        DispatcherTimer moveTimer;
        //модефикатор счета
        int modifier;
        //таймер комбо
        DispatcherTimer comboTimer;
        //коэффициент ускорения
        double k;
        //бонусы замедления
        List<PositionedEntity> slow;
        //запуск основного таймера
        bool ShouldStartTimer = false;
        //счетчик яблок
        int appleCounter;
        // список препятствий
        List<PositionedEntity> rocks;
        //мины
        List<PositionedEntity> mines;
        List<PositionedEntity> minesToDel;

        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();
            
            snake = new List<PositionedEntity>();
            slow = new List<PositionedEntity>();
            rocks = new List<PositionedEntity>();
            mines = new List<PositionedEntity>();
            minesToDel = new List<PositionedEntity>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

            //создаем таймер срабатывающий раз в 300 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);

            //создаем таймер срабатывающий раз в 100 мс
            comboTimer = new DispatcherTimer();
            comboTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            comboTimer.Tick += new EventHandler(comboTimer_Tick);
        }

        //метод перерисовывающий экран
        private void UpdateField()
        {
            UpdateSnakeSprites();
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
                Canvas.SetTop(p.image, p.y);
                Canvas.SetLeft(p.image, p.x);
            }

            //обновление мин
            foreach (var mine in mines)
            {
                Canvas.SetTop(mine.image, mine.y);
                Canvas.SetLeft(mine.image, mine.x);
            }

            //обновление камней
            foreach (var rock in rocks)
            {
                Canvas.SetTop(rock.image, rock.y);
                Canvas.SetLeft(rock.image, rock.x);
            }

            //обновляем положение яблока
            Canvas.SetTop(apple.image, apple.y);
            Canvas.SetLeft(apple.image, apple.x);

            //обновляем положения бонуса замедления
            if (slow.Count != 0)
            {
                Canvas.SetTop(slow[0].image, slow[0].y);
                Canvas.SetLeft(slow[0].image, slow[0].x);
            }
            
            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);
            //обновляем модефикатор счета
            lbComboModifier.Content = $"x {modifier}";
        }

        //обработчик тика таймера. Все движение происходит здесь
        void moveTimer_Tick(object sender, EventArgs e)
        {
            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи не врезалась в тело
            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == head.x && p.y == head.y)
                {
                    //мы проиграли
                    moveTimer.Stop();
                    comboTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    return;
                }
            }

            //проверяем, что голова змеи не вышла за пределы поля
            if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
            {
                //мы проиграли
                moveTimer.Stop();
                comboTimer.Stop();
                tbGameOver.Visibility = Visibility.Visible;
                return;
            }

            //проверяем, что голова змеи врезалась в яблоко
            if (head.x == apple.x && head.y == apple.y)
            {
                //увеличиваем счет
                if (apple.isSuper)
                {
                    score += 10 * modifier;
                    effectiveScore += 10 * modifier;
                }
                else
                {
                    score += modifier;
                    effectiveScore += modifier;
                }
                //увеличиваем счетчик яблок
                appleCounter++;
                //увеличиваем модефикатор счета
                if (modifier < 10) ++modifier;
                //увеличиваем модефикатор скорости
                k *= 0.95;
                //двигаем яблоко на новое место
                apple.move();
                // добавляем новый сегмент к змее
                var part = new BodyPart(snake.Last());
                canvas1.Children.Add(part.image);
                snake.Add(part);
                //запускаем таймер комбо
                comboTimer.Start();
                pbComboTimer.Value = 100;
                //отключаем супер яблоко
                if (apple.isSuper)
                {
                    apple.isSuper = false;
                    apple.image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/fruit.png") as ImageSource;
                }
                //меняем интервал тика таймера
                moveTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)Math.Round(300 * k, 0));
                //генерируем супер яблоко
                if (appleCounter != 0 && appleCounter % 5 == 0 && rand.Next(2) == 0) 
                {
                    apple.isSuper = true;
                    //заменяем спрайт яблока
                    apple.image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/superfruit.png") as ImageSource;
                }
                //генерируем мину
                if (rand.Next(5) == 0)
                {
                    var mine = new Mine(snake.Last());
                    canvas1.Children.Add(mine.image);
                    mines.Add(mine);
                }
                // очистка препятствий
                if (effectiveScore >= 250)
                {
                    effectiveScore -= 250;
                    foreach (var rock in rocks)
                    {
                        canvas1.Children.Remove(rock.image);
                    }
                    rocks.Clear();
                }
            }

            //проверяем, что голова змеи врезалась в бонус замедления
            if(slow.Count!=0 && head.x==slow[0].x && head.y == slow[0].y)
            {
                //изменяем модефикатор скорости
                k *= 1.25;
                //меняем интервал тика таймера
                moveTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)Math.Round(300 * k, 0));
                //удаляем бонус
                canvas1.Children.Remove(slow[0].image);
                slow.Clear();
            }

            //проверяем что змея врезалась в мину
            foreach (var mine in mines)
            {
                if (head.x == mine.x && head.y == mine.y) 
                {
                    //уменьшаем счет
                    score -= modifier;
                    effectiveScore -= modifier;
                    if (score < 0) score = 0;
                    if (effectiveScore < 0) effectiveScore = 0;
                    //удаляем мину
                    canvas1.Children.Remove(mine.image);
                    minesToDel.Add(mine);
                }
            }
            foreach (var mine in minesToDel)
            {
                mines.Remove(mine);
            }
            minesToDel.Clear();

            //проверяем что змея врезалась в камень
            foreach (var rock in rocks)
            {
                if(rock.x==head.x && rock.y == head.y)
                {
                    //мы проиграли
                    moveTimer.Stop();
                    comboTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    return;
                }
            }

            //генератор бонусов
            switch(rand.Next(20))
            {
                // бонус замедления
                case 0:
                    if (slow.Count == 0)
                    {
                        var bonus = new BonusSlow(snake, mines, rocks, apple);
                        slow.Add(bonus);
                        canvas1.Children.Add(slow[0].image);
                    }
                    break;
                // камень
                case 1:
                    var rock = new Rock(snake, slow, mines, rocks, apple);
                    canvas1.Children.Add(rock.image);
                    rocks.Add(rock);
                    break;
            }
            //перерисовываем экран
            UpdateField();
        }

        //обработчик тика таймера комбо
        void comboTimer_Tick(object sender, EventArgs e)
        {
            pbComboTimer.Value--; 
            //если время закончилось
            if (pbComboTimer.Value == 0)
            {
                //останавливаем таймер
                comboTimer.Stop();
                //сбрасываем модефикатор
                modifier = 1;
                //в случае если супер яблоко на поле
                if (apple.isSuper)
                {
                    //возвращаем обычную текстуру
                    apple.image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/fruit.png") as ImageSource;
                    //перемещаем яблоко
                    apple.move();
                    //убираем метку супер яблока
                    apple.isSuper = false;
                }
                //обновляем экран
                UpdateField();
            }
        }

        // Обработчик нажатия на кнопку клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (snake.Count != 0 && (snake.Count == 1 || snake[1].x != head.x || snake[1].y != head.y - 40))
                        head.direction = Head.Direction.UP;
                    if (ShouldStartTimer)
                    {
                        moveTimer.Start();
                        ShouldStartTimer = false;
                    }
                    break;
                case Key.Down:
                    if (snake.Count != 0 && (snake.Count == 1 || snake[1].x != head.x || snake[1].y != head.y + 40))
                        head.direction = Head.Direction.DOWN;
                    if (ShouldStartTimer)
                    {
                        moveTimer.Start();
                        ShouldStartTimer = false;
                    }
                    break;
                case Key.Left:
                    if (snake.Count != 0 && (snake.Count == 1 || snake[1].x != head.x - 40 || snake[1].y != head.y))
                        head.direction = Head.Direction.LEFT;
                    if (ShouldStartTimer)
                    {
                        moveTimer.Start();
                        ShouldStartTimer = false;
                    }
                    break;
                case Key.Right:
                    if (snake.Count != 0 && (snake.Count == 1 || snake[1].x != head.x + 40 || snake[1].y != head.y))
                        head.direction = Head.Direction.RIGHT;
                    if (ShouldStartTimer)
                    {
                        moveTimer.Start();
                        ShouldStartTimer = false;
                    }
                    break;
            }
        }

        // Обработчик нажатия кнопки "Start"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // обнуляем счет
            score = 0;
            effectiveScore = 0;
            // обнуляем счетчик яблок
            appleCounter = 0;
            // сбрасываем модефикатор счета
            modifier = 1;
            // сбрасываем таймер комбо
            pbComboTimer.Value = 0;
            // сбрасываем коэффициент ускорения
            k = 1;
            // сбрасываем бонус замедления
            slow.Clear();
            // устанавливаем тик таймера движения на 300 мс
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            // обнуляем змею
            snake.Clear();
            //очишаем список мин
            mines.Clear();
            minesToDel.Clear();
            //очишаем препятствия
            rocks.Clear();
            // очищаем канвас
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;
            
            // добавляем поле на канвас
            canvas1.Children.Add(field.image);
            // создаем новое яблоко и добавлем его
            apple = new Apple(snake, mines, rocks, slow);
            canvas1.Children.Add(apple.image);
            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);
            


            //запускаем таймер
            ShouldStartTimer = true;
            UpdateField();

        }
        // обновление спрайтов змеи
        public void UpdateSnakeSprites()
        {
            // тело змеи
            if (snake.Count != 1)
            {
                for(int i = 1; i < snake.Count - 1; ++i)
                {
                    // прямая часть
                    RotateTransform rotateTransform = new RotateTransform(0);
                    snake[i].image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/body.png") as ImageSource;
                    if (snake[i].x == snake[i + 1].x && snake[i].x == snake[i - 1].x) rotateTransform = new RotateTransform(0);
                    else if (snake[i].y == snake[i + 1].y && snake[i].y == snake[i - 1].y) rotateTransform = new RotateTransform(90);
                    else
                    // изгибы
                    {
                        snake[i].image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/corner.png") as ImageSource;
                        if((snake[i].x == snake[i + 1].x && snake[i].y + 40 == snake[i + 1].y && snake[i].x - 40 == snake[i - 1].x && snake[i].y == snake[i - 1].y) ||
                            (snake[i].x == snake[i - 1].x && snake[i].y + 40 == snake[i - 1].y && snake[i].x - 40 == snake[i + 1].x && snake[i].y == snake[i + 1].y))
                            rotateTransform = new RotateTransform(90);
                        if ((snake[i].x == snake[i + 1].x && snake[i].y - 40 == snake[i + 1].y && snake[i].y == snake[i - 1].y && snake[i].x - 40 == snake[i - 1].x) ||
                            (snake[i].x == snake[i - 1].x && snake[i].y - 40 == snake[i - 1].y && snake[i].y == snake[i + 1].y && snake[i].x - 40 == snake[i + 1].x))
                            rotateTransform = new RotateTransform(180);
                        if ((snake[i].x == snake[i + 1].x && snake[i].y - 40 == snake[i + 1].y && snake[i].x + 40 == snake[i - 1].x && snake[i].y == snake[i - 1].y) ||
                            (snake[i].x == snake[i - 1].x && snake[i].y - 40 == snake[i - 1].y && snake[i].x + 40 == snake[i + 1].x && snake[i].y == snake[i + 1].y))
                            rotateTransform = new RotateTransform(270);
                    }
                    snake[i].image.RenderTransformOrigin = new Point(0.5, 0.5);
                    snake[i].image.RenderTransform = rotateTransform;
                }
            }
            // хвост змеи
            if (snake.Count != 1)
            {
                snake[snake.Count - 1].image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/tail.png") as ImageSource;
                RotateTransform rotateTransform = new RotateTransform(0);
                int i = 2;
                // момент добавления элемента, убираем дублирование изображений в последней клетке змеи
                if(snake[snake.Count - 2].x == snake[snake.Count - 1].x &&
                    snake[snake.Count - 2].y == snake[snake.Count - 1].y)
                {
                    if(snake.Count==2) snake[1].image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/empty.png") as ImageSource;
                    else
                    {
                        i = 3;
                        snake[snake.Count - 2].image.Source = (new ImageSourceConverter()).ConvertFromString("pack://application:,,,/Resources/empty.png") as ImageSource;
                    }
                }
                //направление хвоста
                if (snake[snake.Count - i].x == snake[snake.Count - 1].x &&
                    snake[snake.Count - i].y + 40 == snake[snake.Count - 1].y)
                {
                    rotateTransform = new RotateTransform(90);
                }
                if (snake[snake.Count - i].x == snake[snake.Count - 1].x &&
                    snake[snake.Count - i].y - 40 == snake[snake.Count - 1].y)
                {
                    rotateTransform = new RotateTransform(270);
                }
                if (snake[snake.Count - i].y == snake[snake.Count - 1].y &&
                    snake[snake.Count - i].x - 40 == snake[snake.Count - 1].x)
                {
                    rotateTransform = new RotateTransform(180);
                }
                snake[snake.Count - 1].image.RenderTransformOrigin = new Point(0.5, 0.5);
                snake[snake.Count - 1].image.RenderTransform = rotateTransform;
            }
        }

        
        public class Entity
        {
            protected int m_width;
            protected int m_height;
            
            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
                set
                {
                    m_image = value;
                }
            }
        }

        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }

        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            List<PositionedEntity> m_mines;
            List<PositionedEntity> m_bonusSlow;
            List<PositionedEntity> m_rock;
            public bool isSuper;
            public Apple(List<PositionedEntity> s, List<PositionedEntity> m, List<PositionedEntity> slow , List<PositionedEntity> r)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
            {
                m_snake = s;
                m_mines = m;
                m_bonusSlow = slow;
                m_rock = r;
                isSuper = false;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var mine in m_mines)
                    {
                        if (mine.x == x && mine.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var bonus in m_bonusSlow)
                    {
                        if (bonus.x == x && bonus.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var p in m_rock)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
                    image.RenderTransform = rotateTransform;
                }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }
            }
        }

        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
        }
        public class BonusSlow : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            List<PositionedEntity> m_rock;
            List<PositionedEntity> m_mines;
            Apple m_apple;
            public BonusSlow(List<PositionedEntity> s, List<PositionedEntity> m, List<PositionedEntity> r, Apple a)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/bonusSlow.png")
            {
                m_snake = s;
                m_mines = m;
                m_apple = a;
                m_rock = r;
                move();
            }
            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var p in m_mines)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var p in m_rock)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    if (m_apple.x == x && m_apple.y == y) overlap = true;
                    if (!overlap)
                        break;
                } while (true);

            }
        }
        public class Rock : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            List<PositionedEntity> m_slow;
            List<PositionedEntity> m_mines;
            List<PositionedEntity> m_rock;
            Apple m_apple;
            public Rock(List<PositionedEntity> s, List<PositionedEntity> slow, List<PositionedEntity> m, List<PositionedEntity> r, Apple a)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/Rock.png")
            {
                m_snake = s;
                m_slow = slow;
                m_mines = m;
                m_rock = r;
                m_apple = a;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var bonus in m_slow)
                    {
                        if (bonus.x == x && bonus.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var p in m_rock)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    foreach (var p in m_mines)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    if (m_apple.x == x && m_apple.y == y) overlap = true;
                    if (!overlap)
                        break;
                } while (true);
            }
        }
        public class Mine : PositionedEntity
        {
            public Mine(PositionedEntity last)
                : base(last.x, last.y, 40, 40, "pack://application:,,,/Resources/mine.png") { }
        }
    }
}
